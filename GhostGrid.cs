
// GhostGrid v0.1.3.8

// Lightweight grid component with auto snapping & additional magic. Just add
// 'GhostGrid.cs' to any GameObject to activate the grid for him and his
// children.

// Check out 'Tools/GhostGrid' in the menu for shortcuts!

// Andrés Villalobos ~ twitter.com/@matnesis ~ andresalvivar@gmail.com
// 07/01/2015 3:21 am


#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;


[ExecuteInEditMode]
public class GhostGrid : MonoBehaviour
{
	[Header("Config")]
	public float gridSize = 1f;

	// Data shared with the editor script
	[HideInInspector]
	public int childrenCount = 0;
	[HideInInspector]
	public bool doAutoSnap = false;
	[HideInInspector]
	public bool doRename = false;
	[HideInInspector]
	public bool doCleanOverlappedChildren = false;
	[HideInInspector]
	public bool doTurnOffUnneededColliders2D = false;

	private LayerMask layer;
	private Transform[] children = null;
	private static List<GhostGrid> others = null;

	private const string NOT_FOUND = "GhostGrid :: GhostGrid not found on selected GameObject (or parents).";


	void Update()
	{
		if (!doAutoSnap)
			return;


		// Stop while playing
		if (Application.isPlaying)
			doAutoSnap = false;


		// On any changes
		SnapAll();
	}


	void OnEnable()
	{
		// Save the reference for menu items maneuvers
		if (others == null)
			others = new List<GhostGrid>();

		// Add yourself
		if (!others.Contains(this))
			others.Add(this);
	}


	void OnDisable()
	{
		// Remove yourself
		if (others.Contains(this))
			others.Remove(this);
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
	/// Returns the Transform with the name.
	/// </summary>
	public static Transform GetCreateTransform(string name)
	{
		GameObject go = GameObject.Find(name);

		if (go == null)
			go = new GameObject(name);

		return go.transform;
	}


	/// <summary>
	/// Snap all children to the grid.
	/// </summary>
	public void SnapAll()
	{
		if (gridSize > 0)
		{
			children = GetComponentsInChildren<Transform>();
			childrenCount = children.Length ;

			for (int i = 0; i < childrenCount; i++)
				children[i].position = GetSnapVector(children[i].position, gridSize);
		}
	}


	/// <summary>
	/// Changes the parent of all overlapped children (with same position) to
	/// [GhostGrid|Overlapped] and returns the excluded count.
	/// </summary>
	public int ExcludeOverlappedChildren(bool alsoDeleteIt = false)
	{
		List<Transform> safeChildren = new List<Transform>();
		Transform overlappedParent = GetCreateTransform("[GhostGrid|Overlapped]");


		children = GetComponentsInChildren<Transform>();
		childrenCount = children.Length;
		int excludedCount = 0;

		for (int i = 0; i < childrenCount; i++)
		{
			// The first will be the safe always
			safeChildren.Add(children[i]);


			for (int j = 0; j < childrenCount; j++)
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


		if (alsoDeleteIt)
			DestroyImmediate(overlappedParent.gameObject);


		return excludedCount;
	}


	/// <summary>
	/// Enable all 2D colliders that are borders and disable all 2D colliders
	/// that are sorrounded by other colliders. Returns the count of disabled
	/// colliders.
	/// </summary>
	public int TurnOffUnneededColliders2D()
	{
		// Children
		Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
		childrenCount = colliders.Length;


		// Only if there is something in the list
		if (colliders.Length < 1)
			return 0;


		// First, reactivate all
		for (int i = 0; i < childrenCount; i++)
			colliders[i].enabled = true;


		// Assuming all colliders are squares and they have the same size
		float rayLength = colliders[1].bounds.extents.x * 1.1f;

		// Assuming we all have the same layer
		layer = gameObject.layer;

		// Collect the unneeded colliders by checking around them
		List<Collider2D> unneededColliders = new List<Collider2D>();
		for (int i = 0; i < childrenCount; i++)
		{
			// Test for adjacent colliders ignoring itself
			int sorroundedCount = 0;
			Vector3 pos = colliders[i].transform.position;

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


			// Colliders that are completely sorrounded
			if (sorroundedCount == 4)
				colliders[i].enabled = false;
		}


		// Turn off unneeded
		for (int i = 0; i < unneededColliders.Count; i++)
			unneededColliders[i].enabled = false;


		return unneededColliders.Count;
	}


	/// <summary>
	/// Rename all children, numerically, correctly padded.
	/// </summary>
	public int RenameChildren()
	{
		children =  GetComponentsInChildren<Transform>();
		childrenCount = children.Length;

		int count = 0;
		int leftPad = childrenCount.ToString().Length;

		for (int i = 0; i < childrenCount; i++)
		{
			// Ignore self
			if (children[i] == transform)
				continue;

			// Rename
			children[i].name = i.ToString().PadLeft(leftPad, '0');
			count += 1;
		}

		return count;
	}


	// ~
	// MENU ITEMS

	// ~
	// ALT + S
	[MenuItem("Tools/GhostGrid/Snap Once &s")]
	private static void SnapSelectedGrid()
	{
		GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

		if (grid != null)
		{
			grid.doAutoSnap = false;
			grid.SnapAll();

			Debug.Log("GhostGrid :: Grid snapped!");
		}
		else
		{
			Debug.Log(NOT_FOUND);
		}
	}

	// Disable the previous menu item if no Transform is selected.
	[MenuItem("Tools/GhostGrid/Snap Once &s", true)]
	private static bool ValidateSnapSelectedGrid()
	{
		return Selection.activeTransform != null;
	}


	// ~
	// ALT + A
	[MenuItem("Tools/GhostGrid/Enable Auto Snap &a")]
	private static void EnableGridAutoSnap()
	{
		GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

		if (grid != null)
		{
			grid.SnapAll();
			grid.doAutoSnap = true;

			Debug.Log("GhostGrid :: Auto Snap enabled on selected grid!");
		}
		else
		{
			Debug.Log(NOT_FOUND);
		}
	}

	// Disable the previous menu item if no Transform is selected.
	[MenuItem("Tools/GhostGrid/Enable Auto Snap &a", true)]
	private static bool ValidateEnableGridAutoSnap()
	{
		return Selection.activeTransform != null;
	}


	// ~
	// ALT + D
	[MenuItem("Tools/GhostGrid/Disable Auto Snap On All Grids &d")]
	private static void DisableAllGrids()
	{
		Debug.Log("GhostGrid :: Auto Snap disabled on all grids.");

		if (others == null)
			return;

		for (int i = 0; i < others.Count; i++)
			others[i].doAutoSnap = false;
	}


	// ~
	// ALT + F
	[MenuItem("Tools/GhostGrid/Apply Current Optimizations &f")]
	private static void MenuApplyCurrentOptimizations()
	{
		GhostGrid grid = Selection.activeTransform.GetComponentInParent<GhostGrid>();

		if (grid != null)
		{
			int howMany = 0;
			List<string> messages = new List<string>();


			if (grid.doCleanOverlappedChildren)
			{
				howMany = grid.ExcludeOverlappedChildren(true);
				messages.Add(howMany + " overlapped deleted");
			}

			if (grid.doTurnOffUnneededColliders2D)
			{
				howMany = grid.TurnOffUnneededColliders2D();
				messages.Add(howMany + " unneeded 2D colliders were turned off");
			}

			if (grid.doRename)
			{
				howMany = grid.RenameChildren();
				messages.Add(howMany + " renamed");
			}


			// Messages
			string message = "";
			for (int i = 0; i < messages.Count; i++)
				message += messages[i] + (i + 1 < len ? ", " : ".");

			message = message.Length > 1 ? message : "Nothing to do. Check the current options on the selected GhostGrid.";
			Debug.Log("GhostGrid :: " + message);
		}
		else
		{
			Debug.Log(NOT_FOUND);
		}
	}

	// Disable the previous menu item if no Transform is selected.
	[MenuItem("Tools/GhostGrid/Apply Current Optimizations &f", true)]
	private static bool ValidateMenuApplyCurrentOptimizations()
	{
		return Selection.activeTransform != null;
	}
}
#endif
