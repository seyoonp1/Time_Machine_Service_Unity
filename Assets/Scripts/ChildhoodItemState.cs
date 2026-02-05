public static class ChildhoodItemState
{
    public static bool HasBearHead { get; private set; }
    public static bool HasSewingTool { get; private set; }
    public static bool IsReady => HasBearHead && HasSewingTool;

    public static bool MarkBearHead()
    {
        if (HasBearHead)
        {
            return false;
        }

        HasBearHead = true;
        return true;
    }

    public static bool MarkSewingTool()
    {
        if (HasSewingTool)
        {
            return false;
        }

        HasSewingTool = true;
        return true;
    }
}
