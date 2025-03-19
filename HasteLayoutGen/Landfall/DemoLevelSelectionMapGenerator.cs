/* Copyright (c) 2025, Landfall Games.
 * Small compatibility modifications by frereit, shared with permission.
 */

using System.Numerics;
using HasteLayoutGen.Compat;

namespace HasteLayoutGen.Landfall
{
    public static class DemoLevelSelectionMapGenerator
    {
        public class GenerationInfo
        {
            public int Depth;
            public List<LevelSelectionNode> Nodes;
            public List<LevelSelectionPath> Paths;
        }

        public static (List<LevelSelectionNode>, List<LevelSelectionPath>) Generate(GenerationInfo generationInfo, Random random)
        {
            List<LevelSelectionNode> levelNodes = generationInfo.Nodes;
            List<LevelSelectionPath> levelPaths = generationInfo.Paths;

            var node = SpawnNode(Vector3.UnitY * 10.0f, 0, generationInfo, LevelSelectionNode.NodeType.Default); // spawn first node...

            int depth = generationInfo.Depth;

            for (int i = 1; i <= depth; i++)
            {
                int count = random.Next(2, 4);

                for (int x = 0; x < count; x++)
                {
                    for (int j = 0; j < 15; j++)
                    {
                        var spawnPos = new Vector3(random.Range(-20, 20), 10.0f, random.Range(5, 7) + i * 7);

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
                int count = (int)MathF.Round(levelNodes.Count * percentage, MidpointRounding.ToEven);
                for (int i = 0; i < count; i++)
                {
                    var challengeNode = random.Choice(levelNodes.Skip(1).Where(x => x.Type == LevelSelectionNode.NodeType.Default).ToList());
                    challengeNode.SetType(type);
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

            return (levelNodes, levelPaths);
        }

        private static void MakeShops(GenerationInfo generationInfo, int depth, Random random)
        {
            var shopCount = 1;
            for (int i = 1; i < depth - 1; i++)
            {
                if ((i - 1) % 5 == 0)
                    shopCount++;
            }

            var firstShopDepth = 3;
            var lastShopDepth = depth;

            if (shopCount > 1)
            {
                SetShopAtDepth(firstShopDepth);
                shopCount--;
            }

            if (shopCount > 1)
            {
                SetShopAtDepth(lastShopDepth);
                shopCount--;
            }


            var dFirstLastShop = lastShopDepth - firstShopDepth;
            var shopFrequency = dFirstLastShop / (shopCount + 1);

            for (int i = 1; i < dFirstLastShop; i++)
            {
                if (i % shopFrequency == 0)
                {
                    SetShopAtDepth(firstShopDepth + i);
                    shopCount--;
                }

                if (shopCount <= 0)
                    break;
            }

            void SetShopAtDepth(int depth)
            {
                var nodesOnThisDepth = generationInfo.Nodes.Where(n => n.Depth == depth).ToArray();

                if (nodesOnThisDepth.Any())
                {
                    var ran = random.Next(0, nodesOnThisDepth.Length);
                    //nodesOnThisDepth[ran].SetType(LevelSelectionNode.NodeType.Shop);
                    for (var i = 0; i < nodesOnThisDepth.Length; i++)
                    {
                        var node = nodesOnThisDepth[i];
                        if (i == ran || random.NextFloat() > .5f)
                            node.SetType(LevelSelectionNode.NodeType.Shop);
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
    }

}