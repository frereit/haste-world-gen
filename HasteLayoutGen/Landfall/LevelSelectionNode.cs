using System.Numerics;

namespace HasteLayoutGen.Landfall
{
    public class LevelSelectionNode
    {
        public enum NodeType
        {
            Default,
            Shop,
            Challenge,
            Encounter,
            Boss,
            RestStop
        }
        public int Depth;
        public NodeType Type;
        public Vector3 Position;

        internal void SetType(NodeType type)
        {
            Type = type;
        }
    }
}
