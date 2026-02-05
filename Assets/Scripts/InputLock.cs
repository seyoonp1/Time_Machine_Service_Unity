public static class InputLock
{
    private static int _lockCount;

    public static bool IsLocked => _lockCount > 0;

    public static void Lock()
    {
        _lockCount++;
    }

    public static void Unlock()
    {
        if (_lockCount <= 0)
        {
            _lockCount = 0;
            return;
        }

        _lockCount--;
    }
}
