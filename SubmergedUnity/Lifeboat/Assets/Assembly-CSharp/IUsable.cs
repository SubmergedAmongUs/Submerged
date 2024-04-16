using System;

public interface IUsable
{
	float UsableDistance { get; }

	float PercentCool { get; }

	ImageNames UseIcon { get; }

	void SetOutline(bool on, bool mainTarget);

	float CanUse(GameData.PlayerInfo pc, out bool canUse, out bool couldUse);

	void Use();
}
