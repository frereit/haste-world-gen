using HasteLayoutGen.Landfall;
using HasteLayoutGen.Analysis;
using SkiaSharp;
using System.IO.Pipelines;
using System.Buffers;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Reflection;

namespace SeedVisualiser
{
    public static class Visualiser
    {
        public static byte[] DrawLayoutAsPng(int seed, int depth, string generator)
        {
            List<LevelSelectionNode> nodes;
            List<LevelSelectionPath> paths;
            if (generator == "experimental")
            {
                ExperimentalLevelSelectionMapGenerator.GenerationInfo info = new() { Depth = depth, Nodes = [], Paths = [] };
                (nodes, paths) = ExperimentalLevelSelectionMapGenerator.Generate(info, new Random(seed));
            }
            else if (generator == "demo")
            {
                DemoLevelSelectionMapGenerator.GenerationInfo info = new() { Depth = depth, Nodes = [], Paths = [] };
                (nodes, paths) = DemoLevelSelectionMapGenerator.Generate(info, new Random(seed));
            }
            else
            {
                throw new ArgumentException($"Unkown generator {generator}");
            }
            float scale = 17f;

            var imageInfo = new SKImageInfo(
                width: 1200,
                height: 2000,
                colorType: SKColorType.Rgba8888,
                alphaType: SKAlphaType.Premul);
            var surface = SKSurface.Create(imageInfo);

            var canvas = surface.Canvas;
            canvas.Clear(SKColor.Parse("#FFFFFF"));

            // Draw the edges
            var lineColor = SKColor.Parse("#000000");
            float lineWidth = 5f;
            var linePaint = new SKPaint { Color = lineColor, StrokeWidth = lineWidth, IsAntialias = true, Style = SKPaintStyle.Stroke };
            foreach (var path in paths)
            {
                int x1 = (int)(path.From.Position.X * scale) + imageInfo.Width / 2;
                int y1 = imageInfo.Height - (int)(path.From.Position.Z * scale) - (int)(2 * scale);
                int x2 = (int)(path.To.Position.X * scale) + imageInfo.Width / 2;
                int y2 = imageInfo.Height - (int)(path.To.Position.Z * scale) - (int)(2 * scale);
                canvas.DrawLine(x1, y1, x2, y2, linePaint);
            }

            // Draw the best edges thicker
            linePaint.StrokeWidth = 9f;
            var best = LayoutAnalysis.FindBestPath(nodes, paths);
            for (int i = 1; i < best.Count; i++)
            {
                var from = best[i - 1];
                var to = best[i];
                int x1 = (int)(from.Position.X * scale) + imageInfo.Width / 2;
                int y1 = imageInfo.Height - (int)(from.Position.Z * scale) - (int)(2 * scale);
                int x2 = (int)(to.Position.X * scale) + imageInfo.Width / 2;
                int y2 = imageInfo.Height - (int)(to.Position.Z * scale) - (int)(2 * scale);
                canvas.DrawLine(x1, y1, x2, y2, linePaint);
            }

            // Draw the nodes
            var nodeColors = new Dictionary<LevelSelectionNode.NodeType, SKColor>()
            {
                { LevelSelectionNode.NodeType.Default, SKColor.Parse("#2ecc71") },
                { LevelSelectionNode.NodeType.Shop, SKColor.Parse("#d5ba80") },
                { LevelSelectionNode.NodeType.Challenge, SKColor.Parse("#222f3e") },
                { LevelSelectionNode.NodeType.Encounter, SKColor.Parse("#73bfd0") },
                { LevelSelectionNode.NodeType.Boss, SKColor.Parse("#6D214F") },
                { LevelSelectionNode.NodeType.RestStop, SKColor.Parse("#da9090")}
            };
            foreach (var node in nodes)
            {
                var nodeColor = nodeColors[node.Type];
                var nodePaint = new SKPaint { Color = nodeColor, StrokeWidth = 1, IsAntialias = true, Style = SKPaintStyle.Fill };
                int x1 = (int)(node.Position.X * scale) + imageInfo.Width / 2;
                int y1 = imageInfo.Height - (int)(node.Position.Z * scale) - (int)(2 * scale);
                canvas.DrawCircle(x1, y1, scale * 1.5f, nodePaint);
            }


            // Draw a legend
            int numTypes = 0;
            var rectPaint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            var textPaint = new SKPaint
            {
                IsAntialias = true,
            };
            var assembly = Assembly.GetExecutingAssembly();
            var stream = assembly.GetManifestResourceStream("SeedVisualiser.NotoSans-Regular.ttf");
            if (stream == null)
                throw new IOException("Unable to find embedded font resource");

            var textFont = new SKFont(SKTypeface.FromStream(stream), scale * 5);

            canvas.DrawText($"seed {seed}", imageInfo.Width - scale, scale * 5, SKTextAlign.Right, textFont, textPaint);
            textFont.Size = scale * 2;
            canvas.DrawText($"{generator} generator", imageInfo.Width - scale, scale * 7, SKTextAlign.Right, textFont, textPaint);

            textFont.Size = scale * 3;
            foreach (var type in Enum.GetValues(typeof(LevelSelectionNode.NodeType)))
            {
                textPaint.Color = nodeColors[(LevelSelectionNode.NodeType)type];
                canvas.DrawText(Enum.GetName(typeof(LevelSelectionNode.NodeType), type), scale * 5, scale * 4 + numTypes * scale * 3, SKTextAlign.Left, textFont, textPaint);

                var rect = new SKRect(scale, 2 * scale + numTypes * scale * 3, scale * 4, scale * 4 + numTypes * scale * 3);
                rectPaint.Color = nodeColors[(LevelSelectionNode.NodeType)type];
                canvas.DrawRect(rect, rectPaint);
                numTypes++;
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 80);
            return data.ToArray();
        }

    }
}
