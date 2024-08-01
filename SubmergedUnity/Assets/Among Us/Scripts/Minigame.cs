using JetBrains.Annotations;
using UnityEngine;

public abstract class Minigame : MonoBehaviour
{
	[UsedImplicitly]
	public virtual void Close()
	{
	}

	public TransitionType TransType;
	public AudioClip OpenSound;
	public AudioClip CloseSound;

	protected enum CloseState
	{
		None,
		Waiting,
		Closing
	}
}
