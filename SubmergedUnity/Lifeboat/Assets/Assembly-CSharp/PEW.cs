using System;
using System.Runtime.CompilerServices;

public static class PEW
{
	public static class Hardware
	{
		private static PEW.Hardware.HardwareType type = PEW.Hardware.GetHardwareType();

		private static int tier = PEW.Hardware.GetHardwareTier();

		public static PEW.Hardware.HardwareType hardwareType
		{
			get
			{
				return PEW.Hardware.type;
			}
			private set
			{
				PEW.Hardware.type = value;
			}
		}

		public static int hardwareTier
		{
			get
			{
				return PEW.Hardware.tier;
			}
			private set
			{
				PEW.Hardware.tier = value;
			}
		}

		private static PEW.Hardware.HardwareType GetHardwareType()
		{
			return PEW.Hardware.HardwareType.Unknown;
		}

		private static int GetHardwareTier()
		{
			return 4;
		}

		public enum HardwareType
		{
			PC,
			Switch,
			XboxOne,
			XboxScarlett,
			PS4,
			PS5,
			Stadia,
			Unknown
		}
	}

	[Obsolete("\nDon't reference this class directly, create a class that derives from it! Unity won't properly serialize classes with generics.\n\npublic class ConditionalWhatever : ConditionalValue<Whatever> { }; \n\n(Ignore if you're already doing that)")]
	public class ConditionalValue<T>
	{
		public T PC;

		public T Switch;

		public T XboxOne;

		public T XboxOneX;

		public T PS4;

		public T PS4Pro;

		public T PS5;

		public T Stadia;

		public T Misc;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public T Select()
		{
			return this.PC;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator T(PEW.ConditionalValue<T> input)
		{
			return input.Select();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator PEW.ConditionalValue<T>(T input)
		{
			return new PEW.ConditionalValue<T>
			{
				PC = input,
				Switch = input,
				XboxOne = input,
				XboxOneX = input,
				PS4 = input,
				PS4Pro = input,
				PS5 = input,
				Stadia = input,
				Misc = input
			};
		}
	}
}
