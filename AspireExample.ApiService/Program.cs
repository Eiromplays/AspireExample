using AspireExample.ApiService;
using AspireExample.ApiService.Database;
using AspireExample.ApiService.Dtos;
using AspireExample.ApiService.Models;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

builder.AddRedisOutputCache("cache");
builder.AddNpgsqlDbContext<ApiDbContext>("apidb");

// Add services to the container.
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseOutputCache();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

app.MapGet("/todos", async (ApiDbContext dbContext) =>
    {
        var todos = await dbContext.Todos
            .Select(todo => todo.ToDto())
            .ToListAsync();

        return Results.Ok(todos);
    })
    .CacheOutput(policyBuilder =>
    {
        policyBuilder.Expire(TimeSpan.FromMinutes(1));
        policyBuilder.Tag("todos");
    });

app.MapGet("/todos/{id:int}", async (int id, ApiDbContext dbContext) =>
    {
        var todo = await dbContext.Todos.FindAsync(id);
        if (todo is null)
            return Results.NotFound();

        return Results.Ok(todo.ToDto());
    })
    .CacheOutput(policyBuilder =>
    {
        policyBuilder.Expire(TimeSpan.FromMinutes(1));
        policyBuilder.SetVaryByRouteValue("id");
    });

app.MapPost("/todos", async (TodoDto todoDto, ApiDbContext dbContext, IOutputCacheStore outputCacheStore) =>
{
    var todo = new Todo
    {
        Title = todoDto.Title,
        Description = todoDto.Description,
        IsCompleted = todoDto.IsCompleted,
        CreatedAt = DateTimeOffset.UtcNow,
        UpdatedAt = DateTimeOffset.UtcNow
    };

    dbContext.Todos.Add(todo);
    await dbContext.SaveChangesAsync();

    await outputCacheStore.EvictByTagAsync("todos", default);

    return Results.Created($"/todos/{todo.Id}", todo.ToDto());
});

app.MapDelete("/todos/{id:int}", async (int id, ApiDbContext dbContext, IOutputCacheStore outputCacheStore) =>
{
    var todo = await dbContext.Todos.FindAsync(id);
    if (todo is null)
        return Results.NotFound();

    dbContext.Todos.Remove(todo);
    await dbContext.SaveChangesAsync();

    await outputCacheStore.EvictByTagAsync("todos", default);
    return Results.NoContent();
});

app.MapDefaultEndpoints();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
var dbCreator = dbContext.GetService<IRelationalDatabaseCreator>();

var strategy = dbContext.Database.CreateExecutionStrategy();
await strategy.ExecuteAsync(async () =>
{
    // Create the database if it does not exist.
    // Do this first so there is then a database to start a transaction against.
    if (!await dbCreator.ExistsAsync())
    {
        await dbCreator.CreateAsync();
    }

    // Run migration in a transaction to avoid partial migration if it fails.
    await using var transaction = await dbContext.Database.BeginTransactionAsync();
    await dbContext.Database.MigrateAsync();
    await transaction.CommitAsync();
});

app.Run();
