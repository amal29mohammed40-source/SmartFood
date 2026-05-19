namespace SmartFood.Models;

public class AiRequestLog
{
    public Guid Id { get; set; }
    public int Weight { get; set; }
    public string Goal { get; set; } = "";
    public int TargetCalories { get; set; }
    public int MaxCookingTime { get; set; }
    public string? Ingredients { get; set; }
    public DateTime CreatedAt { get; set; }
}