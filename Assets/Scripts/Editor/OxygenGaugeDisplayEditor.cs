using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(OxygenGaugeDisplay))]
public class OxygenGaugeDisplayEditor : Editor
{
    string folderPath = "Assets/Sprites/Layers/oxygen_needle";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Load", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Load Frames From Folder"))
        {
            LoadFrames((OxygenGaugeDisplay)target, folderPath);
        }
    }

    void LoadFrames(OxygenGaugeDisplay gauge, string path)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { path });
        var sprites = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(p => p)
            .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
            .Where(s => s != null)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found at '{path}'.");
            return;
        }

        Undo.RecordObject(gauge, "Load Frames");
        gauge.frames = sprites;
        EditorUtility.SetDirty(gauge);
        Debug.Log($"Loaded {sprites.Length} frames into {gauge.gameObject.name} from '{path}'.");
    }
}
