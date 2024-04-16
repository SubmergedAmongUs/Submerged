using System;
using UnityEngine;

public static class Palette
{
	public static readonly Color AcceptedGreen = new Color32(0, byte.MaxValue, 42, byte.MaxValue);

	public static readonly Color DisabledGrey = new Color(0.3f, 0.3f, 0.3f, 1f);

	public static readonly Color DisabledClear = new Color(1f, 1f, 1f, 0.3f);

	public static readonly Color EnabledColor = new Color(1f, 1f, 1f, 1f);

	public static readonly Color Black = new Color(0f, 0f, 0f, 1f);

	public static readonly Color ClearWhite = new Color(1f, 1f, 1f, 0f);

	public static readonly Color HalfWhite = new Color(1f, 1f, 1f, 0.5f);

	public static readonly Color White = new Color(1f, 1f, 1f, 1f);

	public static readonly Color LightBlue = new Color(0.5f, 0.5f, 1f);

	public static readonly Color Blue = new Color(0.2f, 0.2f, 1f);

	public static readonly Color Orange = new Color(1f, 0.6f, 0.005f);

	public static readonly Color Purple = new Color(0.6f, 0.1f, 0.6f);

	public static readonly Color Brown = new Color(0.72f, 0.43f, 0.11f);

	public static readonly Color CrewmateBlue = new Color32(140, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public static readonly Color ImpostorRed = new Color32(byte.MaxValue, 25, 25, byte.MaxValue);

	public static readonly StringNames[] ColorNames = new StringNames[]
	{
		StringNames.ColorRed,
		StringNames.ColorBlue,
		StringNames.ColorGreen,
		StringNames.ColorPink,
		StringNames.ColorOrange,
		StringNames.ColorYellow,
		StringNames.ColorBlack,
		StringNames.ColorWhite,
		StringNames.ColorPurple,
		StringNames.ColorBrown,
		StringNames.ColorCyan,
		StringNames.ColorLime,
		StringNames.ColorMaroon,
		StringNames.ColorRose,
		StringNames.ColorBanana,
		StringNames.ColorGray,
		StringNames.ColorTan,
		StringNames.ColorCoral
	};

	public static readonly Color32[] PlayerColors = new Color32[]
	{
		new Color32(198, 17, 17, byte.MaxValue),
		new Color32(19, 46, 210, byte.MaxValue),
		new Color32(17, 128, 45, byte.MaxValue),
		new Color32(238, 84, 187, byte.MaxValue),
		new Color32(240, 125, 13, byte.MaxValue),
		new Color32(246, 246, 87, byte.MaxValue),
		new Color32(63, 71, 78, byte.MaxValue),
		new Color32(215, 225, 241, byte.MaxValue),
		new Color32(107, 47, 188, byte.MaxValue),
		new Color32(113, 73, 30, byte.MaxValue),
		new Color32(56, byte.MaxValue, 221, byte.MaxValue),
		new Color32(80, 240, 57, byte.MaxValue),
		Palette.FromHex(6233390),
		Palette.FromHex(15515859),
		Palette.FromHex(15787944),
		Palette.FromHex(7701907),
		Palette.FromHex(9537655),
		Palette.FromHex(14115940)
	};

	public static readonly Color32[] ShadowColors = new Color32[]
	{
		new Color32(122, 8, 56, byte.MaxValue),
		new Color32(9, 21, 142, byte.MaxValue),
		new Color32(10, 77, 46, byte.MaxValue),
		new Color32(172, 43, 174, byte.MaxValue),
		new Color32(180, 62, 21, byte.MaxValue),
		new Color32(195, 136, 34, byte.MaxValue),
		new Color32(30, 31, 38, byte.MaxValue),
		new Color32(132, 149, 192, byte.MaxValue),
		new Color32(59, 23, 124, byte.MaxValue),
		new Color32(94, 38, 21, byte.MaxValue),
		new Color32(36, 169, 191, byte.MaxValue),
		new Color32(21, 168, 66, byte.MaxValue),
		Palette.FromHex(4263706),
		Palette.FromHex(14586547),
		Palette.FromHex(13810825),
		Palette.FromHex(4609636),
		Palette.FromHex(5325118),
		Palette.FromHex(11813730)
	};

	public static readonly Color32 VisorColor = new Color32(149, 202, 220, byte.MaxValue);

	private static Color32 FromHex(int v)
	{
		return new Color32((byte)(v >> 16), (byte)(v >> 8), (byte)v, byte.MaxValue);
	}
}
