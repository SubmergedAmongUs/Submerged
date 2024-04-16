using System;
using UnityEngine;

public class AspectPosition : MonoBehaviour
{
	public Camera parentCam;

	private const int LeftFlag = 1;

	private const int RightFlag = 2;

	private const int BottomFlag = 4;

	private const int TopFlag = 8;

	public bool updateAlways;

	public Vector3 DistanceFromEdge;

	public AspectPosition.EdgeAlignments Alignment;

	public void Update()
	{
		if (this.updateAlways)
		{
			this.AdjustPosition();
		}
	}

	private void OnEnable()
	{
		this.parentCam = (this.parentCam ? this.parentCam : Camera.main);
		this.AdjustPosition();
		ResolutionManager.ResolutionChanged += this.AdjustPosition;
	}

	internal void SetNormalizedX(float nx, float widthPadding)
	{
		float num = this.parentCam.orthographicSize * this.parentCam.aspect + widthPadding;
		float x = this.DistanceFromEdge.x;
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = Mathf.Lerp(x, num * 2f - x, nx);
		int alignment = (int)this.Alignment;
		if ((alignment & 1) != 0)
		{
			localPosition.x -= num;
		}
		else if ((alignment & 2) != 0)
		{
			localPosition.x = num - localPosition.x;
		}
		base.transform.localPosition = localPosition;
	}

	private void OnDisable()
	{
		ResolutionManager.ResolutionChanged -= this.AdjustPosition;
	}

	public void AdjustPosition()
	{
		Rect safeArea = Screen.safeArea;
		float aspect = Mathf.Min((this.parentCam ? this.parentCam : Camera.main).aspect, safeArea.width / safeArea.height);
		this.AdjustPosition(aspect);
	}

	public void AdjustPosition(float aspect)
	{
		float orthographicSize = (this.parentCam ? this.parentCam : Camera.main).orthographicSize;
		base.transform.localPosition = AspectPosition.ComputePosition(this.Alignment, this.DistanceFromEdge, orthographicSize, aspect);
	}

	public static Vector3 ComputeWorldPosition(Camera cam, AspectPosition.EdgeAlignments alignment, Vector3 relativePos)
	{
		Rect safeArea = Screen.safeArea;
		float aspect = Mathf.Min(cam.aspect, safeArea.width / safeArea.height);
		float orthographicSize = cam.orthographicSize;
		return cam.transform.TransformPoint(AspectPosition.ComputePosition(alignment, relativePos, orthographicSize, aspect));
	}

	public static Vector3 ComputePosition(AspectPosition.EdgeAlignments alignment, Vector3 relativePos)
	{
		Rect safeArea = Screen.safeArea;
		Camera main = Camera.main;
		float aspect = Mathf.Min(main.aspect, safeArea.width / safeArea.height);
		float orthographicSize = main.orthographicSize;
		return AspectPosition.ComputePosition(alignment, relativePos, orthographicSize, aspect);
	}

	public static Vector3 ComputePosition(AspectPosition.EdgeAlignments alignment, Vector3 relativePos, float cHeight, float aspect)
	{
		float num = cHeight * aspect;
		Vector3 vector = relativePos;
		if ((alignment & AspectPosition.EdgeAlignments.Left) != (AspectPosition.EdgeAlignments)0)
		{
			vector.x -= num;
		}
		else if ((alignment & AspectPosition.EdgeAlignments.Right) != (AspectPosition.EdgeAlignments)0)
		{
			vector.x = num - vector.x;
		}
		if ((alignment & AspectPosition.EdgeAlignments.Bottom) != (AspectPosition.EdgeAlignments)0)
		{
			vector.y -= cHeight;
		}
		else if ((alignment & AspectPosition.EdgeAlignments.Top) != (AspectPosition.EdgeAlignments)0)
		{
			vector.y = cHeight - vector.y;
		}
		return vector;
	}

	public enum EdgeAlignments
	{
		RightBottom = 6,
		LeftBottom = 5,
		RightTop = 10,
		Left = 1,
		Right,
		Top = 8,
		Bottom = 4,
		LeftTop = 9
	}
}
