using System;
using UnityEngine;

public interface IFocusHolder
{
	void GiveFocus();

	void LoseFocus();

	bool CheckCollision(Vector2 pt);
}
