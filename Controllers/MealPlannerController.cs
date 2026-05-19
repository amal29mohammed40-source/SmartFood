using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFood.Data;
using SmartFood.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFood.Controllers;

// كائن ذكي لتمرير البيانات منسقة وجاهزة للواجهة الإحترافية
public class MealPlannerViewModel
{
    // قاموس يربط (اليوم، نوع الوجبة) بالوجبة المخططة لسرعة بحث O(1)
    public Dictionary<(string Day, string Type), MealPlanner> ScheduledMeals { get; set; } = new();
    
    // قاموس يحتوي على مجموع السعرات لكل يوم
    public Dictionary<string, int> DailyCalories { get; set; } = new();
    
    // قاموس يحتوي على مجموع البروتين، الكربوهيدرات، والدهون لكل يوم
    public Dictionary<string, (int Protein, int Carbs, int Fat)> DailyMacros { get; set; } = new();
}

public class MealPlannerController : Controller
{
    private readonly AppDbContext _context;

    // الأيام والأنواع الثابتة لضمان تطابق البيانات
    private readonly string[] Days = { "Saturday", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };
    private readonly string[] MealTypes = { "Breakfast", "Lunch", "Dinner", "Snack" };

    public MealPlannerController(AppDbContext context)
    {
        _context = context;
    }

    // 1. عرض جدول التخطيط مع حسابات الـ Macros ذكياً بحسب المستخدم الحالي
    public async Task<IActionResult> Index()
    {
        // 🔍 جلب بريد المستخدم الحالي النشط من الـ Session
        string? userEmail = HttpContext.Session.GetString("UserEmail");

        // 🔒 حماية: إذا لم تسجل دخولها، يتم توجيهها فوراً لصفحة الـ Login
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Index", "Login");
        }

        // 🎯 جلب الوجبات المرتبطة بإيميل المستخدم الحالي فقط
        var meals = await _context.MealPlanners
            .AsNoTracking()
            .Include(m => m.Recipe)
            .Where(m => m.UserEmail == userEmail)
            .ToListAsync();

        var viewModel = new MealPlannerViewModel();

        // تنظيم الوجبات في القاموس لسهولة استدعائها بالـ View لضمان عدم التداخل
        viewModel.ScheduledMeals = meals
            .Where(m => m.Recipe != null && m.DayName != null && m.MealType != null)
            .GroupBy(m => (m.DayName, m.MealType))
            .ToDictionary(g => g.Key, g => g.First());

        // حساب السعرات والمغذيات لكل يوم بشكل مسبق للمستخدم الحالي فقط
        foreach (var day in Days)
        {
            var dayMeals = meals.Where(m => m.DayName == day && m.Recipe != null).ToList();
            
            int totalCal = dayMeals.Sum(m => m.Recipe!.Calories);
            int totalProtein = dayMeals.Sum(m => m.Recipe!.Protein);
            int totalCarbs = dayMeals.Sum(m => m.Recipe!.Carbs);
            int totalFat = dayMeals.Sum(m => m.Recipe!.Fat);

            viewModel.DailyCalories[day] = totalCal;
            viewModel.DailyMacros[day] = (totalProtein, totalCarbs, totalFat);
        }

        return View(viewModel);
    }

    // 2. دالة GET لفتح صفحة اختيار الوصفة عند الضغط على (+) مع فحص الأمان
    [HttpGet]
    public async Task<IActionResult> Create(string day, string type)
    {
        // 🔍 التحقق من حالة تسجيل الدخول أولاً قبل البدء
        string? userEmail = HttpContext.Session.GetString("UserEmail");
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Index", "Login");
        }

        ViewBag.DayName = day;
        ViewBag.MealType = type;
        
        // جلب الوصفات المتاحة في النظام ليعرضها المتصفح في القائمة
        ViewBag.Recipes = await _context.Recipes.AsNoTracking().ToListAsync();
        
        return View();
    }

    // 3. استقبال البيانات وحفظها في قاعدة البيانات وربطها بالمستخدم الحالي ديناميكياً
    [HttpPost]
    public async Task<IActionResult> Create(Guid recipeId, string dayName, string mealType)
    {
        // 🔍 جلب بريد المستخدم الحالي من الـ Session
        string? userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Index", "Login");
        }

        // إزالة أي وجبة قديمة محجوزة لنفس اليوم ونفس الوجبة *لهذا المستخدم فقط* لمنع التكرار
        var existingMeal = await _context.MealPlanners
            .FirstOrDefaultAsync(m => m.UserEmail == userEmail && m.DayName == dayName && m.MealType == mealType);

        if (existingMeal != null)
        {
            _context.MealPlanners.Remove(existingMeal);
        }

        // إنشاء السجل الجديد وحفظ البصمة الديناميكية للمستخدم
        var newMeal = new MealPlanner
        {
            Id = Guid.NewGuid(),
            UserEmail = userEmail, // حفظ الإيميل الحالي النشط (مثل hawa أو amal)
            RecipeId = recipeId,
            DayName = dayName,
            MealType = mealType,
            CreatedAt = DateTime.UtcNow
        };

        _context.MealPlanners.Add(newMeal);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // 4. حذف وجبة مع التحقق الصارم من ملكية المستخدم لها
    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        // 🔍 جلب بريد المستخدم الحالي من الـ Session
        string? userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Index", "Login");
        }

        // جلب الوجبة والتأكد من أنها تعود للمستخدم الحالي منعاً للتلاعب بالحذف
        var meal = await _context.MealPlanners
            .FirstOrDefaultAsync(m => m.Id == id && m.UserEmail == userEmail);

        if (meal != null)
        {
            _context.MealPlanners.Remove(meal);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}