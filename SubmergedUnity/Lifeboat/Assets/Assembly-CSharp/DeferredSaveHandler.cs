using System;
using System.Collections.Generic;

public class DeferredSaveHandler : DestroyableSingleton<DeferredSaveHandler>
{
	public bool savePlayerPrefs;

	public bool saveSecureData;

	public bool saveAnnouncements;

	public bool saveQuickChatFavorites;

	public List<DeferredSaveHandler.GameOptionsSaveRequest> saveGameOptions = new List<DeferredSaveHandler.GameOptionsSaveRequest>();

	public struct GameOptionsSaveRequest : IEquatable<DeferredSaveHandler.GameOptionsSaveRequest>
	{
		public string filename;

		public GameOptionsData data;

		public bool Equals(DeferredSaveHandler.GameOptionsSaveRequest other)
		{
			return this.filename.Equals(other.filename);
		}
	}
}
