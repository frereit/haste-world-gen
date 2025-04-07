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

            app.MapGet("/shards/{shardId}/{seed}.png", async (HttpContext context, [FromRoute] int shardId, [FromRoute] int seed) =>
            {
                if (seed < 1 || !ShardData.Shards.ContainsKey(shardId))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    return;
                }
                context.Response.ContentType = "image/png";
                var png = Visualiser.DrawLayoutAsPng(ShardData.Shards[shardId], seed);
                await context.Response.Body.WriteAsync(png);
            });

            app.Run();
        }
    }
}
