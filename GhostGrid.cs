using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Lightweight grid component with auto snapping for the current transform and
/// his children.
/// Alt + A = Enable grid for selected transform
/// Alt + S = Snap everything
/// Alt + D = Disable all grids
/// </summary>
[ExecuteInEditMode]
public class GhostGrid : MonoBehaviour
{
    public float gridSize = 1f;

    [HideInInspector]
    public bool autoSnapEnabled = false;
    private Transform[] children = null;

    private static List<GhostGrid> brothers = null;


    void Update()
    {
        if (autoSnapEnabled == false)
            return;

        // Stop while playing
        if (Application.isPlaying)
            autoSnapEnabled = false;

        // Mouse up
        SnapAll();
    }


    public void OnEnable()
    {
        // Save the reference for menu items maneuvers
        if (brothers == null)
            brothers = new List<GhostGrid>();

        if (brothers.Contains(this) == false)
            brothers.Add(this);
    }


    public void OnDisable()
    {
        // Clean up
        if (brothers.Contains(this))
            brothers.Remove(this);
    }


    /// <summary>
    /// Snap all children to the grid.
    /// </summary>
    public void SnapAll()
    {
        if (gridSize > 0)
        {
            children = GetComponentsInChildren<Transform>();

            for (int i = 0; i < children.Length; i++)
            {
                Vector3 pos = children[i].position;
                Vector3 snapped = new Vector3(
                    Mathf.Round(pos.x / gridSize) * gridSize,
                    Mathf.Round(pos.y / gridSize) * gridSize,
                    Mathf.Round(pos.z / gridSize) * gridSize);
                children[i].position = snapped;
            }
        }
    }


#if UNITY_EDITOR
    /// <summary>
    /// Menu item to enable the grid for the selected transform.
    /// </summary>
    [MenuItem("GhostGrid/Enable autosnap &a")]
    private static void LogSelectedTransformName()
    {
        GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

        if (grid != null)
        {
            grid.autoSnapEnabled = true;
            grid.SnapAll();

            Debug.Log("GhostGrid :: Grid enabled for selected transform!");
        }
        else
        {
            Debug.Log("GhostGrid :: Selected transform doesn't know GhostGrid.");
        }
    }


    /// <summary>
    /// Disable the previous menu item if no transform is selected.
    /// </summary>
    [MenuItem("GhostGrid/Enable autosnap &a", true)]
    private static bool ValidateLogSelectedTransformName()
    {
        return Selection.activeTransform != null;
    }


    /// <summary>
    /// Menu item to snap all to the grids.
    /// Shortcut: Shift + S
    /// </summary>
    [MenuItem("GhostGrid/Snap everything &s")]
    private static void SnapEverything()
    {
        Debug.Log("GhostGrid :: Everything was snapped!");

        if (brothers == null)
            return;

        for (int i = 0; i < brothers.Count; i++)
        {
            brothers[i].SnapAll();
        }
    }


    /// <summary>
    /// Menu item to disable all running grids.
    /// Shortcut: Shift + D
    /// </summary>
    [MenuItem("GhostGrid/Disable all grids &d")]
    private static void DisableAllGrids()
    {
        Debug.Log("GhostGrid :: All grids disabled.");

        if (brothers == null)
            return;

        for (int i = 0; i < brothers.Count; i++)
        {
            brothers[i].autoSnapEnabled = false;
        }
    }
#endif
}
