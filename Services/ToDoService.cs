using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using ToDoGrpc.Data;
using ToDoGrpc.Models;

namespace ToDoGrpc.Services;

public class ToDoService : ToDoIt.ToDoItBase
{
    private readonly ApplicationDbContext _context;

    public ToDoService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async override Task<CreateTodoResponse> CreateTodo(CreateTodoRequest request, ServerCallContext context)
    {
        if (string.IsNullOrEmpty(request.Title) || string.IsNullOrEmpty(request.Description))
            throw new RpcException(new Status(StatusCode.InvalidArgument, "you must supply a valid object."));
        var todoItem = new ToDoItem
        {
            Title = request.Title,
            Description = request.Description,
        };
        await _context.AddAsync(todoItem);
        await _context.SaveChangesAsync();
        return await Task.FromResult(new CreateTodoResponse { Id = todoItem.Id });
    }
    public async override Task<ReadToDoResponse> ReadTodo(ReadToDoRequest request, ServerCallContext context)
    {
        var todoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if (todoItem is null)
            throw new RpcException(new Status(StatusCode.NotFound, "can not find a record with this id"));
        return await Task.FromResult(new ReadToDoResponse
        {
            Id = todoItem.Id,
            Title = todoItem.Title,
            Description = todoItem.Description,
            ToDoStatus = todoItem.ToDoStatus
        });
    }
    public async override Task<GetAllResponse> ListTodo(GetAllRequest request, ServerCallContext context)
    {
        var response = new GetAllResponse();
        var todoItems = await _context.ToDoItems.ToListAsync();
        foreach (var todoItem in todoItems)
        {
            response.ToDo.Add(new ReadToDoResponse
            {
                Id = todoItem.Id,
                Title = todoItem.Title,
                Description = todoItem.Description,
                ToDoStatus = todoItem.ToDoStatus
            });
        }
        return await Task.FromResult(response);
    }
    public override async Task<UpdateToDoResponse> UpdateTodo(UpdateToDoRequest request, ServerCallContext context)
    {
        var todoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if (todoItem is null)
            throw new RpcException(new Status(StatusCode.NotFound, "this todo item is not found"));
        todoItem.Title = request.Title;
        todoItem.Description = request.Description;
        todoItem.ToDoStatus = request.ToDoStatus;
        todoItem.Id = request.Id;
        await _context.SaveChangesAsync();
        return await Task.FromResult(new UpdateToDoResponse { Id = request.Id });
    }
    public override async Task<DeleteToDoResponse> DeleteTodo(DeleteToDoRequest request, ServerCallContext context)
    {
        if (request.Id <= 0)
            throw new RpcException(new Status(StatusCode.InvalidArgument, "the id must be greater than 0."));
        var todoItem = await _context.ToDoItems.FirstOrDefaultAsync(t => t.Id == request.Id);
        if (todoItem is null)
            throw new RpcException(new Status(StatusCode.NotFound, "this todo item is not found"));
        _context.Remove(todoItem);
        await _context.SaveChangesAsync();
        return await Task.FromResult(new DeleteToDoResponse { Id = request.Id });
    }
}
