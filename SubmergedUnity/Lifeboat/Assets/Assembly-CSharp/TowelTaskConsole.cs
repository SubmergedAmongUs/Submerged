using System;

public class TowelTaskConsole : AutoTaskConsole
{
	protected override void AfterUse(NormalPlayerTask task)
	{
		if (this.useSound && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.useSound, false, 1f);
		}
		int num = task.Data.IndexOf((byte b) => (int)b == this.ConsoleId);
		task.Data[num] = 250;
		this.Image.color = Palette.ClearWhite;
	}
}
