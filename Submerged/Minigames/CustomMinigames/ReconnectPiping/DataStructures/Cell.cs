using Submerged.Minigames.CustomMinigames.ReconnectPiping.MonoBehaviours;

namespace Submerged.Minigames.CustomMinigames.ReconnectPiping.DataStructures;

public sealed class Cell
{
    public Wall bottomWall;

    public Wall leftWall;

    public PipeCell pipe;
    public Wall rightWall;
    public Wall topWall;
    public bool visited = false;
}
