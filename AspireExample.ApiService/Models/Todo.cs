using System.ComponentModel.DataAnnotations.Schema;
using AspireExample.ApiService.Dtos;

namespace AspireExample.ApiService.Models;

public class Todo
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsCompleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public TodoDto ToDto()
    {
        return new TodoDto
        {
            Id = Id,
            Title = Title,
            Description = Description,
            IsCompleted = IsCompleted,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
