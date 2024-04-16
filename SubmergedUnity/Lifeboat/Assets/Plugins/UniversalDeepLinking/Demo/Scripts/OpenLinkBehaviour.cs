using UnityEngine;
using UnityEngine.UI;

public class OpenLinkBehaviour : MonoBehaviour
{
    public string Url;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Open);
    }
	
    public void Open()
    {
        Application.OpenURL(Url);
    }
}