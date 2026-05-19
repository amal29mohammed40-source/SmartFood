namespace SmartFood.Models;

public class MealPlanner
{
    public Guid Id { get; set; }

    public string UserEmail { get; set; } = "";

    public string DayName { get; set; } = "";

    public string MealType { get; set; } = "";

    public Guid RecipeId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Recipe? Recipe { get; set; }
}