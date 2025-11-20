using Reactor.Utilities.Attributes;
using Submerged.Localization.Strings;
using TMPro;
using UnityEngine;

namespace Submerged.UI;

[RegisterInIl2Cpp]
public sealed class CreditsScreenManager(nint ptr) : MonoBehaviour(ptr)
{
    private const string TRANSLATORS = "DekoKiyo (日本語), ItsNiceCraft (Deutsch), MissJukebox (Español),\n" +
                                       "Monid73 (Русский), PENGUN (Italiano), RevoLou (Português do Brasil), RobinRMC (Nederlands),\n" +
                                       "SPRLC (Français), ねろちゃん (日本語), 阿龍DragonTw (繁體中文), 黑客Hecker (简体中文)";

    public void Awake()
    {
        TMP_FontAsset font = FindObjectOfType<VersionShower>().text.font;

        foreach (TextMeshPro tmp in GetComponentsInChildren<TextMeshPro>())
        {
            tmp.font = font;
        }
    }

    public void OnEnable()
    {
        Transform textParent = transform.Find("Credits");
        textParent.Find("Project Lead").GetComponent<TextMeshPro>().text = General.Credits_ProjectLead;
        textParent.Find("Map Design").GetComponent<TextMeshPro>().text = General.Credits_MapDesign;
        textParent.Find("Developers").GetComponent<TextMeshPro>().text = General.Credits_Developers;
        textParent.Find("Artists").GetComponent<TextMeshPro>().text = General.Credits_Artists;
        textParent.Find("Technical Support").GetComponent<TextMeshPro>().text = ContributorsCredit;
        textParent.Find("Additional Art").GetComponent<TextMeshPro>().text = General.Credits_AdditionalArt;
        transform.Find("Translators/Text").GetComponent<TextMeshPro>().text = $"<u><b>{General.Credits_Translators}:</b></u> {TRANSLATORS}";
    }

    private static string ContributorsCredit => General.Credits_Contributors == "Contributors" && !string.IsNullOrWhiteSpace(Deprecated.Credits_TechnicalSupport)
        ? Deprecated.Credits_TechnicalSupport
        : General.Credits_Contributors;
}
