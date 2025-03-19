using HasteLayoutGen.Compat;
using HasteLayoutGen.Landfall;
using static HasteLayoutGen.Landfall.LevelSelectionNode;

namespace HasteLayoutGen
{
    public static class LayoutAnalysis
    {
        public static int PathCount(List<LevelSelectionNode> nodes)
        {
            return nodes.Count() - nodes.Where(n => n.Type != NodeType.Default && n.Type != NodeType.Challenge).Count();
        }

        public static List<LevelSelectionNode> FindBestPath(List<LevelSelectionNode> nodes, List<LevelSelectionPath> paths)
        {
            var start = nodes.MinBy(n => n.Depth);
            var end = nodes.MaxBy(n => n.Depth);
            return FindEitherBestOrWorstPath(start, end, paths, false);
        }

        public static List<LevelSelectionNode> FindWorstPath(List<LevelSelectionNode> nodes, List<LevelSelectionPath> paths)
        {
            var start = nodes.MinBy(n => n.Depth);
            var end = nodes.MaxBy(n => n.Depth);
            return FindEitherBestOrWorstPath(start, end, paths, true);
        }

        public static List<LevelSelectionNode> FindRandomPath(List<LevelSelectionNode> nodes, List<LevelSelectionPath> paths)
        {
            var start = nodes.MinBy(n => n.Depth);
            var end = nodes.MaxBy(n => n.Depth);
            var rng = new Random();
            List<LevelSelectionNode> path = [start];
            while (path.Last() != end)
            {
                LevelSelectionNode? next = rng.Choice(paths.Where(e => e.From == path.Last()).ToList()).To;
                if (next == null)
                {
                    throw new Exception("Invalid path! An edge has a null end before found a path to the end node!");
                }
                path.Add(next);
            }
            return path;
        }

        private static List<LevelSelectionNode> FindEitherBestOrWorstPath(LevelSelectionNode start, LevelSelectionNode end, List<LevelSelectionPath> edges, bool findWorst)
        {
            Dictionary<LevelSelectionNode, int> badNodeCount = new();
            Dictionary<LevelSelectionNode, LevelSelectionNode?> cameFrom = new();
            PriorityQueue<LevelSelectionNode, int> pq = new(); // Priority queue for Dijkstra-style traversal

            int weight = findWorst ? 0 : 1;
            // Start with the start node
            badNodeCount[start] = (start.Type == NodeType.Default || start.Type == NodeType.Challenge) ? weight : (1 - weight);
            pq.Enqueue(start, badNodeCount[start]);

            while (pq.Count > 0)
            {
                LevelSelectionNode current = pq.Dequeue();

                if (current == end)
                    break; // Reached the destination

                foreach (var edge in edges.Where(e => e.From == current))
                {
                    LevelSelectionNode neighbor = edge.To;
                    int newBadCount = badNodeCount[current] + ((neighbor.Type == NodeType.Default || neighbor.Type == NodeType.Challenge) ? weight : (1 - weight));

                    // Only update if we found a better (fewer bad nodes) path
                    if (!badNodeCount.ContainsKey(neighbor) || newBadCount < badNodeCount[neighbor])
                    {
                        badNodeCount[neighbor] = newBadCount;
                        cameFrom[neighbor] = current;
                        pq.Enqueue(neighbor, newBadCount);
                    }
                }
            }

            // Reconstruct path
            List<LevelSelectionNode> path = new();
            LevelSelectionNode? pathNode = end;

            while (pathNode != null)
            {
                path.Add(pathNode);
                pathNode = cameFrom.ContainsKey(pathNode) ? cameFrom[pathNode] : null;
            }

            path.Reverse();
            return path;
        }
    }
}
