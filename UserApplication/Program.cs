
using Microsoft.EntityFrameworkCore;
using UserApplication.API.Models;
using UserApplication.Persistence;

namespace UserApplication
{
    public class Program
    {
        async public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var canConnect = await db.Database.CanConnectAsync();
                app.Logger.LogInformation("Can connect to database: {CanConnect}", canConnect);

                db.Database.Migrate();
                if (!db.Users.Any(u => u.Admin == true))
                {
                    db.Users.Add(new User
                    {
                        Login = "admin",
                        Password = "admin",
                        Name = "admin",
                        Admin = true,
                        CreatedBy = ""
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
