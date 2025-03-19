using HasteLayoutGen.Landfall;
using Microsoft.AspNetCore.Mvc;

namespace SeedVisualiser
{
    public class Program
    {
        private const int LAYOUT_DEPTH = 13;

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
                context.Response.ContentType = "image/png";
                var png = Visualiser.DrawLayoutAsPng(int.Parse(seed), LAYOUT_DEPTH, generator);
                await context.Response.Body.WriteAsync(png);
            });

            app.Run();
        }
    }
}
