using System;

public class WinningPlayerData
{
	public string Name;

	public bool IsDead;

	public bool IsImpostor;

	public int ColorId;

	public uint SkinId;

	public uint HatId;

	public uint PetId;

	public bool IsYou;

	public WinningPlayerData()
	{
	}

	public WinningPlayerData(GameData.PlayerInfo player)
	{
		this.IsYou = (player.Object == PlayerControl.LocalPlayer);
		this.Name = player.PlayerName;
		this.IsDead = (player.IsDead || player.Disconnected);
		this.IsImpostor = player.IsImpostor;
		this.ColorId = player.ColorId;
		this.SkinId = player.SkinId;
		this.PetId = player.PetId;
		this.HatId = player.HatId;
	}
}
