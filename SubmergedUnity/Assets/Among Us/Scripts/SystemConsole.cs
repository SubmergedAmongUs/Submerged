using JetBrains.Annotations;
using UnityEngine;

public class SystemConsole : MonoBehaviour
{
	[UsedImplicitly]
	public void Use()
	{
	}

	public ImageNames useIcon = ImageNames.UseButton;
	public float usableDistance = 1f;
	public bool FreeplayOnly;
	public bool onlyFromBelow;
	public SpriteRenderer Image;
	public Minigame MinigamePrefab;
}
