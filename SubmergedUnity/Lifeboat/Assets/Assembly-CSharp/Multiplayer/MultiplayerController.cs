using System;
using System.ComponentModel;

namespace Multiplayer
{
	[Obsolete("This is from the XDK, please use the GDK versions instead")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	internal class MultiplayerController : IDisposable
	{
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}
	}
}
