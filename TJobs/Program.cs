using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace TJobs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: MyAllowSpecificOrigins,
                                  policy =>
                                  {
                                      policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                                  });
            });

            // Add services to the container.
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("EraaSoft##EraaSoft##EraaSoft##EraaSoft##EraaSoft##"))
                };
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer("Data Source=.;Initial Catalog=TJobs;Integrated Security=True;TrustServerCertificate=True"));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                            .AddEntityFrameworkStores<ApplicationDbContext>()
                            .AddDefaultTokenProviders();

            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }

            app.UseHttpsRedirection();

            app.UseCors(MyAllowSpecificOrigins);

            app.UseAuthentication();
            app.UseAuthorization();

            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                dbInitializer.Initialize();
            }

            app.MapControllers();

            app.Run();
        }
    }
}
