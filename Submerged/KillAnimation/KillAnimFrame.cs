using System.Globalization;
using UnityEngine;

namespace Submerged.KillAnimation;

public struct KillAnimFrame
{
    public int animation;
    public float time;
    public int length;
    public Vector2 offset;

    public static string Serialize(KillAnimFrame frame) => $"{frame.animation},{frame.time},{frame.length},{frame.offset.x},{frame.offset.y}";

    public static KillAnimFrame Deserialize(string dataString)
    {
        string[] data = dataString.Split(',');

        return new KillAnimFrame
        {
            animation = int.Parse(data[0], CultureInfo.InvariantCulture),
            time = float.Parse(data[1], CultureInfo.InvariantCulture),
            length = int.Parse(data[2], CultureInfo.InvariantCulture),
            offset = new Vector2(float.Parse(data[3], CultureInfo.InvariantCulture), float.Parse(data[4], CultureInfo.InvariantCulture))
        };
    }
}
