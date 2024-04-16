using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxTMP : MonoBehaviour, IFocusHolder
{
	public static readonly HashSet<char> SymbolChars = new HashSet<char>
	{
		'?',
		'!',
		',',
		'.',
		'\'',
		':',
		';',
		'(',
		')',
		'/',
		'\\',
		'%',
		'^',
		'&',
		'-',
		'=',
		'¿',
		'？'
	};

	public static readonly HashSet<char> EmailChars = new HashSet<char>
	{
		'!',
		'~',
		'@',
		'.',
		'-',
		'_',
		'+'
	};

	public bool allowAllCharacters;

	public string text;

	private string compoText = "";

	public int characterLimit = -1;

	[SerializeField]
	public TextMeshPro outputText;

	public SpriteRenderer Background;

	public MeshRenderer Pipe;

	private float pipeBlinkTimer;

	public bool ClearOnFocus;

	public bool ForceUppercase;

	public Button.ButtonClickedEvent OnEnter;

	public Button.ButtonClickedEvent OnChange;

	public Button.ButtonClickedEvent OnFocusLost;

	private TouchScreenKeyboard keyboard;

	public bool AllowSymbols;

	public bool AllowEmail;

	public bool IpMode;

	public bool AllowPaste;

	private Collider2D[] colliders;

	private bool hasFocus;

	private StringBuilder tempTxt = new StringBuilder();

	public SpriteRenderer sendButtonGlyph;

	public SpriteRenderer quickChatGlyph;

	public float TextHeight
	{
		get
		{
			return this.outputText.GetNotDumbRenderedHeight();
		}
	}

	public void Start()
	{
		this.colliders = base.GetComponents<Collider2D>();
		DestroyableSingleton<PassiveButtonManager>.Instance.RegisterOne(this);
		if (this.Pipe)
		{
			this.Pipe.enabled = false;
		}
	}

	public void OnDestroy()
	{
		if (this.keyboard != null)
		{
			this.keyboard.active = false;
			this.keyboard = null;
		}
		if (DestroyableSingleton<PassiveButtonManager>.InstanceExists)
		{
			DestroyableSingleton<PassiveButtonManager>.Instance.RemoveOne(this);
		}
	}

	public void Clear()
	{
		this.SetText(string.Empty, string.Empty);
	}

	public void Update()
	{
		if (!base.enabled)
		{
			return;
		}
		if (!this.hasFocus)
		{
			return;
		}
		if (this.AllowPaste && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.V))
		{
			string clipboardString = ClipboardHelper.GetClipboardString();
			if (!string.IsNullOrWhiteSpace(clipboardString))
			{
				this.SetText(this.text + clipboardString, "");
			}
		}
		string inputString = Input.inputString;
		if (inputString.Length > 0 || this.compoText != Input.compositionString)
		{
			if (this.text == null || this.text == "Enter Name")
			{
				this.text = "";
			}
			this.SetText(this.text + inputString, Input.compositionString);
		}
		if (this.Pipe && this.hasFocus)
		{
			this.pipeBlinkTimer += Time.deltaTime * 2f;
			this.Pipe.enabled = ((int)this.pipeBlinkTimer % 2 == 0);
		}
	}

	public void GiveFocus()
	{
		if (!base.enabled)
		{
			return;
		}
		Input.imeCompositionMode = IMECompositionMode.On;
		if (this.hasFocus)
		{
			return;
		}
		if (this.ClearOnFocus)
		{
			this.text = string.Empty;
			this.compoText = string.Empty;
			this.outputText.text = string.Empty;
		}
		this.hasFocus = true;
		if (TouchScreenKeyboard.isSupported)
		{
			this.keyboard = TouchScreenKeyboard.Open(this.text);
		}
		if (this.Background)
		{
			this.Background.color = Color.green;
		}
		this.pipeBlinkTimer = 0f;
		if (this.Pipe)
		{
			this.Pipe.transform.localPosition = this.outputText.CursorPos();
		}
	}

	public void LoseFocus()
	{
		if (!this.hasFocus)
		{
			return;
		}
		Input.imeCompositionMode = IMECompositionMode.Off;
		if (this.compoText.Length > 0)
		{
			this.SetText(this.text + this.compoText, "");
			this.compoText = string.Empty;
		}
		this.hasFocus = false;
		if (this.keyboard != null)
		{
			this.keyboard.active = false;
			this.keyboard = null;
		}
		if (this.Background)
		{
			this.Background.color = Color.white;
		}
		if (this.Pipe)
		{
			this.Pipe.enabled = false;
		}
		this.OnFocusLost.Invoke();
		if (this.sendButtonGlyph)
		{
			this.sendButtonGlyph.enabled = true;
		}
		if (this.quickChatGlyph)
		{
			this.quickChatGlyph.enabled = false;
		}
	}

	public bool CheckCollision(Vector2 pt)
	{
		for (int i = 0; i < this.colliders.Length; i++)
		{
			if (this.colliders[i].OverlapPoint(pt))
			{
				return true;
			}
		}
		return false;
	}

	public void SetText(string input, string inputCompo = "")
	{
		bool flag = false;
		char c = ' ';
		this.tempTxt.Clear();
		for (int index = 0; index < input.Length; index++)
		{
			char c2 = input[index];
			if (c != ' ' || c2 != ' ')
			{
				if (c2 == '\r' || c2 == '\n')
				{
					flag = true;
				}

				if (c2 == '\b')
				{
					this.tempTxt.Length = Math.Max(this.tempTxt.Length - 1, 0);
				}

				if (this.ForceUppercase)
				{
					c2 = char.ToUpperInvariant(c2);
				}

				if (this.IsCharAllowed(c2))
				{
					this.tempTxt.Append(c2);
					c = c2;
				}
			}
		}

		if (!this.tempTxt.ToString().Equals(DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.EnterName, Array.Empty<object>()), StringComparison.OrdinalIgnoreCase) && this.characterLimit > 0)
		{
			this.tempTxt.Length = Math.Min(this.tempTxt.Length, this.characterLimit);
		}
		input = this.tempTxt.ToString();
		if (!input.Equals(this.text) || !inputCompo.Equals(this.compoText))
		{
			this.text = input;
			this.compoText = inputCompo;
			this.outputText.text = this.text + "<color=#FF0000>" + this.compoText + "</color>";
			this.outputText.ForceMeshUpdate(true, true);
			if (this.keyboard != null)
			{
				this.keyboard.text = this.text;
			}
			this.OnChange.Invoke();
		}
		if (flag)
		{
			this.OnEnter.Invoke();
		}
		if (this.Pipe)
		{
			this.Pipe.transform.localPosition = this.outputText.CursorPos();
		}
	}

	public bool IsCharAllowed(char i)
	{
		if (this.IpMode)
		{
			return (i >= '0' && i <= '9') || i == '.';
		}
		return i == ' ' || (i >= 'A' && i <= 'Z') || (i >= 'a' && i <= 'z') || (i >= '0' && i <= '9') || (i >= 'À' && i <= 'ÿ') || (i >= 'Ѐ' && i <= 'џ') || (i >= '぀' && i <= '㆟') || (i >= 'ⱡ' && i <= '힣') || (this.AllowSymbols && TextBoxTMP.SymbolChars.Contains(i)) || (this.AllowEmail && TextBoxTMP.EmailChars.Contains(i));
	}

	public void AddText(string text)
	{
		if (this.text.Length > 0 && this.text[this.text.Length - 1] == ' ')
		{
			this.SetText(this.text + text + " ", "");
			return;
		}
		this.SetText(this.text + " " + text + " ", "");
	}

	public void Backspace()
	{
		this.SetText(this.text + "\b", "");
	}

	public void ClearLastWord()
	{
		if (this.text.Length > 2)
		{
			int num = this.text.Length - 2;
			while (num > 0 && (this.text[num] != ' ' || this.text[num + 1] == ' '))
			{
				num--;
			}
			this.SetText(this.text.Substring(0, num), "");
			return;
		}
		this.SetText("", "");
	}
}
