using JetBrains.Annotations;
using UnityEngine;

public class Vent : MonoBehaviour
{
	[UsedImplicitly]
	public void ClickRight()
	{
	}

	[UsedImplicitly]
	public void ClickLeft()
	{
	}

	[UsedImplicitly]
	public void ClickCenter()
	{
	}

	[UsedImplicitly]
	public void Use()
	{
	}

	public int Id;
	public Vent Left;
	public Vent Right;
	public Vent Center;
	public ButtonBehavior[] Buttons;
	public GameObject[] CleaningIndicators;
	public AnimationClip EnterVentAnim;
	public AnimationClip ExitVentAnim;
	public Vector3 Offset = new Vector3(0f, 0.3636057f, 0f);
	public float spreadAmount;
	public float spreadShift;
}
