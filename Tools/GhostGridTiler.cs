
// Experimental GhostGrid tool that tiles everything!

// @matnesis
// 2016/08/09 11:59 PM


using UnityEngine;
using UnityEngine.SceneManagement;

public class GhostGridTiler : MonoBehaviour
{
    public int childrenCount = 0;

    [Header("Config")]
    public Transform target;
    public LayerMask layer;
    public float zLayer = -5;
    public float snapToGrid = 3.99f;

    [Header("Tiles")]
    public Transform[] upLeft;
    public Transform[] upMiddle;
    public Transform[] upRight;
    public Transform[] center;
    public Transform[] down;
    public Transform[] alone;

    private Collider2D[] colliders;


    [ContextMenu("TILE UP!")]
    public int TileAll()
    {
        // Wake up
        target.gameObject.SetActive(true);

        // Children
        colliders = target.GetComponentsInChildren<Collider2D>();
        childrenCount = colliders.Length;


        // Only if there is something in the list
        if (colliders.Length < 1)
            return 0;


        // The parent that will hold them
        int currentScene = SceneManager.GetActiveScene().buildIndex;
        string tilesName = target.name + "@GhostTiles" + currentScene;

        Transform parent = GhostGrid.GetOrCreateTransform(tilesName);
        DestroyImmediate(parent.gameObject); // Overwrite

        parent = GhostGrid.GetOrCreateTransform(tilesName);
        parent.gameObject.layer = target.gameObject.layer;


        // First, reactivate all, we need to calculate over this
        for (int i = 0; i < childrenCount; i++)
            colliders[i].enabled = true;


        // Assuming all colliders are squares and they have the same size
        float rayLength = colliders[1].bounds.extents.x * 1.2f;

        // Assuming we all have the same layer
        if (layer == 0) layer = 1 << target.gameObject.layer;

        // Analyze which kind of tile should be created
        for (int i = 0; i < childrenCount; i++)
        {
            // Test for adjacent colliders
            Vector4 surrounded = new Vector4();
            Vector3 pos = colliders[i].transform.position;


            pos.z = Random.Range(-0.2f, 0.2f); // Z fix to avoid tiles sharing the same space
            pos = GhostGrid.GetSnapVector(pos, snapToGrid); // Snap!


            colliders[i].enabled = false;

            // Saving the collision data
            if (Physics2D.Raycast(pos, Vector2.up, rayLength, layer))
                surrounded.x = 1;

            if (Physics2D.Raycast(pos, Vector2.right, rayLength, layer))
                surrounded.y = 1;

            if (Physics2D.Raycast(pos, Vector2.down, rayLength, layer))
                surrounded.z = 1;

            if (Physics2D.Raycast(pos, Vector2.left, rayLength, layer))
                surrounded.w = 1;

            colliders[i].enabled = true;


            // Analyze and create the tile!
            Transform tile = null;

            // Up left
            if (surrounded.x == 0 && surrounded.y == 1 && surrounded.w == 0)
                tile = Instantiate(upLeft[Random.Range(0, upLeft.Length)], pos, Quaternion.identity) as Transform;

            // Up middle
            else if (surrounded.x == 0 && surrounded.y == 1 && surrounded.w == 1)
                tile = Instantiate(upMiddle[Random.Range(0, upMiddle.Length)], pos, Quaternion.identity) as Transform;

            // Up right
            else if (surrounded.x == 0 && surrounded.y == 0 && surrounded.w == 1)
                tile = Instantiate(upRight[Random.Range(0, upRight.Length)], pos, Quaternion.identity) as Transform;

            // Up, with something below
            else if (surrounded.x == 0 && surrounded.y == 0 && surrounded.z == 1 && surrounded.w == 0)
                tile = Instantiate(upMiddle[Random.Range(0, upMiddle.Length)], pos, Quaternion.identity) as Transform;

            // Alone
            else if (surrounded.x == 0 && surrounded.y == 0 && surrounded.z == 0 && surrounded.w == 0)
                tile = Instantiate(alone[Random.Range(0, alone.Length)], pos, Quaternion.identity) as Transform;

            // Center
            else if (surrounded.x == 1 && surrounded.z == 1)
                tile = Instantiate(center[Random.Range(0, center.Length)], pos, Quaternion.identity) as Transform;

            // Down
            else if (surrounded.x == 1)
                tile = Instantiate(down[Random.Range(0, down.Length)], pos, Quaternion.identity) as Transform;

            // Config
            if (tile != null)
            {
                tile.name = (i + "").PadLeft(4);
                tile.parent = parent;
            }
        }


        // We don't need the original grid
        target.gameObject.SetActive(false);


        // A GhostGrid will be useful on the generated grid
        GhostGrid grid = parent.gameObject.AddComponent<GhostGrid>();
        grid.layer = layer;
        grid.gridSize = snapToGrid;


        // Z layer
        {
            Vector3 pos = parent.position;
            pos.z = zLayer;
            parent.position = pos;
        }


        return childrenCount;
    }
}
