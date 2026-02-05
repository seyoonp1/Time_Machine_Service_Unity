using UnityEngine;

public static class SceneReturnState
{
    public static bool HasReturnPosition { get; private set; }
    public static Vector3 ReturnPosition { get; private set; }
    public static bool HasReturnTime { get; private set; }
    public static TimeSlot ReturnTime { get; private set; }

    public static void StoreReturnPosition(Vector3 position)
    {
        ReturnPosition = position;
        HasReturnPosition = true;
    }

    public static void StoreReturnTime(TimeSlot timeSlot)
    {
        ReturnTime = timeSlot;
        HasReturnTime = true;
    }

    public static void Clear()
    {
        HasReturnPosition = false;
        HasReturnTime = false;
        ReturnPosition = Vector3.zero;
        ReturnTime = TimeSlot.T1;
    }
}
