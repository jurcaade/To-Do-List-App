using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

var app = builder.Build();

const string dataFile = "tasks.json";
List<TaskItem> tasks = File.Exists(dataFile)
    ? JsonSerializer.Deserialize<List<TaskItem>>(File.ReadAllText(dataFile)) ?? new()
    : new();

void SaveTasks() => File.WriteAllText(dataFile, JsonSerializer.Serialize(tasks));


app.MapGet("/api/tasks", () => tasks);

app.MapPost("/api/tasks", (TaskItem task) =>
{
    task.Id = tasks.Any() ? tasks.Max(t => t.Id) + 1 : 1;
    tasks.Add(task);
    SaveTasks();
    return Results.Created($"/api/tasks/{task.Id}", task);
});

app.MapPut("/api/tasks/{id}", (int id, TaskItem updatedTask) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return Results.NotFound();
    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;
    SaveTasks();
    return Results.Ok(task);
});

app.MapDelete("/api/tasks/{id}", (int id) =>
{
    var task = tasks.FirstOrDefault(t => t.Id == id);
    if (task is null) return Results.NotFound();
    tasks.Remove(task);
    SaveTasks();
    return Results.NoContent();
});

app.UseDefaultFiles(); 
app.UseStaticFiles();

app.Run();
