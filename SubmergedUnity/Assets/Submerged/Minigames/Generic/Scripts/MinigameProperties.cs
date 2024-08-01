#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(DivertPowerMetagame))]
public class MinigameProperties : MonoBehaviour
{
    public GameObject _gameObject;
    public StowArms _stowArms;
    public PolishRubyGame _polishRubyGame;
    public TextLink _textLink;
    public Tilemap2 _tilemap2;

    [ContextMenu("Find Exisintg Data Object")]
    public void FindObjects()
    {
        _gameObject = transform.Find("MinigameProperties").gameObject;

        _stowArms = _gameObject.GetComponent<StowArms>();
        _polishRubyGame = _gameObject.GetComponent<PolishRubyGame>();
        _textLink = _gameObject.GetComponent<TextLink>();
        _tilemap2 = _gameObject.GetComponent<Tilemap2>();
    }

    public void CreateObjects()
    {
        _gameObject = new GameObject("MinigameProperties");
        _gameObject.transform.parent = transform;
        _gameObject.transform.SetSiblingIndex(0);
        _gameObject.SetActive(false);

        _stowArms = _gameObject.AddComponent<StowArms>();
        _polishRubyGame = _gameObject.AddComponent<PolishRubyGame>();
        _textLink = _gameObject.AddComponent<TextLink>();
        _tilemap2 = _gameObject.AddComponent<Tilemap2>();

        #if UNITY_EDITOR
        Undo.SetCurrentGroupName("Create Data Object");

        Undo.RecordObject(gameObject, "");
        Undo.RegisterCreatedObjectUndo(_gameObject, "");

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        #endif
    }

    public void DestroyObjects()
    {
        #if UNITY_EDITOR
        Undo.SetCurrentGroupName("Destroy Data Object");

        Undo.RecordObject(gameObject, "");
        Undo.DestroyObjectImmediate(_gameObject);

        Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
        #endif
    }
}
