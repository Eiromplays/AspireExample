namespace AspireExample.Web;

public class TodoApiClient(HttpClient httpClient)
{
    public async Task<TodoDto[]> GetTodosAsync(int maxItems = 10, CancellationToken cancellationToken = default)
    {
        List<TodoDto>? todos = null;

        await foreach (var todo in httpClient.GetFromJsonAsAsyncEnumerable<TodoDto>("/todos", cancellationToken))
        {
            if (todos?.Count >= maxItems)
            {
                break;
            }
            if (todo is not null)
            {
                todos ??= [];
                todos.Add(todo);
            }
        }

        return todos?.ToArray() ?? [];
    }

    public async Task<TodoDto?> GetTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TodoDto>($"/todos/{id}", cancellationToken);
    }

    public async Task<TodoDto?> CreateTodoAsync(TodoDto todo, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/todos", todo, cancellationToken);

        return await response.Content.ReadFromJsonAsync<TodoDto>(cancellationToken: cancellationToken);
    }

    public async Task DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        await httpClient.DeleteAsync($"/todos/{id}", cancellationToken);
    }
}

public record TodoDto
{
    public int Id { get; set; }

    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsCompleted { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}