#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


/// <summary>
/// Editor controls for GhostGrid.
/// </summary>
[CustomEditor(typeof(GhostGrid))]
public class GhostGridEditor : Editor
{
    private GhostGrid grid = null;


    public void OnEnable()
    {
        grid = target as GhostGrid;
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        // Auto snap button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(grid.autoSnapEnabled ? "Disable Auto Snap" : "Enable Auto Snap", GUILayout.ExpandWidth(false)))
        {
            grid.autoSnapEnabled = !grid.autoSnapEnabled;

            if (grid.autoSnapEnabled)
                grid.SnapAll();
        }

        // Snap once button
        if (GUILayout.Button("Snap Once", GUILayout.ExpandWidth(false)))
        {
            if (grid.autoSnapEnabled)
            {
                grid.autoSnapEnabled = false;
            }
            else
            {
                grid.SnapAll();
            }
        }
        GUILayout.EndHorizontal();

        // Status label
        GUILayout.Label(grid.autoSnapEnabled ? "Auto Snap Running!" : "Stopped.");
    }
}
#endif
