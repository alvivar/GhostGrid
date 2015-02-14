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
/// ALT + S = Snap all game objects in the grid for the selected transform
/// ALT + A = Enable auto snap in the grid for the selected grid
/// ALT + D = Disable all running grids
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
            quantity = children.Length;

            for (int i = 0; i < quantity; i++)
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
    /// Menu item to snap all game objects in the grid for the selected transform.
    /// Shortcut: ALT + S
    /// </summary>
    [MenuItem("GhostGrid/Snap Grid &s")]
    private static void SnapSelectedGrid()
    {
        GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

        if (grid != null)
        {
            grid.SnapAll();

            Debug.Log("GhostGrid :: Grid snapped!");
        }
        else
        {
            Debug.Log("GhostGrid :: Selected transform doesn't know GhostGrid. Just add the component!");
        }
    }


    /// <summary>
    /// Disable the previous menu item if no transform is selected.
    /// </summary>
    [MenuItem("GhostGrid/Snap Grid &s", true)]
    private static bool ValidateSnapSelectedGrid()
    {
        return Selection.activeTransform != null;
    }


    /// <summary>
    /// Menu item to enable auto snap in the grid for the selected transform.
    /// Shortcut: ALT + A
    /// </summary>
    [MenuItem("GhostGrid/Enable Grid Auto Snap &a")]
    private static void EnableGridAutoSnap()
    {
        GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

        if (grid != null)
        {
            grid.SnapAll();
            grid.autoSnapEnabled = true;

            Debug.Log("GhostGrid :: Grid enabled for the selected transform!");
        }
        else
        {
            Debug.Log("GhostGrid :: Selected transform doesn't know GhostGrid.");
        }
    }


    /// <summary>
    /// Disable the previous menu item if no transform is selected.
    /// </summary>
    [MenuItem("GhostGrid/Enable Grid Auto Snap &a", true)]
    private static bool ValidateEnableGridAutoSnap()
    {
        return Selection.activeTransform != null;
    }


    /// <summary>
    /// Menu item to disable all running grids.
    /// Shortcut: ALT + D
    /// </summary>
    [MenuItem("GhostGrid/Disable All Grids &d")]
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
