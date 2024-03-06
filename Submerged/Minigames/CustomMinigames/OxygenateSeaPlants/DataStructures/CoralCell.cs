namespace Submerged.Minigames.CustomMinigames.OxygenateSeaPlants.DataStructures;

public sealed class CoralCell
{
    public CoralWall bottomWall;

    public CoralWall leftWall;
    public CoralWall rightWall;
    public CoralWall topWall;
    public bool visited = false;

    /*public void SetState(Walls direction, List<Sprite> validSprites, Transform transform)
    {
        if (!transform) return;
        bool wallThere = false;

        switch (direction)
        {
            case Walls.North:
                wallThere = topWall.exists;

                break;
            case Walls.East:
                wallThere = rightWall.exists;

                break;
            case Walls.South:
                wallThere = bottomWall.exists;

                break;
            case Walls.West:
                wallThere = leftWall.exists;

                break;
        }

        transform.GetComponent<BoxCollider2D>().isTrigger = !wallThere;
        if (wallThere) transform.GetComponent<SpriteRenderer>().sprite = validSprites.Random();
    }

    public bool GetThreeWalls()
    {
        int walls = 0;
        walls += leftWall.exists ? 1 : 0;
        walls += rightWall.exists ? 1 : 0;
        walls += bottomWall.exists ? 1 : 0;
        walls += topWall.exists ? 1 : 0;

        return walls == 3;
    }*/
}
