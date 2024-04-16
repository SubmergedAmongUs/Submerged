using System;
using System.Collections.Generic;
using InnerNet;
using UnityEngine;
using UnityEngine.Analytics;

namespace Assets.CoreScripts
{
	public class Telemetry : DestroyableSingleton<Telemetry>
	{
		private static readonly string[] ColorNames = new string[]
		{
			"Red",
			"Blue",
			"Green",
			"Pink",
			"Orange",
			"Yellow",
			"Black",
			"White",
			"Purple",
			"Brown",
			"Cyan",
			"Lime",
			"Mahogany",
			"Rose",
			"Banana",
			"Gray",
			"Tan",
			"Lilac",
			"Aqua",
			"Cobalt",
			"Barf",
			"Salmon",
			"Chocolate"
		};

		private bool amHost;

		private bool gameStarted;

		private DateTime timeStarted;

		public void Init()
		{
			this.gameStarted = true;
		}

		public void StartGame(bool isHost, int playerCount, int impostorCount, GameModes gameMode, uint timesImpostor, uint gamesPlayed, uint crewStreak)
		{
			this.amHost = isHost;
			this.timeStarted = DateTime.UtcNow;
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"Platform",
					Application.platform
				},
				{
					"TimesImpostor",
					timesImpostor
				},
				{
					"CrewStreak",
					crewStreak
				},
				{
					"GamesPlayed",
					gamesPlayed
				},
				{
					"GameMode",
					gameMode
				}
			};
			if (this.amHost)
			{
				dictionary.Add("PlayerCount", playerCount);
				dictionary.Add("InfectedCount", impostorCount);
			}
			Analytics.CustomEvent("StartGame", dictionary);
		}

		public void StartGameCosmetics(int colorId, uint hatId, uint skinId, uint petId)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"Color",
					Telemetry.ColorNames[colorId]
				},
				{
					"Hat",
					DestroyableSingleton<HatManager>.Instance.GetHatById(hatId).name
				},
				{
					"Skin",
					DestroyableSingleton<HatManager>.Instance.GetSkinById(skinId).name
				},
				{
					"Pet",
					DestroyableSingleton<HatManager>.Instance.GetPetById(petId).name
				}
			};
			Analytics.CustomEvent("StartGameCosmetics", dictionary);
		}

		public void WriteMeetingStarted(bool isEmergency)
		{
			if (!this.amHost)
			{
				return;
			}
			Analytics.CustomEvent("MeetingStarted", new Dictionary<string, object>
			{
				{
					"IsEmergency",
					isEmergency
				}
			});
		}

		public void WriteMeetingEnded(float duration)
		{
			if (!this.amHost)
			{
				return;
			}
			Analytics.CustomEvent("MeetingEnded", new Dictionary<string, object>
			{
				{
					"IsEmergency",
					duration
				}
			});
		}

		public void WritePosition(byte playerNum, Vector2 worldPos)
		{
		}

		public void WriteMurder()
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("Murder");
		}

		public void WriteSabotageUsed(SystemTypes systemType)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("SabotageUsed", new Dictionary<string, object>
			{
				{
					"SystemType",
					systemType
				}
			});
		}

		public void CardSwipeComplete(int attempts)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("CardSwipeComplete", new Dictionary<string, object>
			{
				{
					"Attempts",
					attempts
				}
			});
		}

		public void WriteUse(byte playerNum, TaskTypes taskType, Vector3 worldPos)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("ConsoleUsed", new Dictionary<string, object>
			{
				{
					"TaskType",
					taskType
				}
			});
		}

		public void WriteCompleteTask(TaskTypes taskType)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("TaskComplete", new Dictionary<string, object>
			{
				{
					"TaskType",
					taskType
				}
			});
		}

		internal void WriteDisconnect(DisconnectReasons reason)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("Disconnect", new Dictionary<string, object>
			{
				{
					"Reason",
					reason
				}
			});
		}

		public void EndGame(GameOverReason endReason)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Dictionary<string, object> dictionary = new Dictionary<string, object>
			{
				{
					"Reason",
					endReason
				}
			};
			if (this.amHost)
			{
				dictionary.Add("DurationSec", (DateTime.UtcNow - this.timeStarted).TotalSeconds);
			}
			Analytics.CustomEvent("EndGame", dictionary);
		}

		public void SendWho()
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("SentWho");
		}

		public void WonGame(int colorId, uint hatId)
		{
			if (!this.gameStarted)
			{
				return;
			}
			Analytics.CustomEvent("WonGame", new Dictionary<string, object>
			{
				{
					"Color",
					Telemetry.ColorNames[colorId]
				},
				{
					"Hat",
					DestroyableSingleton<HatManager>.Instance.GetHatById(hatId).name
				}
			});
		}
	}
}
