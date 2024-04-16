using System;

public interface IStepWatcher
{
	int Priority { get; }

	SoundGroup MakeFootstep(PlayerControl player);
}
