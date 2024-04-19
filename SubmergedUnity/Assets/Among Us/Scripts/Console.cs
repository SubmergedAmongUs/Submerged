using JetBrains.Annotations;
using UnityEngine;

public class Console : MonoBehaviour
{
	[UsedImplicitly]
	public void Use()
	{
	}

	public float usableDistance = 1f;
	public int ConsoleId;
	public bool onlyFromBelow;
	public bool onlySameRoom;
	public bool checkWalls;
	public bool GhostsIgnored;
	public bool AllowImpostor;
	public SystemTypes Room;
	public TaskTypes[] TaskTypes;
	public TaskSet[] ValidTasks;
	public SpriteRenderer Image;
}
