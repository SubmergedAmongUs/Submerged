using System;

namespace Microsoft.Xbox
{
	public class GameSaveLoadedArgs : EventArgs
	{
		public byte[] Data { get; private set; }

		public GameSaveLoadedArgs(byte[] data)
		{
			this.Data = data;
		}
	}
}
