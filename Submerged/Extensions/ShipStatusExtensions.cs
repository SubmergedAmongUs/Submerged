using Submerged.Enums;

namespace Submerged.Extensions;

public static class ShipStatusExtensions
{
    public static bool IsSubmerged(this ShipStatus shipStatus) => shipStatus && shipStatus.Type == CustomMapTypes.Submerged;
}
