using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridCreator : MonoBehaviour
{
    [Header("Reference")]
    public AnchorNodeCollector anchorScript;

    [Header("Grid Settings")]
    public int width = 100;
    public int height = 20;
    public float sideLength = 2f;
    public Vector2 gridOrigin = Vector2.zero;

    [Header("Rendering")]
    public Sprite spriteNode; 
    public Material materialNode;

    public Sprite spriteAnchor;
    public Material materialAnchor;

    public float scale = 2f;

    private Vector2[] nodes;
    private bool[] isAnchored;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool isVisible = false;

    /*-------------------- output ---------------------------*/

    public Vector2[] Nodes => nodes;
    public bool[] IsAnchored => isAnchored;

    /*-------------------------------------------------------*/

    void Awake()
    {
        List<Vector2> anchors = new List<Vector2>();
        float radius = 0;

        if (anchorScript != null)
        {
            anchors = anchorScript.Nodes; // Sorted by Y
            radius = anchorScript.radius;
        }

        if (width <= 0 || height <= 0) return;

        nodes = new Vector2[width * height];
        isAnchored = new bool[width * height];

        float radiusSqr = radius * radius;
        float rowHeight = sideLength * Mathf.Sqrt(3) / 2f;

        int anchorIndexStart = 0; // start anchor index for current row
        for (int y = 0; y < height; y++)
        {
            float rowY = gridOrigin.y + y * rowHeight;

            // Move start to first anchor in range
            while (anchorIndexStart < anchors.Count && anchors[anchorIndexStart].y < rowY - radius)
                anchorIndexStart++;

            // Move end to first anchor above the row
            int anchorIndexEnd = anchorIndexStart;
            while (anchorIndexEnd < anchors.Count && anchors[anchorIndexEnd].y <= rowY + radius)
                anchorIndexEnd++;

            float offsetX = (y % 2 == 0) ? 0f : sideLength / 2f;

            for (int x = 0; x < width; x++)
            {
                int nodeIndex = y * width + x;
                Vector2 pos = new Vector3(gridOrigin.x + x * sideLength + offsetX, rowY);
                nodes[nodeIndex] = pos;

                // Only check anchors within Y-range
                for (int i = anchorIndexStart; i < anchorIndexEnd; i++)
                {
                    Vector2 anchor = anchors[i];

                    // Optional X-range check (skip anchors too far horizontally)
                    if (Mathf.Abs(anchor.x - pos.x) > radius)
                        continue;

                    if ((pos - anchor).sqrMagnitude < radiusSqr)
                    {
                        isAnchored[nodeIndex] = true;
                        break;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleGrid();
        }
    }

    void ToggleGrid()
    {
        if (isVisible)
            ClearGrid();
        else
            SpawnSprites();

        isVisible = !isVisible;
    }

    void SpawnSprites()
    {
        if (nodes == null) return;
        for (int i = 0; i < nodes.Length; i++)
        {
            GameObject obj = new GameObject("GridNode");
            Vector2 pos = nodes[i];

            obj.transform.position = pos;
            obj.transform.parent = transform;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            if (isAnchored[i])
            {
                obj.transform.localScale = Vector3.one * scale * 4f;
                sr.sprite = spriteAnchor;
                sr.material = materialAnchor;
            }
            else
            {
                obj.transform.localScale = Vector3.one * scale;
                sr.sprite = spriteNode;
                sr.material = materialNode;
            }
            spawnedObjects.Add(obj);
        }
    }

    void ClearGrid()
    {
        foreach (var obj in spawnedObjects)
        {
            Destroy(obj);
        }

        spawnedObjects.Clear();
    }
}
