using HasteLayoutGen.Landfall;
using static HasteLayoutGen.LayoutAnalysis;

namespace HasteLayoutGen
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Dictionary<int, int> bestCounts = new();
            Dictionary<int, int> worstCounts = new();
            Dictionary<int, int> randomCounts = new();

            for (int seed = 0; seed < 1000000; seed++)
            {
                ExperimentalLevelSelectionMapGenerator.GenerationInfo info = new ExperimentalLevelSelectionMapGenerator.GenerationInfo { Depth = 13, Nodes = new(), Paths = new() };
                var (nodes, paths) = ExperimentalLevelSelectionMapGenerator.Generate(info, new Random(seed));
                int bestCount = PathCount(FindBestPath(nodes, paths));
                int randomCount = PathCount(FindRandomPath(nodes, paths));
                int worstCount = PathCount(FindWorstPath(nodes, paths));

                bestCounts[bestCount] = bestCounts.GetValueOrDefault(bestCount, 0) + 1;
                randomCounts[randomCount] = randomCounts.GetValueOrDefault(randomCount, 0) + 1;
                worstCounts[worstCount] = worstCounts.GetValueOrDefault(worstCount, 0) + 1;

                if (seed % 2500 == 0)
                {
                    Console.WriteLine($"Up to seed {seed}.");
                }
            }
            Console.WriteLine("best case");
            for (int i = 1; i < 17; i++)
            {
                Console.WriteLine(bestCounts.GetValueOrDefault(i, 0));
            }
            Console.WriteLine("random case");
            for (int i = 1; i < 17; i++)
            {
                Console.WriteLine(randomCounts.GetValueOrDefault(i, 0));
            }
            Console.WriteLine("worst case");
            for (int i = 1; i < 17; i++)
            {
                Console.WriteLine(worstCounts.GetValueOrDefault(i, 0));
            }
        }
    }
}
