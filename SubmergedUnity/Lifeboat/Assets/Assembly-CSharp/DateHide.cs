using System;
using UnityEngine;

public class DateHide : MonoBehaviour
{
	public int MonthStart;

	public int DayStart;

	public int MonthEnd;

	public int DayEnd;

	private void Awake()
	{
		try
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime t = new DateTime(utcNow.Year, this.MonthStart, this.DayStart);
			DateTime t2 = new DateTime(utcNow.Year, this.MonthEnd, this.DayEnd);
			if (t <= utcNow && utcNow <= t2)
			{
				return;
			}
		}
		catch
		{
		}
		base.gameObject.SetActive(false);
	}
}
