
// A set of GameObject selection functions for the Editor.

// @matnesis
// 2016/09/18 10:56 PM


#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class GhostGridSelection : UnityEditor.Editor
{
    // Selects all Collider2D connected to the selected GameObject. Connected
    // means, the same parent, the same layer. This trick enables all Collider2D
    // on children under the parent to work.
    // #todo Enabled colliders need to return to his original state at the end.
    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders")]
    static GameObject[] Select2D()
    {
        // Selected
        var currentSelection = Selection.gameObjects;


        // We need a parent, and all colliders active
        if (currentSelection.Length > 0 && currentSelection[0].transform.parent)
        {
            var childrenColliders = currentSelection[0].transform.parent.GetComponentsInChildren<Collider2D>();
            for (int i = 0, len = childrenColliders.Length; i < len; i++)
                childrenColliders[i].enabled = true;
        }
        else
        {
            Debug.Log("GhostGrid :: The selected GameObject doesn't have a parent, this trick only works with connected brothers with 2D colliders.");
            return null;
        }


        // Data
        var selectedColl = currentSelection[0].GetComponent<Collider2D>();
        var selectedSize = Mathf.Max(selectedColl.bounds.size.x, selectedColl.bounds.size.x) * 1.10f;


        // Select first detected brothers from all selected
        var brothersAround = new List<Collider2D>();
        for (int i = 0, len = currentSelection.Length; i < len; i++)
        {
            brothersAround.AddRange(
                Physics2D.OverlapCircleAll(currentSelection[i].transform.position, selectedSize, 1 << currentSelection[0].layer)
                    .Where(x => x.transform.parent == currentSelection[i].transform.parent)
                    .Where(x => !brothersAround.Contains(x))
                    .ToList()
            );
        }


        // Looking for more
        var toBeSelected = new List<Collider2D>();
        var alreadyChecked = new List<Collider2D>();
        while (true)
        {
            // Detection
            int detected = 0;
            for (int i = 0, len = brothersAround.Count; i < len; i++)
            {
                if (!alreadyChecked.Contains(brothersAround[i])) alreadyChecked.Add(brothersAround[i]);

                // Around current brothers
                var around = Physics2D.OverlapCircleAll(brothersAround[i].transform.position, selectedSize, 1 << currentSelection[0].layer)
                    .Where(x => x.transform.parent == currentSelection[0].transform.parent)
                    .Where(x => !toBeSelected.Contains(x));

                detected += around.Count();

                // The unique
                toBeSelected.AddRange(around);
            }


            // If there is nothing new, the end
            if (detected < 1) break;


            // Let's repeat looking over the new ones
            brothersAround = toBeSelected.Where(x => !alreadyChecked.Contains(x)).ToList();
        }


        // Editor selection
        var result = toBeSelected.Select(x => x.gameObject).ToArray();
        Selection.objects = result;
        return result;
    }


    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders @X")]
    static void Select2DX()
    {
        var currentSelection = Selection.gameObjects;

        // From all, exclude
        var toBeSelected = new List<GameObject>();
        var allOfThem = Select2D();
        if (allOfThem.Length > 0)
        {
            for (int i = 0, len = currentSelection.Length; i < len; i++)
            {
                toBeSelected.AddRange(
                    allOfThem.Where(x => x.transform.position.y == currentSelection[i].transform.position.y)
                );
            }

            Selection.objects = toBeSelected.ToArray();
        }
    }


    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders @Y")]
    static void Select2DY()
    {
        var currentSelection = Selection.gameObjects;

        // From all, exclude
        var toBeSelected = new List<GameObject>();
        var allOfThem = Select2D();
        if (allOfThem.Length > 0)
        {
            for (int i = 0, len = currentSelection.Length; i < len; i++)
            {
                toBeSelected.AddRange(
                    allOfThem.Where(x => x.transform.position.x == currentSelection[i].transform.position.x)
                );
            }

            Selection.objects = toBeSelected.ToArray();
        }
    }


    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders @Left")]
    static void Select2DAllLeft()
    {
        GameObject selected = Selection.activeGameObject;

        // From all, exclude
        var allOfThem = Select2D();
        if (allOfThem != null)
        {
            Selection.objects = allOfThem.Where(x => x.transform.position.x <= selected.transform.position.x).ToArray();
        }
    }


    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders @Right")]
    static void Select2DAllRight()
    {
        GameObject selected = Selection.activeGameObject;

        // From all, exclude
        var allOfThem = Select2D();
        if (allOfThem != null)
        {
            Selection.objects = allOfThem.Where(x => x.transform.position.x >= selected.transform.position.x).ToArray();
        }
    }


    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders @Up")]
    static void Select2DAllUp()
    {
        GameObject selected = Selection.activeGameObject;

        // From all, exclude
        var allOfThem = Select2D();
        if (allOfThem != null)
        {
            Selection.objects = allOfThem.Where(x => x.transform.position.y >= selected.transform.position.y).ToArray();
        }
    }


    [MenuItem("Tools/GhostGrid/Select/Adjacent 2D Colliders @Down")]
    static void Select2DAllDown()
    {
        GameObject selected = Selection.activeGameObject;

        // From all, exclude
        var allOfThem = Select2D();
        if (allOfThem != null)
        {
            Selection.objects = allOfThem.Where(x => x.transform.position.y <= selected.transform.position.y).ToArray();
        }
    }
}

#endif
