using System;
using ImaginationOverflow.UniversalDeepLinking;
using UnityEngine;
using UnityEngine.UI;

public class DeeplinkingExample : MonoBehaviour
{

    public GameObject Panel;
    public GameObject Reference;

    public GameObject InstructionText;
    void Start()
    {
        SetupUi();
        //
        //  When the game is activated via deeplink or web link call the Instance_LinkActivated method
        //
        DeepLinkManager.Instance.LinkActivated += Instance_LinkActivated;
    }

    private void SetupUi()
    {
        Reference.SetActive(false);
    }


    private void Instance_LinkActivated(LinkActivation s)
    {
        var go = Instantiate(Reference, Panel.transform);

        go.transform.GetChild(0).GetComponent<Text>().text = DateTime.Now.ToString("t");
        go.transform.GetChild(1).GetComponent<Text>().text = s.Uri;
        go.SetActive(true);

        InstructionText.SetActive(false);
        UpdateContentSize();
    }

    private void UpdateContentSize()
    {
        var trans = Panel.GetComponent<RectTransform>();
        trans.sizeDelta = new Vector2(trans.sizeDelta.x, trans.sizeDelta.y + 112);
    }


   
}
