using System;
using System.Collections.Generic;

public abstract class SabotageTask : PlayerTask
{
	protected bool didContribute;

	public ArrowBehaviour[] Arrows;

	public void MarkContributed()
	{
		this.didContribute = true;
	}

	protected void SetupArrows()
	{
		if (base.Owner.AmOwner)
		{
			List<global::Console> list = base.FindConsoles();
			for (int i = 0; i < list.Count; i++)
			{
				int consoleId = list[i].ConsoleId;
				this.Arrows[consoleId].target = list[i].transform.position;
				this.Arrows[consoleId].gameObject.SetActive(true);
			}
			return;
		}
		for (int j = 0; j < this.Arrows.Length; j++)
		{
			this.Arrows[j].gameObject.SetActive(false);
		}
	}
}
