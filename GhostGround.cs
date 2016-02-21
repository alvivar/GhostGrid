
// Experimental dynamic GameObject boxel ground over a GhostGrid (Uh!)

// Andrés Villalobos ~ @matnesis ~ andresalvivar@gmail.com
// 2016/01/03 09:10 PM


using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class GhostGround : MonoBehaviour
{
	[Header("Grid config")]
	public float gridSize = 1;
	public LayerMask layer;
	public float OverlapSphereRatio = 0.001f;

	[Header("Data")]
	public List<Vector3> groundPositions;
	public List<Transform> groundElements;
	public List<Transform> corners;

	[Header("Sides")]
	public List<Transform> xSideUp;
	public List<Transform> xSideDown;
	public List<Transform> ySideUp;
	public List<Transform> ySideDown;
	public List<Transform> zSideUp;
	public List<Transform> zSideDown;


	private static GhostGround instance;
	public static GhostGround g
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<GhostGround>();
				if (instance == null)
				{
					GameObject go = new GameObject("[GhostGround]");
					instance = go.AddComponent<GhostGround>();
				}
			}

			return instance;
		}
	}


	public void SetupGrid(List<Vector3> groundPositions, float gridSize, LayerMask layer)
	{
		this.groundPositions = groundPositions;
		this.gridSize = gridSize;
		this.layer = layer;
	}


	public void CollectElements()
	{
		// All ground elements
		groundElements = new List<Transform>();

		for (int i = 0; i < groundPositions.Count; i++)
		{
			// There is something there?
			Collider[] colliders = Physics.OverlapSphere(groundPositions[i], OverlapSphereRatio, layer);
			for (int j = 0; j < colliders.Length; j++)
			{
				if (!groundElements.Contains(colliders[0].transform))
					groundElements.Add(colliders[0].transform);
			}
		}


		// We need at least one element for the next calculations
		if (groundElements.Count < 1)
			return;


		// Corners
		Transform min = groundElements.Aggregate((a, n) => a.position.x < n.position.x || a.position.y < n.position.y || a.position.z < n.position.z ? a : n);
		Transform max = groundElements.Aggregate((a, n) => a.position.x > n.position.x || a.position.y > n.position.y || a.position.z > n.position.z ? a : n);

		corners = new List<Transform>();
		corners.Add(min);
		corners.Add(max);


		// Sides

		// X
		float xMax = groundElements.Aggregate((agg, next) => agg.position.x > next.position.x ? agg : next).position.x;
		xSideUp = groundElements.Where(x => x.position.x == xMax).ToList();

		float xMin = groundElements.Aggregate((agg, next) => agg.position.x < next.position.x ? agg : next).position.x;
		xSideDown = groundElements.Where(x => x.position.x == xMin).ToList();


		// Y
		float yMax = groundElements.Aggregate((agg, next) => agg.position.y > next.position.y ? agg : next).position.y;
		ySideUp = groundElements.Where(x => x.position.y == yMax).ToList();

		float yMin = groundElements.Aggregate((agg, next) => agg.position.y < next.position.y ? agg : next).position.y;
		ySideDown = groundElements.Where(x => x.position.y == yMin).ToList();


		// Z
		float zMax = groundElements.Aggregate((agg, next) => agg.position.z > next.position.z ? agg : next).position.z;
		zSideUp = groundElements.Where(x => x.position.z == zMax).ToList();

		float zMin = groundElements.Aggregate((agg, next) => agg.position.z < next.position.z ? agg : next).position.z;
		zSideDown = groundElements.Where(x => x.position.z == zMin).ToList();
	}


	/// <summary>
	/// Adds a position to the virtual ground.
	/// </summary>
	public void AddGroundPosition(Vector3 position)
	{
		if (!groundPositions.Contains(position))
			groundPositions.Add(position);
	}


	/// <summary>
	/// Creates an element through a delegate.
	/// </summary>
	private void InstantiateElement(Vector3 position, Func<Transform> onInstantiate)//, bool validateOverlap = true)
	{
		if (onInstantiate == null)
			return;


		// if (validateOverlap)
		// {
		Collider[] colliders = Physics.OverlapSphere(position, OverlapSphereRatio, layer);
		if (colliders.Length < 1)
		{
			Transform t = onInstantiate();
			t.position = position;
			t.SetParent(transform);
		}
		// }
		// else
		// {
		// 	Transform t = onInstantiate();
		// 	t.position = position;
		// 	t.SetParent(transform);
		// }
	}


	/// <summary>
	/// Destroy an element through a delegate.
	/// </summary>
	private void DestroyElement(Transform element, Action<Transform> onDestroy)
	{
		if (onDestroy != null)
			onDestroy(element);
	}


	/// <summary>
	/// Destroy all elements detected on the virtual ground.
	/// </summary>
	public void DestroyAllElements(Action<Transform> onDestroy)
	{
		for (int i = 0; i < groundPositions.Count; i++)
		{
			// There is something there?
			Collider[] colliders = Physics.OverlapSphere(groundPositions[i], OverlapSphereRatio, layer);
			for (int j = 0; j < colliders.Length; j++)
				DestroyElement(colliders[0].transform, onDestroy);
		}
	}


	/// <summary>
	/// Fills with elements all the virtual ground.
	/// </summary>
	public void Fill(Func<Transform> onInstantiate)
	{
		for (int i = 0; i < groundPositions.Count; i++)
			InstantiateElement(GhostGrid.GetSnapVector(groundPositions[i], gridSize), onInstantiate);
	}


	/// <summary>
	/// Grows the virtual grid by creating one layer of elements at the direction.
	/// </summary>
	public void Grow(Vector3 direction, Func<Transform> onInstantiate)
	{
		// Extract the positions that can grow
		List<Transform> toGrow = groundElements;
		var canBeGrown = new List<Vector3>();
		for (int i = 0; i < toGrow.Count; i++)
		{
			if (toGrow[i] == null)
				continue;

			Vector3 currentPosition = toGrow[i].position;
			Vector3 growPosition = currentPosition + direction * gridSize;

			// Grow at direction
			Vector3 newPosition = GhostGrid.GetSnapVector(growPosition, gridSize);

			// Extract if there is nothing there
			Collider[] colliders = Physics.OverlapSphere(newPosition, OverlapSphereRatio, layer);
			if (colliders.Length < 1)
				canBeGrown.Add(newPosition);

			// Add them as virtual ground
			if (!groundPositions.Contains(newPosition))
				groundPositions.Add(newPosition);
		}


		// Required
		if (canBeGrown.Count < 1)
			return;

		// Filter the lower bottom
		if (direction.x > 0)
		{
			float xMin = canBeGrown.Aggregate((agg, next) => agg.x < next.x ? agg : next).x;
			canBeGrown = canBeGrown.Where(x => x.x == xMin).ToList();
		}
		else if (direction.x < 0)
		{
			float xMax = canBeGrown.Aggregate((agg, next) => agg.x > next.x ? agg : next).x;
			canBeGrown = canBeGrown.Where(x => x.x == xMax).ToList();
		}
		else if (direction.y > 0)
		{
			float yMin = canBeGrown.Aggregate((agg, next) => agg.y < next.y ? agg : next).y;
			canBeGrown = canBeGrown.Where(x => x.y == yMin).ToList();
		}
		else if (direction.y < 0)
		{
			float yMax = canBeGrown.Aggregate((agg, next) => agg.y > next.y ? agg : next).y;
			canBeGrown = canBeGrown.Where(x => x.y == yMax).ToList();
		}
		else if (direction.z > 0)
		{
			float zMin = canBeGrown.Aggregate((agg, next) => agg.z < next.z ? agg : next).z;
			canBeGrown = canBeGrown.Where(x => x.z == zMin).ToList();
		}
		else if (direction.z < 0)
		{
			float zMax = canBeGrown.Aggregate((agg, next) => agg.z > next.z ? agg : next).z;
			canBeGrown = canBeGrown.Where(x => x.z == zMax).ToList();
		}


		// Grow them
		for (int i = 0; i < canBeGrown.Count; i++)
			InstantiateElement(canBeGrown[i], onInstantiate);
	}


	/// <summary>
	/// Reduces the virtual ground by eliminating one layer of elements at the direction.
	/// </summary>
	public void Reduce(Vector3 direction, Action<Transform> onDestroy)
	{
		// Direction meaning
		List<Transform> toReduce = new List<Transform>();
		// List<Transform> toReduce = groundElements;

		// Option 1
		if (direction.x > 0) toReduce = xSideUp;
		else if (direction.x < 0) toReduce = xSideDown;
		else if (direction.y > 0) toReduce = ySideUp;
		else if (direction.y < 0) toReduce = ySideDown;
		else if (direction.z > 0) toReduce = zSideUp;
		else if (direction.z < 0) toReduce = zSideDown;


		// Reduce
		List<GameObject> toDestroy = new List<GameObject>();
		for (int i = 0; i < toReduce.Count; i++)
		{
			if (toReduce[i] == null)
				continue;

			// There is something there?
			Collider[] colliders = Physics.OverlapSphere(toReduce[i].position, OverlapSphereRatio, layer);
			for (int j = 0; j < colliders.Length; j++)
			{
				Vector3 currentPosition = colliders[0].transform.position;

				// Add the position behind the removed to allow extrusion
				Vector3 behind = GhostGrid.GetSnapVector(currentPosition + -1 * direction * gridSize, gridSize);
				if (!groundPositions.Contains(behind))
					groundPositions.Add(behind);

				// To delete
				toDestroy.Add(colliders[0].gameObject);
			}
		}


		// Destroy at the end
		for (int i = 0; i < toDestroy.Count; i++)
			DestroyElement(toDestroy[i].transform, onDestroy);
	}


	/// <summary>
	/// Change the material in all ground elements.
	/// </summary>
	public void SetMaterial(Material material)
	{
		if (groundElements == null)
			return;

		for (int i = 0; i < groundElements.Count; i++)
		{
			if (groundElements[i] != null)
				groundElements[i].GetComponent<Renderer>().material = material;
		}
	}
}
