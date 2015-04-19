// GhostGrid v0.1 alpha

// Lightweight grid component with auto snapping. Just add 'GhostGrid.cs' to any
// transform to activate the grid for him and his children.

// - ALT + S = Snap all game objects in the grid for the selected transform
// - ALT + A = Enable auto snap in the grid for the selected grid
// - ALT + D = Disable all running grids


// Created by Andrés Villalobos [andresalvivar@gmail.com] [twitter.com/matnesis]
// 07/01/2015 3:21 am


using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Lightweight grid component with auto snapping. Just add 'GhostGrid.cs' to any
/// transform to activate the grid for him and his children.
/// </summary>
[ExecuteInEditMode]
public class GhostGrid : MonoBehaviour
{
    public float gridSize = 1f;
    public int quantity = 0;

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


    void OnEnable()
    {
        // Save the reference for menu items maneuvers
        if (brothers == null)
            brothers = new List<GhostGrid>();

        if (brothers.Contains(this) == false)
            brothers.Add(this);
    }


    void OnDisable()
    {
        // Remove yourself from the list
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
            quantity = children.Length;

            for (int i = 0; i < quantity; i++)
            {
                children[i].position = GetSnapPosition(children[i], gridSize);
            }
        }
    }


    /// <summary>
    /// Returns the snap position for the transform on a virtual grid.
    /// </summary>
    public static Vector3 GetSnapPosition(Transform t, float gridSize)
    {
        Vector3 pos = t.position;
        Vector3 snapped = new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            Mathf.Round(pos.y / gridSize) * gridSize,
            Mathf.Round(pos.z / gridSize) * gridSize);

        return snapped;
    }


#if UNITY_EDITOR
    /// <summary>
    /// Menu item to snap all game objects in the grid for the selected transform.
    /// Shortcut: ALT + S
    /// </summary>
    [MenuItem("Tools/GhostGrid/Snap Grid &s")]
    private static void SnapSelectedGrid()
    {
        GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

        if (grid != null)
        {
            grid.SnapAll();

            Debug.Log("GhostGrid :: " + grid.quantity + " elements snapped!");
        }
        else
        {
            Debug.Log("GhostGrid :: Selected transform doesn't know GhostGrid. (Add the component!)");
        }
    }


    /// <summary>
    /// Disable the previous menu item if no transform is selected.
    /// </summary>
    [MenuItem("Tools/GhostGrid/Snap Grid &s", true)]
    private static bool ValidateSnapSelectedGrid()
    {
        return Selection.activeTransform != null;
    }


    /// <summary>
    /// Menu item to enable auto snap in the grid for the selected transform.
    /// Shortcut: ALT + A
    /// </summary>
    [MenuItem("Tools/GhostGrid/Enable Grid Auto Snap &a")]
    private static void EnableGridAutoSnap()
    {
        GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

        if (grid != null)
        {
            grid.SnapAll();
            grid.autoSnapEnabled = true;

            Debug.Log("GhostGrid :: Grid auto snap enabled!");
        }
        else
        {
            Debug.Log("GhostGrid :: Selected transform doesn't know GhostGrid. (Add the component!)");
        }
    }


    /// <summary>
    /// Disable the previous menu item if no transform is selected.
    /// </summary>
    [MenuItem("Tools/GhostGrid/Enable Grid Auto Snap &a", true)]
    private static bool ValidateEnableGridAutoSnap()
    {
        return Selection.activeTransform != null;
    }


    /// <summary>
    /// Menu item to disable all running grids.
    /// Shortcut: ALT + D
    /// </summary>
    [MenuItem("Tools/GhostGrid/Disable All Grids &d")]
    private static void DisableAllGrids()
    {
        Debug.Log("GhostGrid :: Auto snap disabled for all grids.");

        if (brothers == null)
            return;

        for (int i = 0; i < brothers.Count; i++)
        {
            brothers[i].autoSnapEnabled = false;
        }
    }
#endif
}
