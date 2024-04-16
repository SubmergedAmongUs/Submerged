using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Weather1Game : Minigame
{
	private static int[] BarrierValidX = new int[]
	{
		-10,
		-8,
		-6,
		-4,
		-2,
		0,
		2,
		4,
		6,
		8
	};

	private static int[] BarrierValidY = new int[]
	{
		-2,
		0,
		2
	};

	private const int MinX = -10;

	private const int MaxX = 8;

	private const int MinY = -3;

	private const int MaxY = 3;

	public Tilemap BarrierMap;

	public Tile fillTile;

	public Tile controlTile;

	public Tile barrierTile;

	public SpriteRenderer pulseCircle1;

	public SpriteRenderer pulseCircle2;

	public AudioClip NodeMove;

	private Controller control = new Controller();

	private bool inControl;

	private Vector3Int controlTilePos = new Vector3Int(-9, 3, 0);

	private static Vector3Int[] Directions = new Vector3Int[]
	{
		Vector3Int.up,
		Vector3Int.down,
		Vector3Int.left,
		Vector3Int.right
	};

	private float moveCooldown;

	public void Start()
	{
		Vector3Int vector3Int = new Vector3Int(-9, 3, 0);
		HashSet<Vector3Int> hashSet = new HashSet<Vector3Int>();
		hashSet.Add(vector3Int);
		this.SolveMaze(vector3Int, hashSet);
		Matrix4x4 matrix4x = Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, 90f));
		new List<Vector3Int>();
		Vector3Int vector3Int2 = default(Vector3Int);
		vector3Int2.x = -10;
		while (vector3Int2.x <= 8)
		{
			bool flag = vector3Int2.x % 2 == 0;
			vector3Int2.y = -3;
			int num;
			while (vector3Int2.y <= 3)
			{
				bool flag2 = vector3Int2.y % 2 == 0;
				if (this.PointIsValid(vector3Int2) && !hashSet.Contains(vector3Int2) && flag == flag2 && BoolRange.Next(0.75f))
				{
					this.BarrierMap.SetTile(vector3Int2, this.barrierTile);
					if (flag)
					{
						this.BarrierMap.SetTransformMatrix(vector3Int2, matrix4x);
					}
				}
				num = vector3Int2.y + 1;
				vector3Int2.y = num;
			}
			num = vector3Int2.x + 1;
			vector3Int2.x = num;
		}
		base.SetupInput(true);
	}

	private bool SolveMaze(Vector3Int curPos, HashSet<Vector3Int> solution)
	{
		if (solution.Count > 50)
		{
			return false;
		}
		bool[] array = new bool[4];
		while (this.Contains<bool>(array, false))
		{
			int num = array.RandomIdx<bool>();
			while (array[num])
			{
				num = (num + 1) % array.Length;
			}
			array[num] = true;
			Vector3Int vector3Int = Weather1Game.Directions[num] + curPos;
			if (this.PointIsValid(vector3Int) && solution.Add(vector3Int))
			{
				if (vector3Int.x == 7 && vector3Int.y == -3)
				{
					return true;
				}
				if (this.SolveMaze(vector3Int, solution))
				{
					return true;
				}
				solution.Remove(vector3Int);
			}
		}
		return false;
	}

	public void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.control.Update();
		if (this.BarrierMap.GetTile(new Vector3Int(7, -3, 0)) == this.controlTile)
		{
			this.pulseCircle1.enabled = false;
			this.pulseCircle2.enabled = false;
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
			return;
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			Player player = ReInput.players.GetPlayer(0);
			if (player.GetButton(11))
			{
				if (!this.inControl)
				{
					this.inControl = true;
				}
				if (this.moveCooldown > 0f)
				{
					this.moveCooldown -= Time.deltaTime;
				}
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				Vector2 vector = new Vector2(Mathf.Abs(axis2DRaw.x), Mathf.Abs(axis2DRaw.y));
				float num = Mathf.Max(vector.x, vector.y);
				float num2 = Mathf.Min(vector.x, vector.y);
				Vector3Int zero = Vector3Int.zero;
				bool flag = false;
				if (num > 0.7f && num2 < 0.2f)
				{
					if (vector.x > vector.y)
					{
						zero = new Vector3Int((int)Mathf.Sign(axis2DRaw.x), 0, 0);
						flag = true;
					}
					else
					{
						zero = new Vector3Int(0, (int)Mathf.Sign(axis2DRaw.y), 0);
						flag = true;
					}
				}
				else
				{
					this.moveCooldown = 0f;
				}
				if (flag && this.moveCooldown <= 0f)
				{
					this.moveCooldown = 0.15f;
					Vector3Int vector3Int = this.controlTilePos + zero;
					if (!this.BarrierMap.GetTile(vector3Int) && this.PointIsValid(vector3Int) && !this.AnythingBetween(this.controlTilePos, vector3Int))
					{
						this.FillLine(this.controlTilePos, vector3Int);
						this.controlTilePos = vector3Int;
						this.BarrierMap.SetTile(this.controlTilePos, this.controlTile);
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySoundImmediate(this.NodeMove, false, 1f, 1f);
						}
					}
				}
			}
			else
			{
				this.moveCooldown = 0f;
				if (this.inControl)
				{
					this.inControl = false;
					Vector3Int vector3Int2 = default(Vector3Int);
					vector3Int2.x = -10;
					while (vector3Int2.x <= 8)
					{
						vector3Int2.y = -3;
						int num3;
						while (vector3Int2.y <= 3)
						{
							if (this.BarrierMap.GetTile(vector3Int2) == this.fillTile)
							{
								this.BarrierMap.SetTile(vector3Int2, null);
							}
							num3 = vector3Int2.y + 1;
							vector3Int2.y = num3;
						}
						num3 = vector3Int2.x + 1;
						vector3Int2.x = num3;
					}
					this.BarrierMap.SetTile(this.controlTilePos, null);
					this.controlTilePos.x = -9;
					this.controlTilePos.y = 3;
					this.BarrierMap.SetTile(this.controlTilePos, this.controlTile);
				}
			}
		}
		else if (this.control.AnyTouch)
		{
			for (int i = 0; i < this.control.Touches.Length; i++)
			{
				Controller.TouchState touch = this.control.GetTouch(i);
				Vector3Int vector3Int3 = this.BarrierMap.WorldToCell(touch.Position);
				TileBase tile = this.BarrierMap.GetTile(vector3Int3);
				if (touch.TouchStart)
				{
					if (tile == this.controlTile)
					{
						this.inControl = true;
						this.controlTilePos = vector3Int3;
					}
				}
				else if (this.inControl && !tile && this.PointIsValid(vector3Int3) && !this.AnythingBetween(this.controlTilePos, vector3Int3))
				{
					this.FillLine(this.controlTilePos, vector3Int3);
					this.controlTilePos = vector3Int3;
					this.BarrierMap.SetTile(this.controlTilePos, this.controlTile);
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySoundImmediate(this.NodeMove, false, 1f, 1f);
					}
				}
			}
		}
		else if (this.control.AnyTouchUp)
		{
			for (int j = 0; j < this.control.Touches.Length; j++)
			{
				this.control.GetTouch(j);
				if (this.inControl)
				{
					Vector3Int vector3Int4 = default(Vector3Int);
					vector3Int4.x = -10;
					while (vector3Int4.x <= 8)
					{
						vector3Int4.y = -3;
						int num3;
						while (vector3Int4.y <= 3)
						{
							if (this.BarrierMap.GetTile(vector3Int4) == this.fillTile)
							{
								this.BarrierMap.SetTile(vector3Int4, null);
							}
							num3 = vector3Int4.y + 1;
							vector3Int4.y = num3;
						}
						num3 = vector3Int4.x + 1;
						vector3Int4.x = num3;
					}
					this.BarrierMap.SetTile(this.controlTilePos, null);
					this.controlTilePos.x = -9;
					this.controlTilePos.y = 3;
					this.BarrierMap.SetTile(this.controlTilePos, this.controlTile);
				}
			}
		}
		else
		{
			this.inControl = false;
		}
		this.pulseCircle1.transform.position = this.BarrierMap.CellToWorld(this.controlTilePos) + new Vector3(0.16f, 0.16f, 0f);
	}

	private void FillLine(Vector3Int controlTilePos, Vector3Int touchCellPos)
	{
		Vector3Int vector3Int = controlTilePos;
		this.BarrierMap.SetTile(vector3Int, this.fillTile);
		if (controlTilePos.x == touchCellPos.x)
		{
			int num = (int)Mathf.Sign((float)(touchCellPos.y - controlTilePos.y));
			while ((vector3Int.y += num) != touchCellPos.y)
			{
				this.BarrierMap.SetTile(vector3Int, this.fillTile);
			}
			return;
		}
		if (controlTilePos.y == touchCellPos.y)
		{
			int num2 = (int)Mathf.Sign((float)(touchCellPos.x - controlTilePos.x));
			while ((vector3Int.x += num2) != touchCellPos.x)
			{
				this.BarrierMap.SetTile(vector3Int, this.fillTile);
			}
		}
	}

	private bool AnythingBetween(Vector3Int controlTilePos, Vector3Int touchCellPos)
	{
		Vector3Int vector3Int = controlTilePos;
		if (controlTilePos.x == touchCellPos.x)
		{
			int num = (int)Mathf.Sign((float)(touchCellPos.y - controlTilePos.y));
			while ((vector3Int.y += num) != touchCellPos.y)
			{
				if (this.BarrierMap.GetTile(vector3Int) || !this.PointIsValid(vector3Int))
				{
					return true;
				}
			}
			return false;
		}
		if (controlTilePos.y == touchCellPos.y)
		{
			int num2 = (int)Mathf.Sign((float)(touchCellPos.x - controlTilePos.x));
			while ((vector3Int.x += num2) != touchCellPos.x)
			{
				if (this.BarrierMap.GetTile(vector3Int) || !this.PointIsValid(vector3Int))
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	private bool PointIsValid(Vector3Int touchCellPos)
	{
		bool flag = touchCellPos.x % 2 == 0;
		bool flag2 = touchCellPos.y % 2 == 0;
		return touchCellPos.x <= 8 && touchCellPos.x >= -10 && touchCellPos.y <= 3 && touchCellPos.y >= -3 && (!flag || flag2);
	}

	private bool Contains<T>(T[] self, T item) where T : IComparable
	{
		for (int i = 0; i < self.Length; i++)
		{
			if (self[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}
}
