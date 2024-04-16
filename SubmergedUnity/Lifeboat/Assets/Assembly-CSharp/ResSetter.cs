using System;
using System.IO;
using UnityEngine;

public class ResSetter : MonoBehaviour
{
	public int Width = 1438;

	public int Height = 810;

	private int cnt;

	public void Start()
	{
		Screen.SetResolution(this.Width, this.Height, false);
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			Directory.CreateDirectory("C:\\AmongUsSS");
			string format = "C:\\AmongUsSS\\Screenshot-{0}.png";
			int num = this.cnt;
			this.cnt = num + 1;
			ScreenCapture.CaptureScreenshot(string.Format(format, num));
		}
	}
}
