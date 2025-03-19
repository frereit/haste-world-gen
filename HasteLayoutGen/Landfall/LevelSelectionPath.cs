namespace HasteLayoutGen.Landfall
{
    public class LevelSelectionPath
    {
        public LevelSelectionPath(LevelSelectionNode from, LevelSelectionNode to)
        {
            From = from;
            To = to;
        }

        public LevelSelectionNode From;
        public LevelSelectionNode To;
        public bool Intersects;
    }
}
