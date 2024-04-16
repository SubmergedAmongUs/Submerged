using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = System.Random;

public class RandomNameGenerator : MonoBehaviour
{
	public TextAsset PatternConfig;

	private List<Pattern> Patterns = new List<Pattern>();

	private Dictionary<string, WordGroup> WordGroups = new Dictionary<string, WordGroup>(StringComparer.OrdinalIgnoreCase);

	private static Random randy = new Random();

	private IEnumerable<Pattern> UsablePatterns;

	private void Awake()
	{
		this.Parse(new StringReader(this.PatternConfig.text));
		this.UsablePatterns = this.Patterns;
	}

	public string GetName()
	{
		if (this.UsablePatterns.Count<Pattern>() == 0)
		{
			this.Reset();
		}
		Pattern pattern = this.UsablePatterns.Random<Pattern>();
		this.UsablePatterns = from p in this.UsablePatterns
		where p != pattern
		select p;
		return pattern.FollowPattern(RandomNameGenerator.randy, this.WordGroups);
	}

	public bool ValidateName(string inName)
	{
		this.UsablePatterns = this.Patterns;
		using (IEnumerator<Pattern> enumerator = this.UsablePatterns.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.CheckPattern(inName, this.WordGroups))
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Reset()
	{
		this.UsablePatterns = this.Patterns;
	}

	public void Parse(TextReader reader)
	{
		string text;
		while ((text = reader.ReadLine()) != null)
		{
			if (text.StartsWith("["))
			{
				string[] array = text.Split(new char[]
				{
					' ',
					'[',
					']'
				}, StringSplitOptions.RemoveEmptyEntries);
				if (array.Length == 2)
				{
					if (array.Any((string w) => w.Equals("words", StringComparison.OrdinalIgnoreCase)))
					{
						string key = array[0];
						WordGroup value = WordGroup.Parse(reader);
						this.WordGroups.Add(key, value);
						continue;
					}
				}
				if (array.Length == 1 && array[0].Equals("patterns", StringComparison.OrdinalIgnoreCase))
				{
					while (!(text = reader.ReadLine()).IsNullOrWhiteSpace())
					{
						this.Patterns.Add(Pattern.Parse(text));
					}
				}
			}
		}
	}
}
