
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GhostGrid))]
public class GhostGridEditor : Editor
{
    private GhostGrid grid;
    private string message;

    private bool doRename = false;
    private bool doCleanOverlappedChildren = false;
    private bool doTurnOffUnneededColliders2D = false;


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
        GUILayout.Label("Snapping");
        GUILayout.BeginHorizontal();


        // Snap Once
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

        // Auto Snap
        if (GUILayout.Button(grid.autoSnapEnabled ? "Disable Auto Snap" : "Enable Auto Snap", GUILayout.ExpandWidth(false)))
        {
            message = "Done!";

            grid.autoSnapEnabled = !grid.autoSnapEnabled;

            if (grid.autoSnapEnabled)
                grid.SnapAll();
        }
        GUILayout.EndHorizontal();


        // Optimizations
        GUILayout.Label("");
        GUILayout.Label("Optimizations");


        // ^
        // Toogle set

        GUILayout.BeginHorizontal();
        doRename = GUILayout.Toggle(doRename, "Rename children");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        doCleanOverlappedChildren = GUILayout.Toggle(doCleanOverlappedChildren, "Delete overlapped");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        doTurnOffUnneededColliders2D = GUILayout.Toggle(doTurnOffUnneededColliders2D, "Turn off unneeded 2D colliders on layer");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false)))
        {
            if (doRename)
                message = grid.RenameChildren() + " children renamed!";

            if (doCleanOverlappedChildren)
                message = grid.ExcludeOverlappedChildren() + " overlapped children deleted!";

            if (doTurnOffUnneededColliders2D)
                message = grid.TurnOffUnneededColliders2D() + " unneeded colliders were turned off!";
        }
        GUILayout.EndHorizontal();


        // Status label
        GUILayout.Label("");
        GUILayout.Label(grid.autoSnapEnabled ? "Auto Snap Running!" : "Auto Snap Disabled.");
        if (message.Length > 0) GUILayout.Label(message);


        // Credits
        GUILayout.Label("");
        GUILayout.Label("GhostGrid v0.1.3.7a by @matnesis");
    }
}
#endif
