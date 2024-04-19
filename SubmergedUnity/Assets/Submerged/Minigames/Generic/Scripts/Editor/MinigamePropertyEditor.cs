using Malee.List;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MinigameProperties))]
public class MinigamePropertyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MinigameProperties properties = (MinigameProperties) target;
        DivertPowerMetagame divertPowerMetagame = properties.GetComponent<DivertPowerMetagame>();

        EditorGUILayout.LabelField("Minigame Property Manager", new GUIStyle(EditorStyles.largeLabel)
        {
            fontStyle = FontStyle.BoldAndItalic,
            normal = new GUIStyleState
            {
                textColor = new Color(0.8f, 0.8f, 0.8f),
            },
        });
        EditorGUILayout.Space();

        if (!properties._gameObject && GUILayout.Button("Create Data Object")) properties.CreateObjects();
        if (properties._gameObject && GUILayout.Button("Destroy Data Object")) properties.DestroyObjects();

        if (!properties._gameObject) return;

        if (!divertPowerMetagame ||
            !properties._gameSettingMenu || !properties._polishRubyGame || !properties._stowArms || !properties._textLink || !properties._tilemap2)
        {
            EditorGUILayout.HelpBox("Missing data components!", MessageType.Error);
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        SerializedObject serializedDivert = new SerializedObject(divertPowerMetagame);
        SerializedProperty TransType = serializedDivert.FindProperty(nameof(DivertPowerMetagame.TransType));
        SerializedProperty OpenSound = serializedDivert.FindProperty(nameof(DivertPowerMetagame.OpenSound));
        SerializedProperty CloseSound = serializedDivert.FindProperty(nameof(DivertPowerMetagame.CloseSound));

        SerializedObject serializedMenu = new SerializedObject(properties._gameSettingMenu);
        SerializedProperty Transforms = serializedMenu.FindProperty(nameof(GameSettingMenu.AllItems));

        SerializedObject serializedRuby = new SerializedObject(properties._polishRubyGame);
        SerializedProperty AudioClips = serializedRuby.FindProperty(nameof(PolishRubyGame.rubSounds));
        SerializedProperty Integers = serializedRuby.FindProperty(nameof(PolishRubyGame.swipes));
        SerializedProperty Vector2s = serializedRuby.FindProperty(nameof(PolishRubyGame.directions));

        SerializedObject serializedStowArms = new SerializedObject(properties._stowArms);
        SerializedProperty GameObjects = serializedStowArms.FindProperty(nameof(StowArms.selectorSubobjects));
        SerializedProperty Colliders = serializedStowArms.FindProperty(nameof(StowArms.GunColliders));

        SerializedObject serializedTextLink = new SerializedObject(properties._textLink);
        SerializedProperty String = serializedTextLink.FindProperty(nameof(TextLink.targetUrl));

        SerializedObject serializedTilemap = new SerializedObject(properties._tilemap2);
        SerializedProperty Sprites = serializedTilemap.FindProperty(nameof(Tilemap2.sprites));

        string playerTaskName = "";
        string minigameName = "";

        string[] splits = String.stringValue.Split(new []{';'}, 2);
        if (splits.Length > 0) playerTaskName = splits[0];
        if (splits.Length > 1) minigameName = splits[1];

        playerTaskName = EditorGUILayout.TextField(new GUIContent("Injected PlayerTask Name"), playerTaskName);
        minigameName = EditorGUILayout.TextField(new GUIContent("Injected Minigame Name"), minigameName);

        String.stringValue = playerTaskName + ";" + minigameName;

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(TransType, new GUIContent("Transition Type"));
        EditorGUILayout.PropertyField(OpenSound, new GUIContent("Open Sound"));
        EditorGUILayout.PropertyField(CloseSound, new GUIContent("Close Sound"));

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        DoList(AudioClips, "Audio Clips");
        DoList(Colliders, "Colliders");
        DoList(GameObjects, "Game Objects");
        DoList(Integers, "Integers");
        DoList(Sprites, "Sprites");
        DoList(Transforms, "Transforms");
        DoList(Vector2s, "Vector2s");

        serializedDivert.ApplyModifiedProperties();
        serializedMenu.ApplyModifiedProperties();
        serializedStowArms.ApplyModifiedProperties();
        serializedRuby.ApplyModifiedProperties();
        serializedTextLink.ApplyModifiedProperties();
        serializedTilemap.ApplyModifiedProperties();
    }

    public static void DoList(SerializedProperty property, string label)
    {
        ReorderableList list = ReorderableDrawer.GetList(property, "Array");
        list.label = new GUIContent(label);
        list.elementDisplayType = ReorderableList.ElementDisplayType.SingleLine;
        list.DoLayoutList();
    }
}

[CustomEditor(typeof(DivertPowerMetagame))]
public class MinigamePropertyEditor_DivertPowerMetagame : Editor
{
    public override void OnInspectorGUI()
    {
        DivertPowerMetagame metagame = (DivertPowerMetagame) target;
        MinigameProperties properties = metagame.GetComponent<MinigameProperties>();

        if (properties == null)
        {
            base.OnInspectorGUI();
            return;
        }

        EditorGUI.HelpBox(
            GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)),
            "This component is being handled by MinigameProperties.",
            MessageType.Info);
    }
}

[CustomEditor(typeof(Minigame), true)]
public class MinigamePropertyEditor_Minigame : Editor
{
    public override void OnInspectorGUI()
    {
        Minigame minigame = (Minigame) target;
        MinigameProperties properties = minigame.GetComponent<MinigameProperties>();

        if (properties == null)
        {
            base.OnInspectorGUI();
            return;
        }

        EditorGUI.HelpBox(
            GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(EditorGUIUtility.singleLineHeight * 1.5f)),
            "This component is being handled by MinigameProperties.",
            MessageType.Info);
    }
}
