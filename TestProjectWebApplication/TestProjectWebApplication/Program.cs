using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using System;
using TestProjectWebApplication;

//�������:������ ����������� id ������� ������� ��������?
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
//builder.Services.AddScoped<IUserRepository, UserRepository>();

object value = builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

//�������� �������� � ���������///////////////////////////////////
//GET-������
//app.MapGet("/",() => Console.WriteLine("Hello World!"));//��� ��������, ������ ������� �� ����� ������. ������ ����� ���� � ������ Console
app.MapGet("/", async (ILogger<Program> logger, HttpResponse response) =>//+++++++
{
    await response.WriteAsync("Hello World");
    logger.LogInformation("Project Launched!!!");//����� � ����� ������ ����.
});
app.MapGet("/todoitems", async (TodoDb db) => { await db.Todos.ToListAsync(); });
app.MapGet("/todoitems/completes", async (TodoDb db, ILogger <Program> logger) => {
    //curl -X GET http://localhost:5091/todoitems/completes
    var completes = await db.Todos.Where(t => t.IsComplete).ToListAsync();

    if (!completes.Any())
        return Results.NotFound("No incomplete todo items found.");
    return Results.Ok(completes);
});
app.MapGet("/todoitems/completes/{id}", async (int id, TodoDb db, ILogger<Program> logger) =>//+++++++
//curl -X GET http://localhost:5091/todoitems/complete/1                                                                               
{
    try
    {
        var todo = await db.Todos.FindAsync(id);
        return todo is not null ? Results.Ok(todo) : Results.NotFound();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving todo item with id {Id}", id);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
});
app.MapGet("/todoitems/falses", async (TodoDb db, ILogger<Program> logger) =>
{//curl -X GET http://localhost:5091/todoitems/falses 
    var falses = await db.Todos.Where(t => t.IsComplete == false).ToListAsync();

    if (!falses.Any())
        return Results.NotFound("No incomplete todo items found");
    return Results.Ok(falses);
});
app.MapGet("/todoitems/falses/{id}", async (int id, TodoDb db, ILogger <Program> logger) => //+++++++
{   //curl -X GET http://localhost:5091/todoitem/falses/{id}
    try
    {
        var todo = await db.Todos.FindAsync(id);
        return todo is not null ? Results.Ok(todo) : Results.NotFound();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Error retrieving todo item with id {Id}", id);
        return Results.StatusCode(StatusCodes.Status500InternalServerError);
    }
});
//POST-�������
//���������: ��������� �� �������� ��������
app.MapPost("/todoitems/completes", async (int? id, Todo todo, TodoDb db) =>
{ //curl -X POST "http://localhost:5091/todoitems/completes" -H "Content-Type: application/json" -d "{\"id\":2, \"Name\":\"Task\", \"IsComplete\":true}"
    
});
app.MapPost("/todoitems/completes/{id}", async (int? id, Todo todo, TodoDb db) =>
{ //curl -X POST "http://localhost:5091/todoitems/completes/1" -H "Content-Type: application/json" - d "{\"id\":2, \"Name\":\"Task\", \"IsComplete\":true}"
    if (id.HasValue)
    {
        var existTodo = await db.Todos.FindAsync(id.Value);
        if (existTodo == null)
        {
            return Results.NotFound();
        }

        existTodo.Name = todo.Name;
        existTodo.IsComplete = todo.IsComplete;
        await db.SaveChangesAsync();
        return Results.Ok(existTodo);
    }
    else
    {
        db.Todos.Add(todo);
        await db.SaveChangesAsync(); //��� ��� ���������.
        return Results.Created($"/todoitems/completes/{todo.Id}", todo);
    }
});
app.MapPost("/todoitems/falses", async (Todo todo, TodoDb db) =>
{//curl -X POST "http://localhost:5091/todoitems/falses" -H "Content-Type: application/json" -d "{\"Name\":\"FTask\", \"IsComplete\":false}"
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/falses/{todo.Id}", todo);
});
app.MapPost("/todoitems/falses/{id}", async (Todo todo, TodoDb db) =>
{//curl -X POST "http://localhost:5091/todoitems/falses/1" -H "Content-Type: application/json" -d "{\"Name\":\"FTask\", \"IsComplete\":false}" //��� id � curl �� ���������
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/falses/{todo.Id}", todo);
});

//������-��������
app.MapDelete("/todoitems/completes/{id}", async (int id, TodoDb db) =>
{   //curl -X DELETE "http://localhost:5091/todoitems/completes/{id}"
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});
app.MapDelete("/todoitems/falses/{id}", async (int id, TodoDb db) =>
{   //curl -X DELETE "http://localhost:5091/todoitems/falses/{id}"
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});
//app.MapGet("/todoitems/{id}/completes/{subid}", async (int id, int subid, TodoDb db) =>
//{

//    // ���������� ��������� ���������
//});

app.MapPost("/todoitems/{id}/completes/{subid}", async (int id, int subid, TodoDb db, Todo todo) =>
{ //curl -X POST "http://todoitems/{id}/completes/{subid}"
    todo.Id = id;
    todo.SubId = subid;
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}/completes/{todo.SubId}", todo);
});
app.Run(); //"�������"