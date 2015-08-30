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
        GUILayout.Label("SNAPPING");
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


        GUILayout.Label("");
        GUILayout.Label("OPTIMIZATIONS");
        GUILayout.BeginHorizontal();

        // Exclude overlapped button
        if (GUILayout.Button("Exclude Overlapped Children", GUILayout.ExpandWidth(false)))
        {
            message = grid.ExcludeOverlappedChildren() + " overlapped children deleted!";
        }
        GUILayout.EndHorizontal();


        // Optimize colliders button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Turn Off Unneeded 2D Colliders", GUILayout.ExpandWidth(false)))
        {
            message = grid.TurnOffUnneededColliders2D() + " unneeded colliders were turned off!";
        }
        GUILayout.EndHorizontal();


        // All optimizations colliders button
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("ALL ^", GUILayout.ExpandWidth(false)))
        {
            message =
                grid.ExcludeOverlappedChildren() + " overlapped children deleted!" + "\n" +
                grid.TurnOffUnneededColliders2D() + " unneeded colliders were turned off!";
        }
        GUILayout.EndHorizontal();


        GUILayout.Label("");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("#experimental Polygon Builder", GUILayout.ExpandWidth(false)))
        {
            message = grid.RebuildPolygon() + "";
        }
        GUILayout.EndHorizontal();


        // Status label
        GUILayout.Label("");
        GUILayout.Label(grid.autoSnapEnabled ? "Auto Snap Running!" : "Auto Snap Disabled.");
        if (message.Length > 0)
            GUILayout.Label(message);


        // Credits
        GUILayout.Label("");
        GUILayout.Label("GhostGrid v0.1.3.5 by @matnesis");
    }
}
#endif
