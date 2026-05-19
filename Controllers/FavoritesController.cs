using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFood.Data;
using SmartFood.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFood.Controllers;

public class FavoritesController : Controller
{
    private readonly AppDbContext _context;

    public FavoritesController(AppDbContext context)
    {
        _context = context;
    }

    // 1. عرض صفحة المفضلة الخاصة بالمستخدم الحالي فقط
    public async Task<IActionResult> Index()
    {
        // 🔍 جلب بريد المستخدم الحالي من الـ Session
        string? userEmail = HttpContext.Session.GetString("UserEmail");

        // 🔒 حماية: إذا لم يسجل دخوله، يتم طرده إلى صفحة الـ Login
        if (string.IsNullOrEmpty(userEmail))
        {
            return RedirectToAction("Index", "Login");
        }

        var favorites = await _context.Favorites
            .AsNoTracking()
            .Include(f => f.Recipe)
            .Where(f => f.UserEmail == userEmail) // الفلترة بحسب المستخدم الحالي
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();

        return View(favorites);
    }

    // 2. إضافة إلى المفضلة عبر AJAX باستخدام إيميل المستخدم المسجل
    [HttpPost]
    public async Task<IActionResult> Add(Guid recipeId)
    {
        // 🔍 جلب بريد المستخدم الحالي من الـ Session
        string? userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { success = false, message = "يرجى تسجيل الدخول أولاً لإضافة الوصفة للمفضلة." });
        }

        // التحقق من وجود الوصفة في مفضلة هذا المستخدم مسبقاً
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.UserEmail == userEmail && f.RecipeId == recipeId);

        if (favorite == null)
        {
            var newFavorite = new Favorite
            {
                Id = Guid.NewGuid(),
                UserEmail = userEmail, // حفظ الإيميل الديناميكي
                RecipeId = recipeId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Favorites.Add(newFavorite);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "تمت الإضافة للمفضلة بنجاح!", favoriteId = newFavorite.Id });
        }

        return Json(new { success = false, message = "الوصفة موجودة بالفعل في المفضلة." });
    }

    // 3. حذف من المفضلة عبر AJAX مع التحقق من ملكية العنصر
    [HttpPost]
    public async Task<IActionResult> Remove(Guid id)
    {
        // 🔍 جلب بريد المستخدم الحالي من الـ Session
        string? userEmail = HttpContext.Session.GetString("UserEmail");

        if (string.IsNullOrEmpty(userEmail))
        {
            return Json(new { success = false, message = "انتهت الجلسة، يرجى إعادة تسجيل الدخول." });
        }

        // جلب العنصر والتأكد من أنه يخص المستخدم الحالي لمنع التلاعب والأخطاء
        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.Id == id && f.UserEmail == userEmail);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "تم الحذف من المفضلة بنجاح." });
        }

        return Json(new { success = false, message = "لم يتم العثور على العنصر المطلوب أو لا تملك صلاحية حذفه." });
    }
}