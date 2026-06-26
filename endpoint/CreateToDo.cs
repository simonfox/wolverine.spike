using JasperFx.MultiTenancy;
using Wolverine.Http;
using Wolverine.Persistence;

namespace endpoint
{
    public record ToDoCreated(Guid Id) { };
    public record ToDoCreatedResponse(Guid Id) : CreationResponse($"api/todos/{Id}");

    public static class CreateToDo
    {
        [WolverinePost("api/todos")]
        public static(ToDoCreatedResponse, Insert<ToDoRecord>, ToDoCreated) Post(object body, TenantId tenantId)
        {
            Guid id = Guid.CreateVersion7();
            ToDoRecord todo = new() { Id = id, Description = $"To do created {id} for {tenantId}" };
            return (new ToDoCreatedResponse(id), Storage.Insert(todo), new ToDoCreated(id));
        }
    }
}
