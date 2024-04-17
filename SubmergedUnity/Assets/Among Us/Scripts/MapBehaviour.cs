using JetBrains.Annotations;
using UnityEngine;

public class MapBehaviour : MonoBehaviour
{
	[UsedImplicitly]
	public void Close()
	{
	}

	public AlphaPulse ColorControl;
	public SpriteRenderer HerePoint;
	public MapCountOverlay countOverlay;
	public InfectedOverlay infectedOverlay;
	public MapTaskOverlay taskOverlay;
	[SerializeField]
	private GameObject fadedBackground;
}
