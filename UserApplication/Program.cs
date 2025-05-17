
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UserApplication.API.Models;
using UserApplication.Persistence;
using UserApplication.Persistence.Repositories.UserRepository;
using UserApplication.Services.UserService;

namespace UserApplication
{
    public class Program
    {
        public class AuthOptions
        {
            public const string ISSUER = "UserAuthServer";
            public const string AUDIENCE = "UserAuthClient";
            public static SymmetricSecurityKey GetSymmetricSecurityKey(string key) =>
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        }

        async public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
            builder.Services.AddAuthorization();
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,
                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,
                        ValidateLifetime = true,
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(builder.Configuration["SecretKey"]!),
                        ValidateIssuerSigningKey = true,
                    };
                });

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserService, UserService>();


            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

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
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
