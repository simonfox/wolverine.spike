using endpoint;
using JasperFx;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.Http;
using Wolverine.SqlServer;
using Wolverine.SqlServer.Transport.NServiceBus;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string transportConnectionString = builder.Configuration.GetConnectionString("Transport") ?? throw new InvalidOperationException("Connection string 'Transport' is required.");

builder.Host.UseWolverine(opts =>
{
    opts.UseSqlServerPersistenceAndTransport(transportConnectionString);
    opts.PersistMessagesWithSqlServer(transportConnectionString)
        .UseMasterTableTenancy(tenants =>
        {
            tenants.Register("tenant_one", "Server=localhost;Database=tenant_one;Trusted_Connection=True;TrustServerCertificate=True");
        });
    opts.UseNServiceBusSqlServerInterop(autoProvision: true);
    opts.Publish(x =>
    {
        x.Message<ToDoCreated>();
        x.ToSqlServerQueue("test_wolverine");
        x.ToNServiceBusSqlServerQueue("test_nsb");
    });   
    opts.Services.AddDbContextWithWolverineManagedMultiTenancy<ToDoDbContext>(
        (dbContextBuilder, connectionString, _) =>
        {
            dbContextBuilder.UseSqlServer(
                connectionString.Value,
                sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        },
        AutoCreate.CreateOrUpdate);
    opts.UseEntityFrameworkCoreWolverineManagedMigrations();
    opts.Policies.AutoApplyTransactions();
    opts.UseRuntimeCompilation();
});

builder.Services.AddWolverineHttp();

WebApplication app = builder.Build();

app.MapWolverineEndpoints(opts =>
{
    opts.TenantId.IsRequestHeaderValue("tenant");
});

// RunJasperFxCommands enables the Wolverine/Weasel CLI:
//   dotnet run -- resources setup   -> provision Wolverine message store tables
//   dotnet run -- db-assert         -> verify schema is current
// When run without CLI args it behaves identically to app.RunAsync()
return await app.RunJasperFxCommands(args);
