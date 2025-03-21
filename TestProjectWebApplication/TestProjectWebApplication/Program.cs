using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using System;
using TestProjectWebApplication;

//Вопросы:Почему id в подкатологах 1го католога сквозное а не обособленное друг от друга
var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
object value = builder.Services.AddDatabaseDeveloperPageExceptionFilter();
var app = builder.Build();

app.Logger.LogInformation("Adding Routes");
//Корневые катологи и категории///////////////////////////////////
//GET-ЗАПРОС

//app.MapGet("/",() => Console.WriteLine("Hello World!"));//Нет формочек, видимо поэтому не видно строку. Скорее всего дело в классе Console

app.MapGet("/", async (ILogger<Program> logger, HttpResponse response) =>
{
    logger.LogInformation("Project Launched!!!");
    await response.WriteAsync("Hello World");
});
app.Logger.LogInformation("Starting the app");
//Модуль(ёк) "Логирование"
//builder.Logging.AddSimpleConsole(i => i.ColorBehavior = LoggerColorBehavior.Disabled);
app.MapGet("/Test", async (ILogger<Program> logger, HttpResponse response) =>
{
    logger.LogInformation("Testing logging in Program.cs");
    await response.WriteAsync("Simple Response");
});
//////////////////////////
app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.ToListAsync());
//Подкатологи/////////////////////////
app.MapGet("/todoitems/completes", async (ILogger < Program > logger, HttpResponse response, TodoDb db) => { //curl GET http://localhost:5091/todoitems/completes
    await response.WriteAsync("All Completes:\n");
    logger.LogInformation($"Gained access to completes");
    await db.Todos.Where(t => t.IsComplete).ToListAsync();
    });
app.MapGet("/todoitems/completes/{id}", async (int id, TodoDb db) => //curl GET http://localhost:5091/todoitems/complete/1 
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(todo)
            : Results.NotFound());
app.MapGet("/todoitems/falses", async (ILogger < Program > logger, HttpResponse response, TodoDb db) =>
{//curl GET http://localhost:5091/todoitems/falses 
    await response.WriteAsync("All Falses:\n");
    logger.LogInformation($"Gained access to falses");
    await db.Todos.Where(t => t.IsComplete == false).ToListAsync();
}); 
app.MapGet("/todoitems/subitems/{subid}", async (int subid, TodoDb db) => //curl GET http://localhost:5091/subitems/1 
    await db.Todos.FindAsync(subid)
    is Todo todo
    ? Results.Ok(todo)
    : Results.NotFound());
//POST-запросы
app.MapPost("/todoitems/completes", async (Todo todo, TodoDb db) => //curl -X POST "http://localhost:5091/todoitems/completes" -H "Content-Type: application/json" -d "{\"Name\":\"Task\", \"IsComplete\":true}"
{
    //curl -X POST "http://localhost:5091/todoitems/completes" -H "Content-Type: application/json" -d "{\"id\":2, \"Name\":\"Task\", \"IsComplete\":true}"
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/completes/{todo.Id}", todo); // конечная точка /todoitems HTTP POST, которая добавляет данные в базу данных в памяти
});
app.MapPost("/todoitems/completes/{id}", async (Todo todo, TodoDb db) => //curl -X POST "http://localhost:5091/todoitems/completes/1" -H "Content-Type: application/json" -d "{\"Name\":\"Task\", \"IsComplete\":true}"
{
    //curl - X POST "http://localhost:5091/todoitems/completes/1" - H "Content-Type: application/json" - d "{\"id\":2, \"Name\":\"Task\", \"IsComplete\":true}"
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/completes/{todo.Id}", todo); // конечная точка /todoitems HTTP POST, которая добавляет данные в базу данных в памяти
});
app.MapPost("/todoitems/falses", async (Todo todo, TodoDb db) => //curl -X POST "http://localhost:5091/todoitems/falses" -H "Content-Type: application/json" -d "{\"Name\":\"FTask\", \"IsComplete\":false}"
{ //Протестуровать как с id так и без id - не работает без id curl
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/falses/{todo.Id}", todo);
});
app.MapPost("/todoitems/falses/{id}", async (Todo todo, TodoDb db) => //curl -X POST "http://localhost:5091/todoitems/falses/1" -H "Content-Type: application/json" -d "{\"Name\":\"FTask\", \"IsComplete\":false}"
{ //без id в curl не сработает
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/falses/{todo.Id}", todo);
});
//PUT-запросы 
app.MapPut("/todoitems/completes/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    //curl -X PUT "http://localhost:5091/todoitems/completes/{id}" -H "Content-Type: application/json" -d "{\"Name\":\"Updated task\", \"IsComplete\":false}"
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});
app.MapPut("/todoitems/falses/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    //curl -X PUT "http://localhost:5091/todoitems/false" -H "Content-Type: application/json" -d "{\"Name\":\"Updated task\", \"IsComplete\":false}"
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
});
//Запрос-Удаление
app.MapDelete("/todoitems/completes/{id}", async (int id, TodoDb db) =>
{
    //curl -X DELETE "http://localhost:5000/todoitems/false/{id}"
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    return Results.NotFound();
});
app.MapDelete("/todoitems/falses/{id}", async (int id, TodoDb db) =>
{
    //curl -X DELETE "http://localhost:5000/todoitems/false/{id}"
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

//    // Возвращаем найденную подзадачу
//});
//Просто чтобы был маршрут по 2 идентификаторам. работает......
app.MapPost("/todoitems/{id}/completes/{subid}", async (int id, int subid, TodoDb db, Todo todo) =>
{
    todo.Id = id;
    todo.SubId = subid;
    db.Todos.Add(todo);
    await db.SaveChangesAsync();
    return Results.Created($"/todoitems/{todo.Id}/completes/{todo.SubId}", todo);
});
app.Run(); //"Слушает"