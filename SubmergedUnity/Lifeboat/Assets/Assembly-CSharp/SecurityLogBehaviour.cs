using System;
using System.Collections.Generic;
using UnityEngine;

public class SecurityLogBehaviour : MonoBehaviour
{
	public const byte ConsoleMask = 240;

	public const byte PlayerMask = 15;

	public Color[] BarColors = new Color[]
	{
		new Color32(33, 77, 173, 128),
		new Color32(173, 81, 16, 128),
		new Color32(16, 97, 8, 128)
	};

	public readonly List<SecurityLogBehaviour.SecurityLogEntry> LogEntries = new List<SecurityLogBehaviour.SecurityLogEntry>();

	public bool HasNew;

	public void LogPlayer(PlayerControl player, SecurityLogBehaviour.SecurityLogLocations location)
	{
		this.HasNew = true;
		this.LogEntries.Add(new SecurityLogBehaviour.SecurityLogEntry(player.PlayerId, location));
		if (this.LogEntries.Count > 20)
		{
			this.LogEntries.RemoveAt(0);
		}
	}

	public enum SecurityLogLocations
	{
		North,
		Southeast,
		Southwest
	}

	public struct SecurityLogEntry
	{
		public byte PlayerId;

		public SecurityLogBehaviour.SecurityLogLocations Location;

		public SecurityLogEntry(byte playerId, SecurityLogBehaviour.SecurityLogLocations location)
		{
			this.PlayerId = playerId;
			this.Location = location;
		}
	}
}
