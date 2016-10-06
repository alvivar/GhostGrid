
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GhostGrid))]
public class GhostGridEditor : Editor
{
    private GhostGrid grid;
    private string message = "";


    public void OnEnable()
    {
        grid = target as GhostGrid;
    }


    public void OnSceneGUI()
    {
        if (grid.doExtrusionMode)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

            if (Selection.activeGameObject != grid.gameObject)
                grid.doExtrusionMode = false;
        }


        // if (grid.doExtrusionMode)
        // {
        //     // Retrieve the control Id
        //     int controlId = GUIUtility.GetControlID(FocusType.Passive);

        //     // Start treating your events
        //     switch (Event.current.type)
        //     {
        //         case EventType.MouseDown:

        //             // Treat your event
        //             Debug.Log("Mouse down " + Time.time);

        //             // Tell the UI your event is the main one to use, it override the selection in  the scene view
        //             GUIUtility.hotControl = controlId;

        //             // Don't forget to use the event
        //             Event.current.Use();

        //             break;
        //     }
        // }
    }


    public override void OnInspectorGUI()
    {
        GUILayout.Label("");
        DrawDefaultInspector();


        // Snapping
        GUILayout.Label("");
        EditorGUILayout.LabelField("Snapping", EditorStyles.boldLabel);
        GUILayout.Label(
            (grid.doAutoSnap ? "Auto Snap is ON" : "Auto Snap is off") +
            (grid.childrenCount > 1 ? " / " + (grid.childrenCount - 1) + " children" : ""));
        GUILayout.BeginHorizontal();

        // Snap Once
        if (GUILayout.Button("Snap Once", GUILayout.ExpandWidth(false)))
        {
            message = "Grid snapped!";

            if (grid.doAutoSnap)
            {
                grid.doAutoSnap = false;
            }
            else
            {
                grid.SnapAll();
            }
        }

        // Auto Snap
        if (GUILayout.Button(grid.doAutoSnap ? "Disable Auto Snap" : "Enable Auto Snap", GUILayout.ExpandWidth(false)))
        {
            grid.doAutoSnap = !grid.doAutoSnap;

            if (grid.doAutoSnap)
                grid.SnapAll();
        }
        GUILayout.EndHorizontal();


        // Optimizations
        GUILayout.Label("");
        EditorGUILayout.LabelField("Optimizations", EditorStyles.boldLabel);

        // Toogle set
        GUILayout.BeginHorizontal();
        grid.doCleanOverlappedChildren = GUILayout.Toggle(grid.doCleanOverlappedChildren, "Delete overlapped");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        grid.doTurnOffUnneededColliders2D = GUILayout.Toggle(grid.doTurnOffUnneededColliders2D, "Turn off unneeded 2D colliders (not borders)");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        grid.doTurnOffRenderers = GUILayout.Toggle(grid.doTurnOffRenderers, "Turn off Renderers");
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        grid.doRename = GUILayout.Toggle(grid.doRename, "Rename children (1..n)");
        GUILayout.EndHorizontal();

        // Apply!
        if (grid.doCleanOverlappedChildren || grid.doTurnOffUnneededColliders2D || grid.doRename)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Apply Now", GUILayout.ExpandWidth(false)))
            {
                message = "On " + (grid.childrenCount - 1) + " children:\n";

                if (grid.doCleanOverlappedChildren)
                    message += "+ " + grid.ExcludeOverlappedChildren(true) + " overlapped deleted\n";

                if (grid.doTurnOffUnneededColliders2D)
                    message += "+ " + grid.TurnOffUnneededColliders2D() + " unneeded 2D colliders were turned off\n";

                if (grid.doTurnOffRenderers)
                    message += "+ " + grid.TurnOffRenderers() + " Renderers were turned off\n";

                // Rename should be last
                if (grid.doRename)
                    message += "+ " + grid.RenameChildren() + " renamed\n";
            }
            GUILayout.EndHorizontal();
        }


        // Message
        GUILayout.Label("");
        if (message.Length > 0)
            GUILayout.Label(message.Trim());


        // Credits
        GUILayout.Label("\nGhostGrid v0.1.3.8 by @matnesis");
    }
}
#endif
