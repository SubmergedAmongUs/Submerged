using UnityEngine;

public abstract class PlayerTask : MonoBehaviour
{
	public SystemTypes StartAt;
	public TaskTypes TaskType;
	public Minigame MinigamePrefab;
	public bool HasLocation;
	public bool LocationDirty = true;
}
