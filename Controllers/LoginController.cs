using Microsoft.AspNetCore.Mvc;

namespace SmartFood.Controllers
{
  using Microsoft.AspNetCore.Mvc;

namespace SmartFood.Controllers
{
    public class LoginController : Controller
    {
        // GET: /
        [HttpGet("")]
        [HttpGet("Login")]
        public IActionResult Index()
        {
            // 🎯 هذا السطر هو السحر! يخبر دوت نت أن يفتح ملفك الأخضر الموجود مسبقاً في الـ Areas مباشرة
            return View("~/Areas/Identity/Pages/Account/Login.cshtml");
        }

        // POST: /Login
        [HttpPost("Login")]
        public IActionResult Index(string email, string password, bool rememberMe)
{
    if (!string.IsNullOrEmpty(email))
    {
        // 💾 حفظ إيميل المستخدم الحالي في الـ Session لنتذكره في باقي الصفحات
        HttpContext.Session.SetString("UserEmail", email);
        
        return RedirectToAction("Index", "Home");
    }
    
    return View("~/Areas/Identity/Pages/Account/Login.cshtml");
}
    }
}
}