
// GhostGrid v0.1.3.5 alpha

// Lightweight grid component with auto snapping. Just add 'GhostGrid.cs' to
// any GameObject to activate the grid for him and his children.

// Check out 'Tools/GhostGrid' in the menu for shortcuts.


// Created by Andrés Villalobos [andresalvivar@gmail.com] [twitter.com/matnesis]
// 07/01/2015 3:21 am


using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


/// <summary>
/// Lightweight grid component with auto snapping. Just add 'GhostGrid.cs' to
/// any GameObject to activate the grid for him and his children.
/// </summary>
[ExecuteInEditMode]
public class GhostGrid : MonoBehaviour
{
	public float gridSize = 1f;
	public int quantity = 0;

	[HideInInspector]
	public bool autoSnapEnabled = false;
	private Transform[] children = null;

	private static List<GhostGrid> others = null;


#if UNITY_EDITOR
	void Update()
	{
		if (autoSnapEnabled == false)
			return;

		// Stop while playing
		if (Application.isPlaying)
			autoSnapEnabled = false;

		// On any changes
		SnapAll();
	}
#endif


	void OnEnable()
	{
		// Save the reference for menu items maneuvers
		if (others == null)
			others = new List<GhostGrid>();

		if (others.Contains(this) == false)
			others.Add(this);
	}


	void OnDisable()
	{
		// Remove yourself from the list
		if (others.Contains(this))
			others.Remove(this);
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
				children[i].position = GetSnapVector(children[i].position, gridSize);
			}
		}
	}


	/// <summary>
	/// Returns the snap position for the current vector on a simulated virtual grid.
	/// </summary>
	public static Vector3 GetSnapVector(Vector3 vector, float gridSize)
	{
		vector.x = Mathf.Round(vector.x / gridSize) * gridSize;
		vector.y = Mathf.Round(vector.y / gridSize) * gridSize;
		vector.z = Mathf.Round(vector.z / gridSize) * gridSize;

		return vector;
	}


	/// <summary>
	/// Changes the parent of all overlapped children (with same position) to
	/// [GhostGrid:Overlapped] GameObject and returns the excluded count.
	/// </summary>
	public int ExcludeOverlappedChildren()
	{
		List<Transform> safeChildren = new List<Transform>();
		Transform overlappedParent = this.GetOrCreateTransform("[GhostGrid:Overlapped]");


		int excludedCount = 0;

		children =  GetComponentsInChildren<Transform>();
		quantity = children.Length;

		for (int i = 0; i < quantity; i++)
		{
			// The first will be the safe forever
			safeChildren.Add(children[i]);


			for (int j = 0; j < quantity; j++)
			{
				// Ignore self
				if (children[i] == children[j])
					continue;


				// Ignore parents
				if (children[i] == children[j].parent)
					continue;


				// Overlapped!
				if (children[i].position == children[j].position)
				{
					// Ignore safe children
					if (!safeChildren.Contains(children[j]))
					{
						children[i].parent = overlappedParent;
						excludedCount += 1;
					}
				}
			}
		}


		return excludedCount;
	}


	/// <summary>
	/// Enable all colliders that are borders and disable all colliders that
	/// are sorrounded by other colliders. Returns the count of disabled
	/// colliders.
	/// </summary>
	public int TurnOffUnneededColliders2D()
	{
		// List to collect the unneeded colliders
		List<Collider2D> unneededColliders = new List<Collider2D>();


		// Children colliders
		Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
		quantity = colliders.Length;


		// First, reactivate all
		for (int i = 0; i < quantity; i++)
			colliders[i].enabled = true;


		// Then, collect the unneeded colliders by checking around them
		for (int i = 0; i < quantity; i++)
		{
			Vector3 pos = colliders[i].transform.position;
			float rayLength = Mathf.Max(colliders[i].bounds.extents.x, colliders[i].bounds.extents.y) * 1.1f;
			int sorroundedCount = 0;


			// Test for adjacent colliders ignoring itself
			colliders[i].enabled = false;

			if (Physics2D.Raycast(pos, Vector2.up, rayLength, layer))
				sorroundedCount += 1;

			if (Physics2D.Raycast(pos, Vector2.down, rayLength, layer))
				sorroundedCount += 1;

			if (Physics2D.Raycast(pos, Vector2.left, rayLength, layer))
				sorroundedCount += 1;

			if (Physics2D.Raycast(pos, Vector2.right, rayLength, layer))
				sorroundedCount += 1;

			colliders[i].enabled = true;


			// Colliders that are completely sorrounded by others
			if (sorroundedCount == 4)
				unneededColliders.Add(colliders[i]);
		}


		// Turn off chosen colliders
		foreach (Collider2D c in unneededColliders)
			c.enabled = false;


		return unneededColliders.Count;
	}


#if UNITY_EDITOR
	/// <summary>
	/// Menu item to snap all game objects in the grid for the selected transform.
	/// Shortcut: ALT + S
	/// </summary>
	[MenuItem("Tools/GhostGrid/Snap Grid Once &s")]
	private static void SnapSelectedGrid()
	{
		GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

		if (grid != null)
		{
			grid.autoSnapEnabled = false;
			grid.SnapAll();

			Debug.Log("GhostGrid :: " + grid.quantity + " elements snapped! Grid auto snap disabled.");
		}
		else
		{
			Debug.Log("GhostGrid :: GhostGrid not found on selected transform (or parents).");
		}
	}


	/// <summary>
	/// Disable the previous menu item if no transform is selected.
	/// </summary>
	[MenuItem("Tools/GhostGrid/Snap Grid Once &s", true)]
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
			Debug.Log("GhostGrid :: GhostGrid not found on selected transform (or parents).");
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
	[MenuItem("Tools/GhostGrid/Disable Auto Snap In All Grids &d")]
	private static void DisableAllGrids()
	{
		Debug.Log("GhostGrid :: Auto snap disabled for all grids.");

		if (others == null)
			return;

		for (int i = 0; i < others.Count; i++)
		{
			others[i].autoSnapEnabled = false;
		}
	}


	/// <summary>
	/// Menu item to exclude overlapped children to [GhostGrid:Overlapped].
	/// </summary>
	[MenuItem("Tools/GhostGrid/Exclude Overlapped Children")]
	private static void PurgeAll()
	{
		GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

		if (grid != null)
		{
			int howMany = grid.ExcludeOverlappedChildren();

			if (howMany > 0)
			{
				Debug.Log("GhostGrid :: " + howMany + " overlapped children moved to [GhostGrid:Overlapped] GameObject.");
			}
			else
			{
				Debug.Log("GhostGrid :: The grid is clean! Nothing overlapped.");
			}
		}
		else
		{
			Debug.Log("GhostGrid :: GhostGrid not found on selected transform (or parents).");
		}
	}


	/// <summary>
	/// Disable the previous menu item if no transform is selected.
	/// </summary>
	[MenuItem("Tools/GhostGrid/Exclude Overlapped Children", true)]
	private static bool ValidatePurgeAll()
	{
		return Selection.activeTransform != null;
	}


	/// <summary>
	/// Menu item to optimize colliders by turning off the unneeded.
	/// </summary>
	[MenuItem("Tools/GhostGrid/Turn Off Unneeded 2D Colliders")]
	private static void MenuTurnOffUnneededColliders()
	{
		GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

		if (grid != null)
		{
			int howMany = grid.TurnOffUnneededColliders2D();

			if (howMany > 0)
			{
				Debug.Log("GhostGrid :: " + howMany + " unneeded colliders were turned off!");
			}
		}
		else
		{
			Debug.Log("GhostGrid :: GhostGrid not found on selected transform (or parents).");
		}
	}


	/// <summary>
	/// Disable the previous menu item if no transform is selected.
	/// </summary>
	[MenuItem("Tools/GhostGrid/Turn Off Unneeded 2D Colliders", true)]
	private static bool ValidateMenuTurnOffUnneededColliders()
	{
		return Selection.activeTransform != null;
	}
#endif
}
