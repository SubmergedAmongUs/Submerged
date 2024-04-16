using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Pattern
{
	private string pattern;

	public string FollowPattern(Random randy, Dictionary<string, WordGroup> words)
	{
		bool flag = false;
		bool flag2 = true;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (char c in this.pattern)
		{
			if (c == '^')
			{
				flag = true;
			}
			else if (char.IsUpper(c))
			{
				WordGroup wordGroup;
				if (words.TryGetValue(c.ToString(), out wordGroup))
				{
					string text2 = wordGroup[randy.Next(wordGroup.Count)];
					if (flag2)
					{
						flag2 = flag;
						stringBuilder.AppendFormat("{0}{1}", text2.Substring(0, 1).ToUpperInvariant(), text2.Substring(1).ToLowerInvariant());
					}
					else
					{
						stringBuilder.Append(text2);
					}
				}
			}
			else
			{
				if (flag2)
				{
					flag2 = flag;
					stringBuilder.Append(char.ToUpperInvariant(c));
				}
				else
				{
					stringBuilder.Append(c);
				}
				if (char.IsWhiteSpace(c))
				{
					flag2 = true;
				}
			}
		}
		return stringBuilder.Replace(" Of", " of").Replace(" The", " the").ToString();
	}

	public static Pattern Parse(string line)
	{
		return new Pattern
		{
			pattern = line
		};
	}

	public bool CheckPattern(string nameToCheck, Dictionary<string, WordGroup> words)
	{
		bool flag = false;
		bool flag2 = true;
		new StringBuilder();
		nameToCheck = nameToCheck.ToLower();
		Func<string, bool> func0 = (string x) => nameToCheck.StartsWith(x.ToLower());
		Func<string, bool> func1 = (string x) => nameToCheck.Equals(x.ToLower());
		foreach (char c in this.pattern)
		{
			if (c == '^')
			{
				flag = true;
			}
			else if (char.IsUpper(c))
			{
				WordGroup wordGroup;
				if (words.TryGetValue(c.ToString(), out wordGroup))
				{
					if (flag2)
					{
						flag2 = flag;
						IEnumerable<string> source = wordGroup;
						Func<string, bool> predicate;
						if ((predicate = func0) == null)
						{
							predicate = (func0 = ((string x) => nameToCheck.StartsWith(x.ToLower())));
						}
						string text2 = source.FirstOrDefault(predicate);
						if (!string.IsNullOrEmpty(text2))
						{
							nameToCheck = nameToCheck.Replace(text2.ToLower(), "");
						}
					}
					else
					{
						IEnumerable<string> source2 = wordGroup;
						Func<string, bool> predicate2;
						if ((predicate2 = func1) == null)
						{
							predicate2 = (func1 = ((string x) => nameToCheck.Equals(x.ToLower())));
						}
						string text3 = source2.FirstOrDefault(predicate2);
						if (!string.IsNullOrEmpty(text3))
						{
							nameToCheck = nameToCheck.Replace(text3.ToLower(), "");
						}
					}
				}
			}
			else
			{
				if (flag2)
				{
					flag2 = flag;
					nameToCheck = nameToCheck.Replace(c.ToString().ToLower(), "");
				}
				else
				{
					nameToCheck = nameToCheck.Replace(c.ToString().ToLower(), "");
				}
				if (char.IsWhiteSpace(c))
				{
					flag2 = true;
				}
			}
		}
		nameToCheck = nameToCheck.Replace(" of", "").Replace(" the", "").Trim().ToString();
		return string.IsNullOrEmpty(nameToCheck);
	}
}
