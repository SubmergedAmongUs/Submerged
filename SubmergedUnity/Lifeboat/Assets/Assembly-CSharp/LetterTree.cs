using System;
using System.Collections.Generic;
using System.Text;

public class LetterTree
{
	private LetterTree.LetterNode root = new LetterTree.LetterNode('\0');

	public void Clear()
	{
		this.root = new LetterTree.LetterNode('\0');
	}

	public void AddWord(string word)
	{
		LetterTree.LetterNode letterNode = this.root;
		foreach (char l in word)
		{
			if (!this.IsFiller(l))
			{
				letterNode = letterNode.CreateChild(l);
			}
		}
		if (letterNode.Terminal == LetterTree.NodeTypes.NonTerm)
		{
			letterNode.Terminal = LetterTree.NodeTypes.Terminal;
			if (word[word.Length - 1] == '~')
			{
				letterNode.Terminal = LetterTree.NodeTypes.TerminalStrict;
			}
			if (word[word.Length - 1] == '^')
			{
				letterNode.Terminal = LetterTree.NodeTypes.TerminalExact;
			}
			if (word[word.Length - 1] == '`')
			{
				letterNode.Terminal = LetterTree.NodeTypes.TerminalUnbroken;
			}
		}
	}

	public bool IsFiller(char l)
	{
		return LetterTree.LetterNode.ToIndex(l) == (int)l;
	}

	public int Search(StringBuilder input, int start)
	{
		if (start >= input.Length || this.IsFiller(input[start]))
		{
			return 0;
		}
		bool exactStart = start == 0 || this.IsFiller(input[start - 1]);
		return this.SubSearchRec(input, start, this.root, false, false, exactStart);
	}

	public int Search(string inputStr, int start)
	{
		StringBuilder stringBuilder = new StringBuilder(inputStr);
		if (start >= stringBuilder.Length || this.IsFiller(stringBuilder[start]))
		{
			return 0;
		}
		bool exactStart = start == 0 || this.IsFiller(stringBuilder[start - 1]);
		return this.SubSearchRec(stringBuilder, start, this.root, false, false, exactStart);
	}

	private int SubSearchRec(StringBuilder input, int start, LetterTree.LetterNode previous, bool postDupes, bool postBreak, bool exactStart)
	{
		if (start >= input.Length)
		{
			return -2;
		}
		char c = input[start];
		if (this.IsFiller(c))
		{
			if (postDupes)
			{
				return -2;
			}
			int num = this.SubSearchRec(input, start + 1, previous, postDupes, true, exactStart);
			if (num > 0)
			{
				return num + 1;
			}
			return -2;
		}
		else
		{
			if (c == previous.Letter && !postBreak)
			{
				int num2 = this.SubSearchRec(input, start + 1, previous, true, postBreak, exactStart);
				if (num2 > 0)
				{
					return num2 + 1;
				}
				if (previous.Terminal != LetterTree.NodeTypes.NonTerm)
				{
					return 1;
				}
			}
			LetterTree.LetterNode letterNode = previous.FindChild(c);
			if (letterNode == null)
			{
				return -3;
			}
			int num3 = this.SubSearchRec(input, start + 1, letterNode, postDupes, postBreak, exactStart);
			if (num3 > 0)
			{
				return num3 + 1;
			}
			if (letterNode.Terminal == LetterTree.NodeTypes.TerminalStrict && num3 == -2 && (exactStart || !postBreak))
			{
				return 1;
			}
			if (letterNode.Terminal == LetterTree.NodeTypes.TerminalUnbroken && num3 == -2 && !postBreak)
			{
				return 1;
			}
			if (letterNode.Terminal == LetterTree.NodeTypes.TerminalExact && (num3 == -2 && exactStart))
			{
				return 1;
			}
			if (letterNode.Terminal == LetterTree.NodeTypes.Terminal && num3 <= 0)
			{
				return 1;
			}
			return num3;
		}
	}

	public IEnumerable<string> GetWords()
	{
		StringBuilder b = new StringBuilder();
		foreach (LetterTree.LetterNode node in this.root.Children)
		{
			foreach (string text in this.GetWords(b, 0, node))
			{
				yield return text;
			}
			IEnumerator<string> enumerator = null;
		}
		LetterTree.LetterNode[] array = null;
		yield break;
		yield break;
	}

	private IEnumerable<string> GetWords(StringBuilder b, int i, LetterTree.LetterNode node)
	{
		if (node == null)
		{
			yield break;
		}
		int length = b.Length;
		b.Length = length + 1;
		b[i] = node.Letter;
		if (node.Terminal == LetterTree.NodeTypes.Terminal)
		{
			yield return b.ToString();
		}
		else if (node.Terminal == LetterTree.NodeTypes.TerminalStrict)
		{
			length = b.Length;
			b.Length = length + 1;
			b[i + 1] = '~';
			yield return b.ToString();
			length = b.Length;
			b.Length = length - 1;
		}
		foreach (LetterTree.LetterNode node2 in node.Children)
		{
			foreach (string text in this.GetWords(b, i + 1, node2))
			{
				yield return text;
			}
			IEnumerator<string> enumerator = null;
		}
		LetterTree.LetterNode[] array = null;
		length = b.Length;
		b.Length = length - 1;
		yield break;
		yield break;
	}

	private enum NodeTypes : byte
	{
		NonTerm,
		Terminal,
		TerminalStrict,
		TerminalExact,
		TerminalUnbroken
	}

	private class LetterNode
	{
		public char Letter;

		public LetterTree.NodeTypes Terminal;

		public LetterTree.LetterNode[] Children = new LetterTree.LetterNode[26];

		public LetterNode(char l)
		{
			this.Letter = l;
		}

		public LetterTree.LetterNode CreateChild(char l)
		{
			int num = LetterTree.LetterNode.ToIndex(l);
			LetterTree.LetterNode letterNode = this.Children[num];
			if (letterNode == null)
			{
				letterNode = (this.Children[num] = new LetterTree.LetterNode(l));
			}
			return letterNode;
		}

		public LetterTree.LetterNode FindChild(char l)
		{
			int num = LetterTree.LetterNode.ToIndex(l);
			return this.Children[num];
		}

		public static int ToIndex(char c)
		{
			if (c >= 'A' && c <= 'Z')
			{
				return (int)(c - 'A');
			}
			if (c >= 'a' && c <= 'z')
			{
				return (int)(c - 'a');
			}
			if (c == 'с')
			{
				return 2;
			}
			if (c == 'к')
			{
				return 10;
			}
			if (c == '$')
			{
				return 18;
			}
			if (c == '+')
			{
				return 19;
			}
			if (c == '0')
			{
				return 14;
			}
			if (c == '1')
			{
				return 8;
			}
			if (c == '!')
			{
				return 8;
			}
			if (c == '2')
			{
				return 18;
			}
			if (c == '3')
			{
				return 4;
			}
			if (c == '4')
			{
				return 0;
			}
			if (c == '5')
			{
				return 18;
			}
			if (c == '7')
			{
				return 19;
			}
			if (c == '8')
			{
				return 1;
			}
			if (c > 'z')
			{
				foreach (char c2 in c.ToString().Normalize(NormalizationForm.FormD))
				{
					if (c2 <= 'z')
					{
						return LetterTree.LetterNode.ToIndex(c2);
					}
				}
			}
			return (int)c;
		}
	}
}
