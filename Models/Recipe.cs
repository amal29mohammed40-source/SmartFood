namespace SmartFood.Models;

public class Recipe
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Ingredients { get; set; } = "";
    public string? ImageUrl { get; set; }
    public int CookingTime { get; set; }
    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbs { get; set; }
    public int Fat { get; set; }
    public string Goal { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}