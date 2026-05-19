using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFood.Data;
using SmartFood.Models; // تأكدي أن هذا النطاق يحتوي على موديل الـ MealPlanner أو الـ Recipe

namespace SmartFood.Controllers
{
    // إضافة الـ Route ليدعم المسار العادي ومسار الـ API لـ React
    [Route("[controller]/[action]")]
    public class RecipesController : Controller
    {
        private readonly AppDbContext _context;

        public RecipesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Recipes/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // جلب قائمة كل الوصفات المخزنة في قاعدة البيانات
            var recipes = await _context.Recipes.ToListAsync();
            return View(recipes);
        }

        // GET: Recipes/Details/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            // جلب تفاصيل الوصفة من قاعدة البيانات بناءً على الـ ID المرسل من الزر
            var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                return NotFound(); // إذا لم تكن الأكلة موجودة في الجدول
            }

            return View(recipe);
        }

        // ==========================================
        // 🚀 الكود الجديد المضاف لتفعيل زر +Plan من الـ React
        // ==========================================
        
        // POST: Recipes/AddToPlan
        [HttpPost]
        public async Task<IActionResult> AddToPlan([FromBody] PlanRequestDto request)
        {
            if (request == null || request.RecipeId == Guid.Empty)
            {
                return BadRequest(new { message = "البيانات المرسلة غير صالحة" });
            }

            // إنشاء سجل جديد لحفظ الوجبة في جدول الـ Planner
            // تأكدي من اسم الموديل عندك هل هو MealPlanner أو اسم آخر في مجلد Models
            var newPlan = new MealPlanner 
            {
                Id = Guid.NewGuid(), // توليد معرف فريد تلقائياً للعملية
                UserEmail = request.UserEmail, // الإيميل القادم من الـ Frontend
                RecipeId = request.RecipeId,   // الـ ID الخاص بالوصفة
                CreatedAt = DateTime.UtcNow    // وقت الإضافة التلقائي
            };

            // إضافة السجل إلى جدول الـ MealPlanners في قاعدة البيانات
            _context.MealPlanners.Add(newPlan);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تمت إضافة الوجبة إلى مخططك الأسبوعي بنجاح!" });
        }
    }

    // الـ DTO المطلوب لاستقبال البيانات القادمة من الـ React Component
    // الـ DTO المطلوب لاستقبال البيانات القادمة من الـ React Component
public class PlanRequestDto
{
    // تعيين قيمة افتراضية تمنع ظهور الخط الأصفر التنبيهي
    public string UserEmail { get; set; } = string.Empty;
    public Guid RecipeId { get; set; }
}
}