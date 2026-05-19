using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity; 
using SmartFood.Data;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSession();
// 1. إضافة خدمات الـ Controllers والـ Views للمشروع
builder.Services.AddControllersWithViews();

// 2. إعداد الاتصال بقاعدة بيانات PostgreSQL (مع زيادة مهلة الانتظار إلى 120 ثانية)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorCodesToAdd: null);
            
            // سطر يمنح السيرفر مهلة دقيقتين كاملتين لإتمام بناء الجداول
            npgsqlOptions.CommandTimeout(120); 
        }));

// 3. تفعيل نظام الـ Identity الصريح ليتوافق مع الـ Controller المخصص
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => {
    options.SignIn.RequireConfirmedAccount = false; // تسهيلاً للتجربة بدون تأكيد الإيميل
    options.Password.RequireDigit = false;          // تبسيط شروط كلمة المرور للتجربة الفورية
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>(); // ربط جداول المستخدمين بملف الـ Context الخاص بك

// 4. 💡 التعديل الحاسم والنهائي: توجيه الكوكيز للمسار الجديد الصافي لكسر كاش المتصفح
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";        // 🛠️ تم تعديلها هنا لتطابق الـ Controller الجديد تماماً
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Home/Index";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

// 5. تفعيل نظام التحقق من الهوية والصلاحيات
app.UseAuthentication(); // يحدد: من أنت؟
app.UseAuthorization();  // يحدد: ماذا يمكنك أن تفعل؟

// 6. ربط مسارات الـ Controllers
app.MapControllerRoute(
    name: "default",
  pattern: "{controller=Login}/{action=Index}/{id?}");

// سطر يقوم بطباعة جميع المسارات المسجلة في التطبيق داخل الترمينال عند التشغيل
app.Lifetime.ApplicationStarted.Register(() => {
    var endpointDataSource = app.Services.GetRequiredService<EndpointDataSource>();
    foreach (var endpoint in endpointDataSource.Endpoints)
    {
        Console.WriteLine($"[Route Found]: {endpoint.DisplayName}");
    }
});
app.UseSession();
app.Run();