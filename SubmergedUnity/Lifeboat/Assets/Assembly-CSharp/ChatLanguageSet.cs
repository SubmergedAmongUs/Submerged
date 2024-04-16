using System;
using System.Collections.Generic;
using System.IO;

public class ChatLanguageSet
{
	public static readonly ChatLanguageSet Instance = new ChatLanguageSet();

	public readonly Dictionary<string, uint> Languages = new Dictionary<string, uint>
	{
		{
			"English",
			256U
		},
		{
			"Español (Latinoamérica)",
			2U
		},
		{
			"Português (Brasil)",
			2048U
		},
		{
			"Português",
			16U
		},
		{
			"한국어",
			4U
		},
		{
			"Pусский",
			8U
		},
		{
			"Nederlands",
			4096U
		},
		{
			"Bisaya",
			64U
		},
		{
			"Français",
			8192U
		},
		{
			"Deutsch",
			16384U
		},
		{
			"Italiano",
			32768U
		},
		{
			"日本語",
			512U
		},
		{
			"Español",
			1024U
		},
		{
			"Al Arabiya",
			32U
		},
		{
			"Polskie",
			128U
		},
		{
			"简体中文",
			65536U
		},
		{
			"繁體中文",
			131072U
		},
		{
			"Gaeilge",
			262144U
		},
		{
			"Other",
			1U
		}
	};

	public void Load()
	{
		string path = Path.Combine(PlatformPaths.persistentDataPath, "languageFilter");
		try
		{
			if (File.Exists(path))
			{
				using (StreamReader streamReader = new StreamReader(path))
				{
					while (!streamReader.EndOfStream)
					{
						string[] array = streamReader.ReadLine().Split(new char[]
						{
							','
						});
						try
						{
							this.Languages[array[0]] = uint.Parse(array[1]);
						}
						catch
						{
						}
					}
				}
			}
		}
		catch
		{
		}
	}

	public void Save()
	{
		try
		{
			string text = Path.Combine(PlatformPaths.persistentDataPath, "languageFilterTemp");
			using (StreamWriter streamWriter = new StreamWriter(text, false))
			{
				foreach (KeyValuePair<string, uint> keyValuePair in this.Languages)
				{
					streamWriter.Write(keyValuePair.Key);
					streamWriter.Write(",");
					streamWriter.WriteLine(keyValuePair.Value);
				}
			}
			string text2 = Path.Combine(PlatformPaths.persistentDataPath, "languageFilter");
			File.Delete(text2);
			File.Move(text, text2);
		}
		catch
		{
		}
	}

	public string GetString(uint flag)
	{
		foreach (KeyValuePair<string, uint> keyValuePair in this.Languages)
		{
			if (keyValuePair.Value == flag)
			{
				return keyValuePair.Key;
			}
		}
		return "???";
	}
}
