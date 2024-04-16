using System;
using System.Linq;

public static class SystemTypeHelpers
{
	public static readonly SystemTypes[] AllTypes = Enum.GetValues(typeof(SystemTypes)).Cast<SystemTypes>().ToArray<SystemTypes>();
}
