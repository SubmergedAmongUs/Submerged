using System;

namespace InnerNet
{
	[Flags]
	public enum GameKeywords : uint
	{
		All = 0U,
		English = 256U,
		SpanishLA = 2U,
		Brazilian = 2048U,
		Portuguese = 16U,
		Korean = 4U,
		Russian = 8U,
		Dutch = 4096U,
		Filipino = 64U,
		French = 8192U,
		German = 16384U,
		Italian = 32768U,
		Japanese = 512U,
		SpanishEU = 1024U,
		Arabic = 32U,
		Polish = 128U,
		SChinese = 65536U,
		TChinese = 131072U,
		Irish = 262144U,
		Other = 1U
	}
}
