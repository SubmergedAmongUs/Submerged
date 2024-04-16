using System;
using System.Collections;
using System.IO;
using System.Linq;
using Rewired;
using UnityEngine;

public class PhotosMinigame : Minigame
{
	public GamePhotoBehaviour[] photos;

	public Sprite[] PhotoContents;

	public Collider2D PoolHitbox;

	public Transform selectorObject;

	public SpriteRenderer selectorHand;

	private Controller controller = new Controller();

	private bool prevHadButton;

	private int currentlyGrabbedObject = -1;

	private bool AllowDraggingPhotos
	{
		get
		{
			return this.MyNormTask.TimerStarted != NormalPlayerTask.TimerState.Started;
		}
	}

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		byte[] data = this.MyNormTask.Data;
		if (data == null || data.Length != 0)
		{
			this.ReadInitialData();
		}
		else
		{
			GamePhotoBehaviour[] array = this.photos;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Image.sprite = this.PhotoContents.Random<Sprite>();
			}
			this.WriteInitialData();
		}
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(this.selectorHand);
		base.SetupInput(false);
	}

	private void WriteInitialData()
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				for (int i = 0; i < this.photos.Length; i++)
				{
					GamePhotoBehaviour p = this.photos[i];
					Vector3 localPosition = p.transform.localPosition;
					binaryWriter.Write(this.PhotoContents.IndexOf((Sprite s) => s == p.Image.sprite));
					binaryWriter.Write(localPosition.x);
					binaryWriter.Write(localPosition.y);
				}
			}
			this.MyNormTask.Data = memoryStream.ToArray();
		}
	}

	private void ReadInitialData()
	{
		using (MemoryStream memoryStream = new MemoryStream(this.MyNormTask.Data))
		{
			using (BinaryReader binaryReader = new BinaryReader(memoryStream))
			{
				for (int i = 0; i < this.photos.Length; i++)
				{
					GamePhotoBehaviour gamePhotoBehaviour = this.photos[i];
					gamePhotoBehaviour.Image.sprite = this.PhotoContents[binaryReader.ReadInt32()];
					gamePhotoBehaviour.transform.localPosition = new Vector3(binaryReader.ReadSingle(), binaryReader.ReadSingle(), 1f);
				}
			}
		}
	}

	public IEnumerator Start()
	{
		float z = 0f;
		yield return null;
		foreach (GamePhotoBehaviour gamePhotoBehaviour in this.photos)
		{
			gamePhotoBehaviour.zOffset = z;
			gamePhotoBehaviour.transform.SetLocalZ(1f + z);
			z -= 0.01f;
			bool flag = gamePhotoBehaviour.Hitbox.IsTouching(this.PoolHitbox);
			gamePhotoBehaviour.TargetColor = (flag ? GamePhotoBehaviour.InWaterPink : Color.white);
			gamePhotoBehaviour.Frame.color = gamePhotoBehaviour.TargetColor;
			gamePhotoBehaviour.Image.color = Color.Lerp(gamePhotoBehaviour.TargetColor, Palette.ClearWhite, this.MyNormTask.TaskTimer / 60f);
		}
		yield break;
	}

	private void Update()
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		this.controller.Update();
		foreach (GamePhotoBehaviour gamePhotoBehaviour in this.photos)
		{
			gamePhotoBehaviour.Frame.color = gamePhotoBehaviour.TargetColor;
			gamePhotoBehaviour.Image.color = Color.Lerp(gamePhotoBehaviour.TargetColor, Palette.ClearWhite, this.MyNormTask.TaskTimer / 60f);
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			this.HandleJoystick();
		}
		else
		{
			this.HandleMouse();
		}
		if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.NotStarted && !this.controller.AnyTouch && this.photos.All((GamePhotoBehaviour p) => p.Hitbox.IsTouching(this.PoolHitbox)))
		{
			this.MyNormTask.TimerStarted = NormalPlayerTask.TimerState.Started;
			base.StartCoroutine(base.CoStartClose(0.75f));
			return;
		}
		if (this.MyNormTask.TimerStarted == NormalPlayerTask.TimerState.Finished && !this.controller.AnyTouch && !this.amOpening && this.photos.All((GamePhotoBehaviour p) => !p.Hitbox.IsTouching(this.PoolHitbox)))
		{
			this.MyNormTask.NextStep();
			this.Close();
		}
	}

	private void HandleJoystick()
	{
		bool button = ReInput.players.GetPlayer(0).GetButton(11);
		Vector3 position = this.selectorObject.transform.position;
		position.x = VirtualCursor.currentPosition.x;
		position.y = VirtualCursor.currentPosition.y;
		this.selectorObject.transform.position = position;
		if (button && this.AllowDraggingPhotos)
		{
			if (!this.prevHadButton)
			{
				float num = 0f;
				this.currentlyGrabbedObject = -1;
				for (int i = 0; i < this.photos.Length; i++)
				{
					float sqrMagnitude = (this.photos[i].transform.localPosition - this.selectorObject.transform.localPosition).sqrMagnitude;
					if (sqrMagnitude <= 1f && (this.currentlyGrabbedObject == -1 || sqrMagnitude < num))
					{
						num = sqrMagnitude;
						this.currentlyGrabbedObject = i;
					}
				}
				if (this.currentlyGrabbedObject != -1)
				{
					GamePhotoBehaviour gamePhotoBehaviour = this.photos[this.currentlyGrabbedObject];
					gamePhotoBehaviour.StopAllCoroutines();
					gamePhotoBehaviour.StartCoroutine(gamePhotoBehaviour.Pickup());
					this.FixZ(gamePhotoBehaviour);
				}
			}
			else if (this.currentlyGrabbedObject != -1)
			{
				GamePhotoBehaviour gamePhotoBehaviour2 = this.photos[this.currentlyGrabbedObject];
				Vector3 position2 = gamePhotoBehaviour2.transform.position;
				position2.x = position.x;
				position2.y = position.y;
				gamePhotoBehaviour2.transform.position = position2;
			}
			this.prevHadButton = true;
		}
		else if (this.AllowDraggingPhotos)
		{
			if (this.prevHadButton && this.currentlyGrabbedObject != -1)
			{
				GamePhotoBehaviour gamePhotoBehaviour3 = this.photos[this.currentlyGrabbedObject];
				bool inWater = gamePhotoBehaviour3.Hitbox.IsTouching(this.PoolHitbox);
				gamePhotoBehaviour3.StopAllCoroutines();
				gamePhotoBehaviour3.StartCoroutine(gamePhotoBehaviour3.Drop(inWater));
				this.WriteInitialData();
			}
			this.prevHadButton = false;
			this.currentlyGrabbedObject = -1;
		}
		if (this.currentlyGrabbedObject != -1 && this.selectorObject.gameObject.activeSelf)
		{
			this.selectorObject.gameObject.SetActive(false);
			return;
		}
		if (this.currentlyGrabbedObject == -1 && !this.selectorObject.gameObject.activeSelf)
		{
			this.selectorObject.gameObject.SetActive(true);
		}
	}

	private void HandleMouse()
	{
		if (this.selectorObject.gameObject.activeSelf)
		{
			this.selectorObject.gameObject.SetActive(false);
		}
		if (this.currentlyGrabbedObject != -1)
		{
			GamePhotoBehaviour gamePhotoBehaviour = this.photos[this.currentlyGrabbedObject];
			bool inWater = gamePhotoBehaviour.Hitbox.IsTouching(this.PoolHitbox);
			gamePhotoBehaviour.StopAllCoroutines();
			gamePhotoBehaviour.StartCoroutine(gamePhotoBehaviour.Drop(inWater));
			this.WriteInitialData();
			this.currentlyGrabbedObject = -1;
			this.prevHadButton = false;
		}
		if (this.AllowDraggingPhotos)
		{
			for (int i = 0; i < this.photos.Length; i++)
			{
				GamePhotoBehaviour gamePhotoBehaviour2 = this.photos[i];
				switch (this.controller.CheckDrag(gamePhotoBehaviour2.Hitbox))
				{
				case DragState.TouchStart:
					gamePhotoBehaviour2.StopAllCoroutines();
					gamePhotoBehaviour2.StartCoroutine(gamePhotoBehaviour2.Pickup());
					this.FixZ(gamePhotoBehaviour2);
					break;
				case DragState.Dragging:
				{
					Vector3 position = this.controller.DragPosition;
					position.z = gamePhotoBehaviour2.transform.position.z;
					gamePhotoBehaviour2.transform.position = position;
					break;
				}
				case DragState.Released:
				{
					bool inWater2 = gamePhotoBehaviour2.Hitbox.IsTouching(this.PoolHitbox);
					gamePhotoBehaviour2.StopAllCoroutines();
					gamePhotoBehaviour2.StartCoroutine(gamePhotoBehaviour2.Drop(inWater2));
					this.WriteInitialData();
					break;
				}
				}
			}
		}
	}

	private void FixZ(GamePhotoBehaviour current)
	{
		if (current.zOffset != (float)this.photos.Length / -100f)
		{
			current.zOffset = (float)this.photos.Length / -100f;
			foreach (GamePhotoBehaviour gamePhotoBehaviour in this.photos)
			{
				if (!(gamePhotoBehaviour == current))
				{
					gamePhotoBehaviour.zOffset += 0.01f;
					gamePhotoBehaviour.transform.SetLocalZ(1f + gamePhotoBehaviour.zOffset);
				}
			}
		}
	}
}
