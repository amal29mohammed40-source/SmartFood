namespace SmartFood.Models;

public class Favorite
{
    public Guid Id { get; set; }

    public string UserEmail { get; set; } = "";

    public Guid RecipeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Recipe? Recipe { get; set; }
}