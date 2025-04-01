using HasteLayoutGen.Landfall;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace SeedVisualiser
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.MapGet("/seed/{generator}/{seed}.png", async (HttpContext context, [FromRoute] string generator, [FromRoute] string seed) =>
            {
                // Old links had a hardcoded depth of 13
                context.Response.ContentType = "image/png";
                var png = Visualiser.DrawLayoutAsPng(int.Parse(seed), 13, generator);
                await context.Response.Body.WriteAsync(png);
            });

            app.MapGet("/seed/{generator}/{depth}/{seed}.png", async (HttpContext context, [FromRoute] string generator, [FromRoute] int depth, [FromRoute] int seed) =>
            {
                if (depth < 1 || depth > 50)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                context.Response.ContentType = "image/png";
                var png = Visualiser.DrawLayoutAsPng(seed, depth, generator);
                await context.Response.Body.WriteAsync(png);
            });

            app.Run();
        }
    }
}
