using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace endpoint;

public class ToDoDbContext(DbContextOptions<ToDoDbContext> options) : DbContext(options)
{
    public DbSet<ToDoRecord> ToDos { get; set; }
}

public class ToDoRecord
{
    public Guid Id { get; set; }
    public string Description { get; set; }

    public class Mapping : IEntityTypeConfiguration<ToDoRecord>
    {
        public void Configure(EntityTypeBuilder<ToDoRecord> builder)
        {
            builder.ToTable("ToDo", "dbo");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();
            builder.Property(x => x.Description).HasMaxLength(100);
        }
    }
}

