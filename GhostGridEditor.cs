#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


/// <summary>
/// Editor controls for GhostGrid.
/// </summary>
[CustomEditor(typeof(GhostGrid))]
public class GhostGridEditor : Editor
{
    private GhostGrid grid;
    private string message;


    public void OnEnable()
    {
        grid = target as GhostGrid;
        message = "";
    }


    public override void OnInspectorGUI()
    {
        GUILayout.Label("");
        DrawDefaultInspector();
        GUILayout.Label("");


        GUILayout.BeginHorizontal();
        // Snap once button
        if (GUILayout.Button("Snap Once", GUILayout.ExpandWidth(false)))
        {
            message = "Grid snapped!";


            if (grid.autoSnapEnabled)
            {
                grid.autoSnapEnabled = false;
            }
            else
            {
                grid.SnapAll();
            }
        }


        // Auto snap button
        if (GUILayout.Button(grid.autoSnapEnabled ? "Disable Auto Snap" : "Enable Auto Snap", GUILayout.ExpandWidth(false)))
        {
            message = "Changed!";


            grid.autoSnapEnabled = !grid.autoSnapEnabled;

            if (grid.autoSnapEnabled)
                grid.SnapAll();
        }
        GUILayout.EndHorizontal();


        // Exclude overlapped button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Exclude Overlapped", GUILayout.ExpandWidth(false)))
        {
            message = "Exclusion done!";
            grid.tt().ttAdd(1, () => message = "");


            grid.ExcludeOverlappedChildren();
        }
        GUILayout.EndHorizontal();


        // Status label
        GUILayout.Label("");
        GUILayout.Label(grid.autoSnapEnabled ? "Auto Snap Running!" : "Auto Snap Disabled.");
        if (message.Length > 0)
            GUILayout.Label(message);


        // Credits
        GUILayout.Label("");
        GUILayout.Label("GhostGrid v0.1.5 by @matnesis");
    }
}
#endif
