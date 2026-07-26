using UnityEditor;
using UnityEngine;
using System.Linq;

/// <summary>
/// Adds a "Load Frames From Folder" button to FramePlayer's Inspector so you
/// don't have to manually multi-select and drag PNGs into the array (which
/// is easy to get wrong - dropping on a single slot instead of the whole
/// list only assigns one frame).
///
/// Must live in a folder named exactly "Editor" - that's what tells Unity
/// this script is editor-only and won't be included in a build.
/// </summary>
[CustomEditor(typeof(FramePlayer))]
public class FramePlayerEditor : Editor
{
    string folderPath = "Assets/Sprites/Layers/pipe_leak_drip";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Load", EditorStyles.boldLabel);
        folderPath = EditorGUILayout.TextField("Folder Path", folderPath);

        if (GUILayout.Button("Load Frames From Folder"))
        {
            LoadFrames((FramePlayer)target, folderPath);
        }

        EditorGUILayout.HelpBox(
            "Folder Path is relative to the project, e.g.\n" +
            "Assets/Sprites/Layers/pipe_leak_drip\n" +
            "Assets/Sprites/Layers/oxygen_needle\n" +
            "Assets/Sprites/Layers/error_blink\n" +
            "Click the button - no manual dragging needed.",
            MessageType.Info);
    }

    void LoadFrames(FramePlayer player, string path)
    {
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { path });
        var sprites = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(p => p) // filenames are zero-padded (e.g. _000, _001) so this sorts correctly
            .Select(AssetDatabase.LoadAssetAtPath<Sprite>)
            .Where(s => s != null)
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found at '{path}'. Check the path is correct " +
                              "and the PNGs are imported as Sprite (2D and UI).");
            return;
        }

        Undo.RecordObject(player, "Load Frames");
        player.frames = sprites;
        EditorUtility.SetDirty(player);
        Debug.Log($"Loaded {sprites.Length} frames into {player.gameObject.name} from '{path}'.");
    }
}
