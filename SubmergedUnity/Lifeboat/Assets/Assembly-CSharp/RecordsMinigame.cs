using System;
using System.Collections;
using UnityEngine;

public class RecordsMinigame : Minigame
{
	public GameObject FoldersContent;

	public SpriteRenderer[] Folders;

	public GameObject DrawerContent;

	public Transform Drawer;

	public SpriteRenderer DrawerFolder;

	public GameObject ShelfContent;

	public SpriteRenderer[] Books;

	public Sprite[] BookCovers;

	private SpriteRenderer targetBook;

	public AudioClip recordFilePlace;

	public AudioClip recordBookPlace;

	public AudioClip grabDocument;

	public AudioClip drawerOpen;

	public AudioClip drawerClose;

	public Transform bookInputPrompt;

	public ControllerButtonBehaviourComplex slideFolderHotkey;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		this.FoldersContent.SetActive(false);
		this.ShelfContent.SetActive(false);
		this.DrawerContent.SetActive(false);
		base.SetupInput(true);
		if (base.ConsoleId == 0)
		{
			this.FoldersContent.SetActive(true);
			for (int i = 0; i < this.Folders.Length; i++)
			{
				if (this.MyNormTask.Data[i] != 0)
				{
					this.Folders[i].gameObject.SetActive(false);
				}
			}
			return;
		}
		if (base.ConsoleId <= 4)
		{
			this.DrawerContent.SetActive(true);
			return;
		}
		if (base.ConsoleId <= 8)
		{
			this.ShelfContent.SetActive(true);
			Sprite sprite = this.BookCovers.Random<Sprite>();
			SpriteRenderer[] books = this.Books;
			for (int j = 0; j < books.Length; j++)
			{
				books[j].sprite = sprite;
			}
			this.targetBook = this.Books.Random<SpriteRenderer>();
			this.targetBook.enabled = false;
			this.targetBook.GetComponent<PassiveButton>().enabled = true;
			ControllerButtonBehavior component = this.targetBook.GetComponent<ControllerButtonBehavior>();
			component.enabled = true;
			Vector3 localPosition = this.bookInputPrompt.transform.localPosition;
			localPosition.x = this.targetBook.transform.localPosition.x;
			this.bookInputPrompt.transform.localPosition = localPosition;
			this.bookInputPrompt.GetComponentInChildren<ActionMapGlyphDisplay>(true).actionToDisplayMappedGlyphFor = component.Action;
			this.bookInputPrompt.gameObject.SetActive(true);
		}
	}

	protected override IEnumerator CoAnimateOpen()
	{
		if (base.ConsoleId == 0)
		{
			this.TransType = TransitionType.SlideBottom;
		}
		else if (base.ConsoleId <= 4)
		{
			yield return this.CoOpenDrawer();
		}
		else if (base.ConsoleId <= 8)
		{
			this.TransType = TransitionType.SlideBottom;
		}
		yield return base.CoAnimateOpen();
		yield break;
	}

	protected override IEnumerator CoDestroySelf()
	{
		if (base.ConsoleId > 0 && base.ConsoleId <= 4)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(this.drawerClose, false, 1f);
			}
			yield return Effects.Slide2D(this.Drawer, Vector2.zero, new Vector2(0f, 7f), 0.3f);
		}
		yield return base.CoDestroySelf();
		yield break;
	}

	public void PlaceBook()
	{
		if (!this.MarkConsoleFinished())
		{
			return;
		}
		this.MyNormTask.NextStep();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.recordBookPlace, false, 1f);
		}
		base.StartCoroutine(this.CoSlideBook());
	}

	private IEnumerator CoSlideBook()
	{
		this.targetBook.GetComponent<PassiveButton>().enabled = false;
		this.targetBook.enabled = true;
		yield return Effects.ColorFade(this.targetBook, Palette.ClearWhite, Color.white, 0.4f);
		yield return base.CoStartClose(0.35f);
		yield break;
	}

	public void FileDocument()
	{
		if (!this.MarkConsoleFinished())
		{
			return;
		}
		this.MyNormTask.NextStep();
		this.slideFolderHotkey.enabled = false;
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.recordFilePlace, false, 1f);
		}
		base.StartCoroutine(this.CoSlideFolder());
	}

	private bool MarkConsoleFinished()
	{
		bool result = false;
		for (int i = 0; i < this.MyNormTask.Data.Length; i++)
		{
			byte b = this.MyNormTask.Data[i];
			if (b != 0 && b != 255)
			{
				this.MyNormTask.Data[i] = byte.MaxValue;
				result = true;
			}
		}
		return result;
	}

	private IEnumerator CoOpenDrawer()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.drawerOpen, false, 1f);
		}
		yield return Effects.Slide2D(this.Drawer, new Vector2(0f, 7f), Vector2.zero, 0.5f);
		yield break;
	}

	private IEnumerator CoSlideFolder()
	{
		this.DrawerFolder.gameObject.SetActive(true);
		yield return Effects.Slide2D(this.DrawerFolder.transform, new Vector2(0f, 5f), Vector2.zero, 0.4f);
		yield return base.CoStartClose(0.35f);
		yield break;
	}

	public void GrabFolder(SpriteRenderer folder)
	{
		if (this.amClosing != Minigame.CloseState.None)
		{
			return;
		}
		int num = this.Folders.IndexOf(folder);
		folder.gameObject.SetActive(false);
		this.MyNormTask.Data[num] = IntRange.NextByte(1, 9);
		this.MyNormTask.UpdateArrow();
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(this.grabDocument, false, 1f);
		}
		base.StartCoroutine(base.CoStartClose(0.75f));
	}
}
