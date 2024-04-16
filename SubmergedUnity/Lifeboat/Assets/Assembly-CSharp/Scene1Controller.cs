using System;
using System.Collections;
using UnityEngine;

public class Scene1Controller : SceneController
{
	public PlayerAnimator[] players;

	public DummyConsole[] Consoles;

	public Vector2[] WayPoints;

	public Camera backupCam;

	public void OnDrawGizmos()
	{
		for (int i = 0; i < this.WayPoints.Length; i++)
		{
			Vector2 vector = this.WayPoints[i];
			Vector2 vector2 = this.WayPoints[(i + 1) % this.WayPoints.Length];
			Gizmos.DrawLine(vector, vector2);
		}
	}

	public void OnEnable()
	{
		this.backupCam.cullingMask = 0;
		base.StartCoroutine(this.RunPlayer(0));
		if (this.players.Length > 1)
		{
			base.StartCoroutine(this.RunPlayer(1));
		}
	}

	public void OnDisable()
	{
		this.backupCam.cullingMask = (int.MaxValue ^ LayerMask.GetMask(new string[]
		{
			"UI"
		}));
	}

	private IEnumerator RunPlayer(int idx)
	{
		PlayerAnimator myPlayer = this.players[idx];
		for (;;)
		{
			int num;
			for (int i = 0; i < this.WayPoints.Length; i = num)
			{
				bool willInterrupt = i == 2 || i == 5;
				yield return myPlayer.WalkPlayerTo(this.WayPoints[i], willInterrupt, 0.1f);
				if (willInterrupt)
				{
					yield return this.DoUse(idx, (i == 2) ? 0 : 1);
				}
				num = i + 1;
			}
		}
		yield break;
	}

	private IEnumerator DoUse(int idx, int consoleid)
	{
		PlayerAnimator myPlayer = this.players[idx];
		yield return Scene1Controller.WaitForSeconds(0.2f);
		if (idx == 0)
		{
			yield return myPlayer.finger.MoveTo(myPlayer.UseButton.transform.position, 0.75f);
		}
		else
		{
			yield return myPlayer.finger.MoveTo(this.Consoles[consoleid].transform.position, 0.75f);
		}
		yield return Scene1Controller.WaitForSeconds(0.2f);
		yield return myPlayer.finger.DoClick(0.4f);
		yield return Scene1Controller.WaitForSeconds(0.2f);
		if (!(myPlayer.joystick is DemoKeyboardStick))
		{
			yield return myPlayer.finger.MoveTo(myPlayer.joystick.transform.position, 0.75f);
		}
		else
		{
			yield return Scene1Controller.WaitForSeconds(0.75f);
		}
		yield break;
	}

	public static IEnumerator WaitForSeconds(float duration)
	{
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			yield return null;
		}
		yield break;
	}
}
