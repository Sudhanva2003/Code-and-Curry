using Code_Curry.Models;
using Microsoft.EntityFrameworkCore;

namespace Code_Curry
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddDbContext<CodeCurryContext>(o =>
                o.UseSqlServer(builder.Configuration.GetConnectionString("mycon")));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // ? Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend",
                    policy => policy.WithOrigins("http://localhost:4200")
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            // ? Enable CORS
            app.UseCors("AllowFrontend");

            app.MapControllers();

            app.Run();
        }
    }
}
