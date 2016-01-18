
// Experimental dynamic GameObject boxel ground over a GhostGrid.

// Andrés Villalobos ~> @matnesis ~> andresalvivar@gmail.com
// 2016/01/03 09:10 PM


using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GhostGround : MonoBehaviour
{
	[Header("Config")]
	public float gridSize = 1;
	public LayerMask layer;

	[Header("Data")]
	[SerializeField] private List<Vector3> groundPositions;
	[SerializeField] private List<Transform> groundElements;
	[SerializeField] private List<Transform> corners;
	[SerializeField] private List<Transform> xSideUp;
	[SerializeField] private List<Transform> xSideDown;
	[SerializeField] private List<Transform> ySideUp;
	[SerializeField] private List<Transform> ySideDown;
	[SerializeField] private List<Transform> zSideUp;
	[SerializeField] private List<Transform> zSideDown;


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


	public void Setup(List<Vector3> groundPositions, float gridSize, LayerMask layer)
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
			Collider[] colliders = Physics.OverlapSphere(groundPositions[i], 0.1f, layer);
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
	public void AddPosition(Vector3 position)
	{
		if (!groundPositions.Contains(position))
			groundPositions.Add(position);
	}


	/// <summary>
	/// Creates an element in the position.
	/// </summary>
	public void PutElement(Transform element, Vector3 position)
	{
		Collider[] colliders = Physics.OverlapSphere(position, 0.1f, layer);
		if (colliders.Length < 1)
			Instantiate(element, position, Quaternion.identity);
	}


	/// <summary>
	/// Destroy all elements detected on the virtual ground.
	/// </summary>
	public void DestroyAllElements()
	{
		for (int i = 0; i < groundPositions.Count; i++)
		{
			// There is something there?
			Collider[] colliders = Physics.OverlapSphere(groundPositions[i], 0.1f, layer);
			for (int j = 0; j < colliders.Length; j++)
				Destroy(colliders[0].gameObject);
		}
	}


	/// <summary>
	/// Fills with elements all the virtual ground.
	/// </summary>
	public void Fill(Transform element)
	{
		for (int i = 0; i < groundPositions.Count; i++)
			PutElement(element, GhostGrid.GetSnapVector(groundPositions[i], gridSize));
	}


	/// <summary>
	/// Grows the virtual grid by creating one layer of elements at the direction.
	/// </summary>
	public void Grow(Transform element, Vector3 direction)
	{
		// Refresh
		CollectElements();


		// Direction meaning
		// List<Transform> toGrow = new List<Transform>();
		List<Transform> toGrow = groundElements;

		// if (direction.x > 0) toGrow = xSideUp;
		// else if (direction.x < 0) toGrow = xSideDown;
		// else if (direction.y > 0) toGrow = ySideUp;
		// else if (direction.y < 0) toGrow = ySideDown;
		// else if (direction.z > 0) toGrow = zSideUp;
		// else if (direction.z < 0) toGrow = zSideDown;

		// if (direction.x != 0)
		// {
		// 	toGrow.AddRange(xSideUp);
		// 	toGrow.AddRange(xSideDown);
		// }
		// else if (direction.y != 0)
		// {
		// 	toGrow.AddRange(ySideUp);
		// 	toGrow.AddRange(ySideDown);
		// }
		// else if (direction.z != 0)
		// {
		// 	toGrow.AddRange(zSideUp);
		// 	toGrow.AddRange(zSideDown);
		// }


		// Grow
		for (int i = 0; i < toGrow.Count; i++)
		{
			if (toGrow[i] == null)
				continue;

			Vector3 currentPosition = toGrow[i].position;
			Vector3 growPosition = currentPosition + direction * gridSize;

			// Grow at direction
			Vector3 newPosition = GhostGrid.GetSnapVector(growPosition, gridSize);
			PutElement(element, newPosition);

			// Also as virtual ground
			if (!groundPositions.Contains(newPosition))
				groundPositions.Add(newPosition);
		}


		// Refresh
		CollectElements();
	}


	/// <summary>
	/// Reduces the virtual grid by eliminating one layer of elements at the direction.
	/// </summary>
	public void Reduce(Vector3 direction)
	{
		// Refresh
		CollectElements();


		// Direction meaning
		List<Transform> toReduce = new List<Transform>();
		// List<Transform> toReduce = groundElements;

		if (direction.x > 0) toReduce = xSideUp;
		else if (direction.x < 0) toReduce = xSideDown;
		else if (direction.y > 0) toReduce = ySideUp;
		else if (direction.y < 0) toReduce = ySideDown;
		else if (direction.z > 0) toReduce = zSideUp;
		else if (direction.z < 0) toReduce = zSideDown;


		// if (direction.x != 0)
		// {
		// 	toReduce.AddRange(xSideUp);
		// 	toReduce.AddRange(xSideDown);
		// }
		// else if (direction.y != 0)
		// {
		// 	toReduce.AddRange(ySideUp);
		// 	toReduce.AddRange(ySideDown);
		// }
		// else if (direction.z != 0)
		// {
		// 	toReduce.AddRange(zSideUp);
		// 	toReduce.AddRange(zSideDown);
		// }


		// Reduce
		List<GameObject> toDestroy = new List<GameObject>();
		for (int i = 0; i < toReduce.Count; i++)
		{
			if (toReduce[i] == null)
				continue;

			// There is something there?
			Collider[] colliders = Physics.OverlapSphere(toReduce[i].position, 0.1f, layer);
			for (int j = 0; j < colliders.Length; j++)
			{
				Vector3 currentPosition = colliders[0].transform.position;

				// Remove current
				// groundPositions.Remove(currentPosition);

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
			Destroy(toDestroy[i]);
	}
}
