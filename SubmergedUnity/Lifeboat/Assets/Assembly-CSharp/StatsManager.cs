using System;
using System.IO;
using UnityEngine;

public class StatsManager
{
	public static StatsManager Instance = new StatsManager();

	private const byte StatsVersion = 3;

	private bool loadedStats;

	private uint bodiesReported;

	private uint emergenciesCalls;

	private uint tasksCompleted;

	private uint completedAllTasks;

	private uint sabsFixed;

	private uint impostorKills;

	private uint timesMurdered;

	private uint timesEjected;

	private uint crewmateStreak;

	private uint timesImpostor;

	private uint timesCrewmate;

	private uint gamesStarted;

	private uint gamesFinished;

	private float banPoints;

	private long lastGameStarted;

	private const int PointsUntilBanStarts = 2;

	private const int MinutesPerBanPoint = 5;

	private uint[] WinReasons = new uint[7];

	private uint[] DrawReasons = new uint[7];

	private uint[] LoseReasons = new uint[7];

	public uint BodiesReported
	{
		get
		{
			this.LoadStats();
			return this.bodiesReported;
		}
		set
		{
			this.LoadStats();
			this.bodiesReported = value;
			this.SaveStats();
		}
	}

	public uint EmergenciesCalled
	{
		get
		{
			this.LoadStats();
			return this.emergenciesCalls;
		}
		set
		{
			this.LoadStats();
			this.emergenciesCalls = value;
			this.SaveStats();
		}
	}

	public uint TasksCompleted
	{
		get
		{
			this.LoadStats();
			return this.tasksCompleted;
		}
		set
		{
			this.LoadStats();
			this.tasksCompleted = value;
			this.SaveStats();
		}
	}

	public uint CompletedAllTasks
	{
		get
		{
			this.LoadStats();
			return this.completedAllTasks;
		}
		set
		{
			this.LoadStats();
			this.completedAllTasks = value;
			this.SaveStats();
		}
	}

	public uint SabsFixed
	{
		get
		{
			this.LoadStats();
			return this.sabsFixed;
		}
		set
		{
			this.LoadStats();
			this.sabsFixed = value;
			this.SaveStats();
		}
	}

	public uint ImpostorKills
	{
		get
		{
			this.LoadStats();
			return this.impostorKills;
		}
		set
		{
			this.LoadStats();
			this.impostorKills = value;
			this.SaveStats();
		}
	}

	public uint TimesMurdered
	{
		get
		{
			this.LoadStats();
			return this.timesMurdered;
		}
		set
		{
			this.LoadStats();
			this.timesMurdered = value;
			this.SaveStats();
		}
	}

	public uint TimesEjected
	{
		get
		{
			this.LoadStats();
			return this.timesEjected;
		}
		set
		{
			this.LoadStats();
			this.timesEjected = value;
			this.SaveStats();
		}
	}

	public uint CrewmateStreak
	{
		get
		{
			this.LoadStats();
			return this.crewmateStreak;
		}
		set
		{
			this.LoadStats();
			this.crewmateStreak = value;
			this.SaveStats();
		}
	}

	public uint TimesImpostor
	{
		get
		{
			this.LoadStats();
			return this.timesImpostor;
		}
		set
		{
			this.LoadStats();
			this.timesImpostor = value;
			this.SaveStats();
		}
	}

	public uint TimesCrewmate
	{
		get
		{
			this.LoadStats();
			return this.timesCrewmate;
		}
		set
		{
			this.LoadStats();
			this.timesCrewmate = value;
			this.SaveStats();
		}
	}

	public uint GamesStarted
	{
		get
		{
			this.LoadStats();
			return this.gamesStarted;
		}
		set
		{
			this.LoadStats();
			this.gamesStarted = value;
			this.SaveStats();
		}
	}

	public uint GamesFinished
	{
		get
		{
			this.LoadStats();
			return this.gamesFinished;
		}
		set
		{
			this.LoadStats();
			this.gamesFinished = value;
			this.SaveStats();
		}
	}

	public float BanPoints
	{
		get
		{
			this.LoadStats();
			return this.banPoints;
		}
		set
		{
			this.LoadStats();
			this.banPoints = Mathf.Max(0f, value);
			this.SaveStats();
		}
	}

	public DateTime LastGameStarted
	{
		get
		{
			this.LoadStats();
			return new DateTime(this.lastGameStarted);
		}
		set
		{
			this.LoadStats();
			this.lastGameStarted = value.Ticks;
			this.SaveStats();
		}
	}

	public float BanMinutes
	{
		get
		{
			return Mathf.Max(this.BanPoints - 2f, 0f) * 5f;
		}
	}

	public bool AmBanned
	{
		get
		{
			return this.BanMinutesLeft > 0;
		}
	}

	public int BanMinutesLeft
	{
		get
		{
			TimeSpan timeSpan = DateTime.UtcNow - this.LastGameStarted;
			int num = Mathf.CeilToInt(this.BanMinutes - (float)timeSpan.TotalMinutes);
			if (num > 1440 || timeSpan.TotalDays > 1.0)
			{
				this.BanPoints = 0f;
				return 0;
			}
			return num;
		}
	}

	public void AddDrawReason(GameOverReason reason)
	{
		this.LoadStats();
		this.DrawReasons[(int)reason] += 1U;
		this.SaveStats();
	}

	public void AddWinReason(GameOverReason reason)
	{
		this.LoadStats();
		this.WinReasons[(int)reason] += 1U;
		this.SaveStats();
	}

	public uint GetWinReason(GameOverReason reason)
	{
		this.LoadStats();
		return this.WinReasons[(int)reason];
	}

	public void AddLoseReason(GameOverReason reason)
	{
		this.LoadStats();
		this.LoseReasons[(int)reason] += 1U;
		this.SaveStats();
	}

	public uint GetLoseReason(GameOverReason reason)
	{
		this.LoadStats();
		return this.LoseReasons[(int)reason];
	}

	protected virtual void LoadStats()
	{
		if (this.loadedStats)
		{
			return;
		}
		this.loadedStats = true;
		Debug.Log("LoadStats");
		string path = Path.Combine(PlatformPaths.persistentDataPath, "playerStats2");
		if (FileIO.Exists(path))
		{
			try
			{
				using (BinaryReader binaryReader = new BinaryReader(File.OpenRead(path)))
				{
					byte b = binaryReader.ReadByte();
					this.bodiesReported = binaryReader.ReadUInt32();
					this.emergenciesCalls = binaryReader.ReadUInt32();
					this.tasksCompleted = binaryReader.ReadUInt32();
					this.completedAllTasks = binaryReader.ReadUInt32();
					this.sabsFixed = binaryReader.ReadUInt32();
					this.impostorKills = binaryReader.ReadUInt32();
					this.timesMurdered = binaryReader.ReadUInt32();
					this.timesEjected = binaryReader.ReadUInt32();
					this.crewmateStreak = binaryReader.ReadUInt32();
					this.timesImpostor = binaryReader.ReadUInt32();
					this.timesCrewmate = binaryReader.ReadUInt32();
					this.gamesStarted = binaryReader.ReadUInt32();
					this.gamesFinished = binaryReader.ReadUInt32();
					for (int i = 0; i < this.WinReasons.Length; i++)
					{
						this.WinReasons[i] = binaryReader.ReadUInt32();
					}
					for (int j = 0; j < this.LoseReasons.Length; j++)
					{
						this.LoseReasons[j] = binaryReader.ReadUInt32();
					}
					if (b > 1)
					{
						for (int k = 0; k < this.DrawReasons.Length; k++)
						{
							this.DrawReasons[k] = binaryReader.ReadUInt32();
						}
					}
					if (b > 2)
					{
						this.banPoints = binaryReader.ReadSingle();
						this.lastGameStarted = binaryReader.ReadInt64();
					}
				}
			}
			catch
			{
				Debug.LogError("Deleting corrupted stats file");
				File.Delete(path);
			}
		}
	}

	protected virtual void SaveStats()
	{
		Debug.Log("SaveStats");
		try
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(File.OpenWrite(Path.Combine(PlatformPaths.persistentDataPath, "playerStats2"))))
			{
				binaryWriter.Write(3);
				binaryWriter.Write(this.bodiesReported);
				binaryWriter.Write(this.emergenciesCalls);
				binaryWriter.Write(this.tasksCompleted);
				binaryWriter.Write(this.completedAllTasks);
				binaryWriter.Write(this.sabsFixed);
				binaryWriter.Write(this.impostorKills);
				binaryWriter.Write(this.timesMurdered);
				binaryWriter.Write(this.timesEjected);
				binaryWriter.Write(this.crewmateStreak);
				binaryWriter.Write(this.timesImpostor);
				binaryWriter.Write(this.timesCrewmate);
				binaryWriter.Write(this.gamesStarted);
				binaryWriter.Write(this.gamesFinished);
				for (int i = 0; i < this.WinReasons.Length; i++)
				{
					binaryWriter.Write(this.WinReasons[i]);
				}
				for (int j = 0; j < this.LoseReasons.Length; j++)
				{
					binaryWriter.Write(this.LoseReasons[j]);
				}
				for (int k = 0; k < this.DrawReasons.Length; k++)
				{
					binaryWriter.Write(this.DrawReasons[k]);
				}
				binaryWriter.Write(this.banPoints);
				binaryWriter.Write(this.lastGameStarted);
			}
		}
		catch (Exception ex)
		{
			Debug.Log("Failed to write out stats: " + ex.Message);
		}
	}
}
