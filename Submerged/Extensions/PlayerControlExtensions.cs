using UnityEngine;

public static class PlayerControlExtensions
{
    public static bool IsHorse(this PlayerControl pc)
    {
    return pc.MyPhysics.bodyType == PlayerBodyTypes.Horse;
    }
    public static bool IsLong(this PlayerControl pc)
    {
    return (pc.MyPhysics.bodyType == PlayerBodyTypes.Long || pc.MyPhysics.bodyType == PlayerBodyTypes.LongSeeker);
    }
}
