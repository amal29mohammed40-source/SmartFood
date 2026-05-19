using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFood.Data;
using SmartFood.Models;

namespace SmartFood.Controllers;

public class AiController : Controller
{
    private readonly AppDbContext _context;

    public AiController(AppDbContext context)
    {
        _context = context;
    }

    // 1. عرض الصفحة فارغة في البداية
    public IActionResult Index()
    {
        return View();
    }

    // 2. معالجة الطلب وإرجاع النتيجة الذكية كـ JSON لمنع إعادة تحميل الصفحة
    [HttpPost]
    public async Task<IActionResult> Suggest([FromBody] AiSuggestionRequest request)
    {
        if (request == null)
        {
            return Json(new { success = false, message = "بيانات الطلب غير صالحة." });
        }

        // تسجيل الطلب في قاعدة البيانات للتحليلات مستقبلاً
        _context.AiRequests.Add(new AiRequestLog
        {
            Weight = request.Weight,
            Goal = request.Goal,
            TargetCalories = request.TargetCalories,
            MaxCookingTime = 0, 
            Ingredients = request.Ingredients,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        // التعديل هنا: استخدام الأحرف الكبيرة (Goal و Calories) لتطابق الـ Model في الـ C#
        var sortedRecipes = await _context.Recipes
            .AsNoTracking()
            .Where(r => r.Goal == request.Goal && r.Calories <= request.TargetCalories)
            .OrderByDescending(r => r.Calories)
            .Take(12) 
            .Select(r => new
            {
                id = r.Id,
                title = r.Title,
                description = r.Description,
                imageUrl = r.ImageUrl ?? "https://images.unsplash.com/photo-1490645935967-10de6ba17061?auto=format&fit=crop&w=500&q=80",
                cookingTime = r.CookingTime,
                calories = r.Calories,
                protein = r.Protein,
                carbs = r.Carbs,
                fat = r.Fat,
                score = 0 
            })
            .ToListAsync();

        return Json(new { success = true, data = sortedRecipes });
    }
}