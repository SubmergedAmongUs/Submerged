using System;

public interface ICriticalSabotage
{
	bool IsActive { get; }

	float Countdown { get; }

	int UserCount { get; }

	void ClearSabotage();
}
