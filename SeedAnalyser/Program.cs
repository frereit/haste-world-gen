using HasteLayoutGen.Analysis;
using HasteLayoutGen.Landfall;
using ScottPlot;
using ScottPlot.Plottables;

namespace SeedAnalyser
{
    internal class Program
    {
        static readonly int SeedCount = 1000000;
        static void Main(string[] args)
        {
            Thread[] threads = new Thread[10];

            for (int shard = 0; shard < 10; shard++)
            {
                int localShard = shard;
                threads[shard] = new Thread(() => AnalyseShard(localShard));
                threads[shard].Start();
            }

            for (int i = 0; i < 10; i++)
            {
                threads[i].Join();
            }
        }

        static void AnalyseShard(int shard)
        {
            Dictionary<int, int> counts = [];
            for (int seed = 0; seed < SeedCount; seed++)
            {
                LevelSelectionMapGenerator.GenerationInfo info = new() { Depth = ShardData.Shards[shard].NrOfLevels, Nodes = [], Paths = [] };
                LevelSelectionMapGenerator.Generate(info, new Random(seed));
                var best = LayoutAnalysis.PathCount(LayoutAnalysis.FindBestPath(info.Nodes, info.Paths));
                counts[best] = counts.GetValueOrDefault(best, 0) + 1;
            }


            var plt = new Plot();
            var positions = counts.Keys.Select(k => (double)k).ToArray();
            var barPlot = plt.Add.Bars(positions, counts.Values.Select(v => (double)v / SeedCount * 100));

            foreach (var bar in barPlot.Bars)
            {
                bar.Label = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:f4}%", bar.Value);
            }
            barPlot.ValueLabelStyle.Bold = true;
            barPlot.ValueLabelStyle.FontSize = 23;


            plt.Title($"Shard {shard+1}");
            plt.XLabel("number of default or challenge levels on the best route");
            plt.YLabel("percentage");
            plt.Axes.Left.TickLabelStyle.FontSize = 32;
            plt.Axes.Left.Label.FontSize = 32;
            plt.Axes.Bottom.TickLabelStyle.FontSize = 32;
            plt.Axes.Bottom.Label.FontSize = 32;
            plt.Axes.Margins(bottom: 0);
            plt.Axes.Bottom.SetTicks(positions, [.. counts.Keys.Select(k => k.ToString())]);
            plt.SavePng($"shard{shard+1}.png", 1200, 800);
        }
    }
}
