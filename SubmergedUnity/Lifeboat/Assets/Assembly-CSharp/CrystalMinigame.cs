using System;
using System.Linq;
using Rewired;
using UnityEngine;

public class CrystalMinigame : Minigame
{
	public CrystalBehaviour[] CrystalPieces;

	private CrystalBehaviour[] Shuffed;

	public Transform[] CrystalSlots;

	public FloatRange XRange;

	public float TrayY = -2.28f;

	public AudioClip[] PickUpSounds;

	public AudioClip AttachSound;

	private Controller myController = new Controller();

	private bool prevHadStick;

	private bool prevHadButton;

	private int prevSelectedCrystal = -1;

	private int selectedCrystal;

	public void Start()
	{
		this.Shuffed = this.CrystalPieces.ToArray<CrystalBehaviour>();
		this.Shuffed.Shuffle(0);
		for (int i = 0; i < this.Shuffed.Length; i++)
		{
			this.Shuffed[i].transform.localPosition = new Vector3(this.XRange.Lerp(((float)i + 0.5f) / (float)this.Shuffed.Length), this.TrayY, ((float)i - (float)this.Shuffed.Length / 2f) / 100f);
		}
		base.SetupInput(true);
	}

	public void Update()
	{
		this.myController.Update();
		if (this.selectedCrystal != this.prevSelectedCrystal)
		{
			if (this.prevSelectedCrystal != -1)
			{
				this.CrystalPieces[this.prevSelectedCrystal].ReceiveMouseOut();
			}
			this.prevSelectedCrystal = this.selectedCrystal;
			this.CrystalPieces[this.selectedCrystal].ReceiveMouseOver();
		}
		if (Controller.currentTouchType == Controller.TouchType.Joystick)
		{
			if (!this.amOpening && this.amClosing == Minigame.CloseState.None)
			{
				Player player = ReInput.players.GetPlayer(0);
				Vector2 axis2DRaw = player.GetAxis2DRaw(13, 14);
				bool button = player.GetButton(11);
				CrystalBehaviour crystalBehaviour = this.CrystalPieces[this.selectedCrystal];
				if (button)
				{
					if (!this.prevHadButton)
					{
						this.inputHandler.disableVirtualCursor = false;
						VirtualCursor.instance.SetWorldPosition(this.CrystalPieces[this.selectedCrystal].transform.position);
						if (Constants.ShouldPlaySfx())
						{
							SoundManager.Instance.PlaySound(this.PickUpSounds.Random<AudioClip>(), false, 1f);
						}
						crystalBehaviour.StopAllCoroutines();
					}
					else
					{
						Vector3 position = this.CrystalPieces[this.selectedCrystal].transform.position;
						position.x = VirtualCursor.currentPosition.x;
						position.y = VirtualCursor.currentPosition.y;
						crystalBehaviour.transform.position = position;
					}
					this.prevHadButton = true;
					return;
				}
				if (this.prevHadButton)
				{
					this.inputHandler.disableVirtualCursor = true;
					this.CheckSolution(this.selectedCrystal);
					if (crystalBehaviour.CanMove)
					{
						int num = this.Shuffed.IndexOf(crystalBehaviour);
						crystalBehaviour.StartCoroutine(Effects.Slide2D(crystalBehaviour.transform, crystalBehaviour.transform.localPosition, new Vector2(this.XRange.Lerp(((float)num + 0.5f) / (float)this.CrystalPieces.Length), this.TrayY), 0.15f));
					}
				}
				this.prevHadButton = false;
				if (axis2DRaw.sqrMagnitude > 0.7f)
				{
					if (!this.prevHadStick)
					{
						Vector2 normalized = axis2DRaw.normalized;
						int num2 = -1;
						float num3 = 0f;
						float num4 = 0.4f;
						for (int i = 0; i < this.CrystalPieces.Length; i++)
						{
							Vector2 vector = this.CrystalPieces[i].transform.position - this.CrystalPieces[this.selectedCrystal].transform.position;
							float magnitude = vector.magnitude;
							vector /= magnitude;
							float num5 = Vector2.Dot(vector, normalized) / Mathf.Lerp(magnitude, 1f, 0.95f);
							if (i != this.selectedCrystal && num5 > num3 && num5 > 0f && num5 > num4)
							{
								num3 = num5;
								num2 = i;
							}
						}
						if (num2 != -1)
						{
							this.selectedCrystal = num2;
						}
					}
					this.prevHadStick = true;
					return;
				}
				this.prevHadStick = false;
				return;
			}
		}
		else
		{
			this.prevHadButton = false;
			this.prevHadStick = false;
			if (this.prevSelectedCrystal != -1)
			{
				this.CrystalPieces[this.prevSelectedCrystal].Renderer.color = Color.white;
				this.prevSelectedCrystal = -1;
			}
			for (int j = 0; j < this.CrystalPieces.Length; j++)
			{
				CrystalBehaviour crystalBehaviour2 = this.CrystalPieces[j];
				switch (this.myController.CheckDrag(crystalBehaviour2.Collider))
				{
				case DragState.TouchStart:
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(this.PickUpSounds.Random<AudioClip>(), false, 1f);
					}
					crystalBehaviour2.StopAllCoroutines();
					break;
				case DragState.Dragging:
				{
					Vector3 position2 = this.myController.DragPosition;
					position2.z = base.transform.position.z;
					crystalBehaviour2.transform.position = position2;
					break;
				}
				case DragState.Released:
					this.CheckSolution(j);
					if (crystalBehaviour2.CanMove)
					{
						int num6 = this.Shuffed.IndexOf(crystalBehaviour2);
						crystalBehaviour2.StartCoroutine(Effects.Slide2D(crystalBehaviour2.transform, crystalBehaviour2.transform.localPosition, new Vector2(this.XRange.Lerp(((float)num6 + 0.5f) / (float)this.CrystalPieces.Length), this.TrayY), 0.15f));
					}
					break;
				}
			}
		}
	}

	private void CheckSolution(int startAt)
	{
		CrystalBehaviour crystalBehaviour = this.CrystalPieces[startAt];
		if (!crystalBehaviour.CanMove)
		{
			return;
		}
		Transform transform = this.CrystalSlots[startAt];
		if (crystalBehaviour.Collider.OverlapPoint(transform.position))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.AttachSound, false, 1f);
			}
			crystalBehaviour.CanMove = false;
			crystalBehaviour.TargetPosition = transform;
			for (int i = startAt; i < this.CrystalPieces.Length; i++)
			{
				this.CrystalPieces[i].Flash((float)(i - startAt) * 0.1f);
			}
			for (int j = startAt - 1; j >= 0; j--)
			{
				this.CrystalPieces[j].Flash((float)(startAt - j) * 0.1f);
			}
			int num = -1;
			float num2 = 0f;
			for (int k = 0; k < this.CrystalPieces.Length; k++)
			{
				if (this.CrystalPieces[k].CanMove && (num == -1 || this.CrystalPieces[k].transform.localPosition.x < num2))
				{
					num = k;
					num2 = this.CrystalPieces[k].transform.localPosition.x;
				}
			}
			this.CrystalPieces[this.selectedCrystal].Renderer.color = Color.white;
			if (num != -1)
			{
				this.selectedCrystal = num;
			}
		}
		if (this.CrystalPieces.All((CrystalBehaviour c) => !c.CanMove))
		{
			this.MyNormTask.NextStep();
			base.StartCoroutine(base.CoStartClose(0.75f));
		}
	}
}
