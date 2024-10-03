using AspireExample.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace AspireExample.ApiService.Database;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Todo>()
            .HasData(new Todo
            {
                Id = 1,
                Title = "Test",
                Description = "This is a test",
                IsCompleted = false,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            }, new Todo
            {
                Id = 2,
                Title = "Test 2",
                Description = "This is a test 2",
                IsCompleted = true,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
            });
    }
}