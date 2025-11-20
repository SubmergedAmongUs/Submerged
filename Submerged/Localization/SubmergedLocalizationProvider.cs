using System.Globalization;
using Reactor.Localization;
using Reactor.Localization.Extensions;
using Reactor.Utilities;
using Submerged.Enums;
using Submerged.Localization.Strings;

namespace Submerged.Localization;

public sealed class SubmergedLocalizationProvider : LocalizationProvider
{
    public override int Priority => ReactorPriority.HigherThanNormal;

    public override void OnLanguageChanged(SupportedLangs newLanguage)
    {
        CultureInfo cultureInfo = newLanguage.ToCultureInfo();

        Deprecated.Culture = cultureInfo;
        General.Culture = cultureInfo;
        Locations.Culture = cultureInfo;
        Tasks.Culture = cultureInfo;
    }

    public override bool TryGetText(StringNames stringName, out string result)
    {
        result = null;

        if ((int) stringName is < CustomStringNames.MINIMUM or > CustomStringNames.MAXIMUM) return false;

        if (!CustomStringNames.TryGetFromStringName(stringName, out CustomStringNames custom)) return false;
        result = custom.getter();

        return result != null;
    }

    public override bool TryGetStringName(SystemTypes systemType, out StringNames? result)
    {
        result = null;

        if ((int) systemType is < CustomSystemTypes.MINIMUM or > CustomSystemTypes.MAXIMUM) return false;

        if (!CustomSystemTypes.TryGetFromSystemType(systemType, out CustomSystemTypes custom)) return false;
        result = custom.stringName;

        return result != null;
    }

    public override bool TryGetStringName(TaskTypes taskType, out StringNames? result)
    {
        result = null;

        if ((int) taskType is < CustomTaskTypes.MINIMUM or > CustomTaskTypes.MAXIMUM) return false;

        if (!CustomTaskTypes.TryGetFromTaskType(taskType, out CustomTaskTypes custom)) return false;
        result = custom.stringName;

        return result != null;
    }
}
