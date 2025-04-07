using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HasteLayoutGen.Compat;
using HasteLayoutGen.Landfall;
using System.Numerics;

public static class LevelSelectionMapGenerator
{
    public static readonly string Version = "release.1.2.d";

    public class GenerationInfo
    {
        public int Depth;

        public List<LevelSelectionNode> Nodes;
        public List<LevelSelectionPath> Paths;
    }

    public static void Generate(GenerationInfo generationInfo, System.Random random)
    {
        List<LevelSelectionNode> levelNodes = generationInfo.Nodes;
        List<LevelSelectionPath> levelPaths = generationInfo.Paths;

        var firstNode = SpawnNode(Vector3.UnitY * 10.0f, 0, generationInfo, LevelSelectionNode.NodeType.Default); // spawn first node...

        int depth = generationInfo.Depth;

        for (int i = 1; i <= depth; i++)
        {
            int count = random.Next(2, 4);

            for (int x = 0; x < count; x++)
            {
                for (int j = 0; j < 15; j++)
                {
                    var spawnPos = new Vector3(random.Range(-20, 20), 10.0f, random.Range(0, 2) + i * 7);

                    if (IsValidPosition(spawnPos, levelNodes))
                    {
                        var newNode = SpawnNode(spawnPos, i, generationInfo, LevelSelectionNode.NodeType.Default);
                        break;
                    }
                }
            }
        }

        var baseNodes = levelNodes.First();
        var firstNodes = levelNodes.FindAll(x => x.Depth == 1);

        foreach (var selectionNode in firstNodes)
        {
            levelPaths.Add(new LevelSelectionPath(baseNodes, selectionNode));
        }

        for (int i = 1; i < depth; i++)
        {
            var nodes = levelNodes.FindAll(x => x.Depth == i);
            var nodesNext = levelNodes.FindAll(x => x.Depth == i + 1);
            foreach (var n in nodes)
            {
                var nodesToConnectTo = GetNodesSortedByClose(nodesNext, n.Position);

                bool connected = false;

                foreach (var selectionNode in nodesToConnectTo)
                {
                    if (!DoesPathsIntersect(n.Position, selectionNode.Position, levelPaths))
                    {
                        levelPaths.Add(new LevelSelectionPath(n, selectionNode));
                        connected = true;
                        break;
                    }
                }

                if (!connected)
                {
                    levelPaths.Add(new LevelSelectionPath(n, random.Choice(nodesNext)));
                }
            }

            var unconnectedNodes = GetUnconnectedNodes(nodesNext, levelPaths);

            foreach (var unconnectedNode in unconnectedNodes)
            {
                var nodeToConnectTo = GetNodesSortedByClose(nodes, unconnectedNode.Position);

                bool connected = false;

                foreach (var testNode in nodeToConnectTo)
                {
                    if (!DoesPathsIntersect(testNode.Position, unconnectedNode.Position, levelPaths))
                    {
                        levelPaths.Add(new LevelSelectionPath(testNode, unconnectedNode));
                        connected = true;
                        break;
                    }
                }

                if (!connected)
                {

                    levelPaths.Add(new LevelSelectionPath(random.Choice(nodes), unconnectedNode));
                }
            }

            foreach (var n in nodes)
            {
                bool extraConnection = random.Range(0, 100) < 25;

                if (!extraConnection)
                {
                    continue;
                }

                var nodesToConnectTo = GetNodesSortedByClose(nodesNext, n.Position).Reverse();

                foreach (var selectionNode in nodesToConnectTo)
                {
                    if (!DoesPathsIntersect(n.Position, selectionNode.Position, levelPaths))
                    {
                        levelPaths.Add(new LevelSelectionPath(n, selectionNode));

                        break;
                    }
                }
            }
        }

        var bossNode = levelNodes.Last().Position.Z + 15.0f;

        var boss = SpawnNode(new Vector3(0, 10.0f, bossNode), depth + 1, generationInfo, LevelSelectionNode.NodeType.Boss);

        var lastNodes = levelNodes.FindAll(x => x.Depth == depth);

        foreach (var n in lastNodes)
        {
            levelPaths.Add(new LevelSelectionPath(n, boss));
        }

        MakeShops(generationInfo, depth, random);

        SetPercentageRandomToType(LevelSelectionNode.NodeType.Challenge, 0.07f);
        SetPercentageRandomToType(LevelSelectionNode.NodeType.Encounter, 0.1f);
        SetPercentageRandomToType(LevelSelectionNode.NodeType.RestStop, 0.07f);
        SetPercentageRandomToType(LevelSelectionNode.NodeType.Shop, 0.02f);

        void SetPercentageRandomToType(LevelSelectionNode.NodeType type, float percentage)
        {
            int count = UnityMathf.RoundToInt(levelNodes.Count * percentage);
            for (int i = 0; i < count; i++)
            {
                var candidates = levelNodes.Skip(1).Where(n => AllowedToConvertNode(n, type, levelPaths)).ToList();
                if (candidates.Count == 0)
                {

                    break;
                }

                random.Choice(candidates).SetType(type);
            }
        }

        for (int i = 0; i < 10; i++)
        {
            foreach (var levelPath in levelPaths)
            {
                var direction = levelPath.From.Position - levelPath.To.Position;

                if (levelPath.From.Depth > 0)
                {
                    levelPath.From.Position += Vector3.UnitX * -1 * direction.X * 0.03f;
                }
                if (levelPath.To.Depth > 0)
                {
                    levelPath.To.Position -= Vector3.UnitX * -1 * direction.X * 0.03f;
                }

                var nodesOnSameDepth = levelNodes.FindAll(x => x.Depth == levelPath.From.Depth);

                var node = levelPath.From;

                foreach (var selectionNode in nodesOnSameDepth)
                {
                    if (selectionNode == node) continue;

                    float sign = -UnityMathf.Sign(selectionNode.Position.X - node.Position.X);
                    float xDistance = UnityMathf.Abs(selectionNode.Position.X - node.Position.X);
                    float range = 10;

                    float factor = UnityMathf.InverseLerp(range, 0, xDistance);

                    node.Position += Vector3.UnitX * factor * sign * range * 0.25f;
                }
            }
        }

        foreach (var path1 in levelPaths)
        {
            foreach (var path2 in levelPaths)
            {
                if (path1 == path2)
                {
                    continue;
                }

                if (Math2DUtility.AreLinesIntersecting(path1.From.Position.xz(), path1.To.Position.xz(),
                        path2.From.Position.xz(), path2.To.Position.xz(), false))
                {
                    path1.Intersects = true;
                    path2.Intersects = true;
                }
            }
        }

    }

    private static void MakeShops(GenerationInfo generationInfo, int depth, System.Random random)
    {
        var firstShopDepth = 3;
        var lastShopDepth = depth;

        for (var i = firstShopDepth; i < lastShopDepth; i += 4)
            SetShopAtDepth(i);

        SetShopAtDepth(lastShopDepth);

        void SetShopAtDepth(int depth)
        {
            var nodesOnThisDepth = generationInfo.Nodes.Where(n => n.Depth == depth).ToArray();

            if (nodesOnThisDepth.Length > 0)
            {
                var ran = random.Choice(nodesOnThisDepth.ToList());
                foreach (var node in nodesOnThisDepth)
                {
                    if (node == ran || random.NextFloat() > .5f)
                    {
                        if (AllowedToConvertNode(node, LevelSelectionNode.NodeType.Shop, generationInfo.Paths))
                        {
                            node.SetType(LevelSelectionNode.NodeType.Shop);
                        }
                    }
                }
            }
        }
    }

    private static LevelSelectionNode SpawnNode(Vector3 pos, int depth, GenerationInfo generationInfo, LevelSelectionNode.NodeType type)
    {
        var nodes = generationInfo.Nodes;

        var node = new LevelSelectionNode { Position = pos, Depth = depth, Type = type };
        nodes.Add(node);

        return node;
    }

    private static bool IsValidPosition(Vector3 spawnPos, List<LevelSelectionNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (Vector3.Distance(node.Position, spawnPos) < 5.0f)
            {
                return false;
            }
        }

        return true;
    }

    private static List<LevelSelectionNode> GetUnconnectedNodes(List<LevelSelectionNode> nodes, List<LevelSelectionPath> paths)
    {
        return nodes.FindAll(x => paths.All(y => y.From != x && y.To != x));
    }

    private static IEnumerable<LevelSelectionNode> GetNodesSortedByClose(List<LevelSelectionNode> nodes, Vector3 pos)
    {
        return nodes.OrderBy(x => Vector3.Distance(pos, x.Position));
    }

    private static bool DoesPathsIntersect(Vector3 start, Vector3 end, List<LevelSelectionPath> paths)
    {
        foreach (var path in paths)
        {
            if (Math2DUtility.AreLinesIntersecting(path.From.Position.xz(), path.To.Position.xz(), start.xz(), end.xz(), false))
            {
                return true;
            }
        }

        return false;
    }

    private static bool AllowedToConvertNode(LevelSelectionNode node, LevelSelectionNode.NodeType type, List<LevelSelectionPath> paths)
    {
        if (node.Type != LevelSelectionNode.NodeType.Default)
            return false;
        if (WouldConvertingProduceDoubleNode(node, type, paths))
            return false;
        if (type != LevelSelectionNode.NodeType.Challenge && ConvertingWouldProduceStreakOf(node, paths) > 2)
            return false;
        if (type == LevelSelectionNode.NodeType.Shop && node.Depth < 3)
            return false;
        return true;
    }

    // If we convert `node` to `type`, will that produce two `type`s in a row? e.g. a shop directly followed by a shop
    private static bool WouldConvertingProduceDoubleNode(LevelSelectionNode node, LevelSelectionNode.NodeType type, List<LevelSelectionPath> paths)
    {
        foreach (var path in paths)
        {
            LevelSelectionNode other;
            if (path.From == node)
                other = path.To;
            else if (path.To == node)
                other = path.From;
            else
                continue;
            if (other.Type == type)
                return true;
        }

        return false;
    }

    // If we convert node to a encounter/shop/reststop, how long of a streak of encounters/shop/reststops would happen?
    // For example, if there exists a path of [shop, rest, x, rest, shop], and x is what is passed into this method, this would return 5.
    private static int ConvertingWouldProduceStreakOf(LevelSelectionNode node, List<LevelSelectionPath> paths)
    {
        return Count(node, true) + Count(node, false) + 1;

        int Count(LevelSelectionNode n, bool forwards)
        {
            var max = 0;
            foreach (var path in paths)
            {
                var me = forwards ? path.From : path.To;
                if (me == n)
                {
                    var next = forwards ? path.To : path.From;
                    if (next != null && next.Type is LevelSelectionNode.NodeType.Encounter or LevelSelectionNode.NodeType.Shop or LevelSelectionNode.NodeType.RestStop)
                        max = UnityMathf.Max(max, Count(next, forwards) + 1);
                }
            }

            return max;
        }
    }

    private static int NumberOfLevelsInShortestPath(List<LevelSelectionNode> nodes, List<LevelSelectionPath> paths)
    {
        Dictionary<LevelSelectionNode, (int numLevels, LevelSelectionNode from)> cameFrom = new();
        var first = nodes.MinBy(n => n.Depth);
        var last = nodes.MaxBy(n => n.Depth);
        cameFrom[first] = (0, null);

        for (var i = first.Depth + 1; i <= last.Depth; i++)
            foreach (var node in nodes)
                if (node.Depth == i)
                    cameFrom[node] = GetCheapest(node);

        return cameFrom[last].numLevels;

        (int numLevels, LevelSelectionNode from) GetCheapest(LevelSelectionNode node)
        {
            var min = paths.Where(path => path.To == node).Select(path => (cameFrom[path.From].numLevels, path.From)).MinBy(v => v.numLevels);
            if (node.Type is LevelSelectionNode.NodeType.Default or LevelSelectionNode.NodeType.Challenge or LevelSelectionNode.NodeType.Boss)
                min.numLevels++;
            return min;
        }
    }
}
