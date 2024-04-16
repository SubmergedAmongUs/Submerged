using System;

public static class RewiredConstsEnum
{
	public enum Action
	{
		Pause = 1,
		MoveHorizontal,
		MoveVertical,
		ToggleMap,
		ToggleTasks,
		ActionPrimary,
		ActionSecondary = 8,
		ActionTertiary = 7,
		MenuHorizontal = 9,
		MenuVertical,
		MenuConfirm,
		MenuCancel,
		TaskLHorizontal,
		TaskLVertical,
		TaskRHorizontal = 16,
		TaskRVertical,
		MenuChat,
		ButtonStart,
		TaskLT = 24,
		TaskLB = 20,
		TaskRT,
		TaskRB,
		MenuUp = 25,
		MenuDown,
		MenuRight,
		MenuLeft,
		TaskConfirmAlt,
		MenuHorizontalAlt,
		MenuVerticalAlt,
		TaskConfirmTertiary,
		ButtonKeyboard,
		MenuRT,
		MenuLT,
		ButtonBan,
		AccountPicker
	}
}
