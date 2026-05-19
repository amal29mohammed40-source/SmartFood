using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartFood.Models;

namespace SmartFood.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 1. أعدنا الجداول كما كانت تماماً لتختفي أخطاء الـ Controllers الـ 19 فوراً وينجح الـ Build
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<MealPlanner> MealPlanners { get; set; } = null!;
        public DbSet<Recipe> Recipes { get; set; } = null!;
        public DbSet<AiRequestLog> AiRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 2. تثبيت جداول الـ Identity أولاً (حرج جداً)
            base.OnModelCreating(modelBuilder);

            // 3. أبقينا إعدادات الجداول ليعمل المشروع بسلام، ولكن لن تسبب تكراراً إذا حذفنا مجلد الـ Migrations
            modelBuilder.Entity<Recipe>().ToTable("recipes");
            modelBuilder.Entity<Recipe>().Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<Recipe>().Property(x => x.Title).HasColumnName("title");
            modelBuilder.Entity<Recipe>().Property(x => x.Description).HasColumnName("description");
            modelBuilder.Entity<Recipe>().Property(x => x.Ingredients).HasColumnName("ingredients");
            modelBuilder.Entity<Recipe>().Property(x => x.ImageUrl).HasColumnName("image_url");
            modelBuilder.Entity<Recipe>().Property(x => x.CookingTime).HasColumnName("cooking_time");
            modelBuilder.Entity<Recipe>().Property(x => x.Calories).HasColumnName("calories");
            modelBuilder.Entity<Recipe>().Property(x => x.Protein).HasColumnName("protein");
            modelBuilder.Entity<Recipe>().Property(x => x.Carbs).HasColumnName("carbs");
            modelBuilder.Entity<Recipe>().Property(x => x.Fat).HasColumnName("fat");
            modelBuilder.Entity<Recipe>().Property(x => x.Goal).HasColumnName("goal");
            modelBuilder.Entity<Recipe>().Property(x => x.CreatedAt).HasColumnName("created_at");

            modelBuilder.Entity<AiRequestLog>().ToTable("ai_requests");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.Weight).HasColumnName("weight");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.Goal).HasColumnName("goal");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.TargetCalories).HasColumnName("target_calories");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.MaxCookingTime).HasColumnName("max_cooking_time");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.Ingredients).HasColumnName("ingredients");
            modelBuilder.Entity<AiRequestLog>().Property(x => x.CreatedAt).HasColumnName("created_at");

            modelBuilder.Entity<Favorite>().ToTable("favorites");
            modelBuilder.Entity<Favorite>().Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<Favorite>().Property(x => x.UserEmail).HasColumnName("user_email");
            modelBuilder.Entity<Favorite>().Property(x => x.RecipeId).HasColumnName("recipe_id");
            modelBuilder.Entity<Favorite>().Property(x => x.CreatedAt).HasColumnName("created_at");

            modelBuilder.Entity<Favorite>()
                .HasOne(x => x.Recipe)
                .WithMany()
                .HasForeignKey(x => x.RecipeId);

            modelBuilder.Entity<MealPlanner>().ToTable("meal_planner");
            modelBuilder.Entity<MealPlanner>().Property(x => x.Id).HasColumnName("id");
            modelBuilder.Entity<MealPlanner>().Property(x => x.UserEmail).HasColumnName("user_email");
            modelBuilder.Entity<MealPlanner>().Property(x => x.DayName).HasColumnName("day_name");
            modelBuilder.Entity<MealPlanner>().Property(x => x.MealType).HasColumnName("meal_type");
            modelBuilder.Entity<MealPlanner>().Property(x => x.RecipeId).HasColumnName("recipe_id");
            modelBuilder.Entity<MealPlanner>().Property(x => x.CreatedAt).HasColumnName("created_at");

            modelBuilder.Entity<MealPlanner>()
                .HasOne(x => x.Recipe)
                .WithMany()
                .HasForeignKey(x => x.RecipeId);
        }
    }
}