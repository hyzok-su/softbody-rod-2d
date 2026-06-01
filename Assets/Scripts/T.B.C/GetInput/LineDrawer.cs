using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LineDrawer2D : MonoBehaviour
{
    [Header("Setup")]
    public Camera cam;                  // Assign your Main Camera
    public Material lineMaterial;       // Assign a material (e.g., Sprites/Default)
    public float lineWidth = 0.1f;
    public float snapDistance = 2.0f;

    
    private bool canDraw = false;       // Enabled by button
    private bool isDrawing = false;
    private Vector2 startPoint2D;
    private Vector2 endPoint2D;
    private LineRenderer currentLine;

    private List<GameObject> objBuffer = new List<GameObject>();

    // Auto-enable drawing if last line was zero-length
    private bool autoEnableAfterZeroLength = false;


    private List<Vector2[]> lineBuffer = new List<Vector2[]>();
    private List<bool[]> isAnchored = new List<bool[]>();



    [Header("GridSnap")]
    /*-------------------- input ---------------------------*/

    public GridCreator gridCreator;
    private Vector2[] nodes;
    private bool[] anchors;

    /*-------------------------------------------------------*/


    /*-------------------- output ---------------------------*/

    public List<Vector2[]> LineBuffer => lineBuffer;
    public List<bool[]> IsAnchored => isAnchored;

    /*-------------------------------------------------------*/

    void Awake()
    {
        // If camera not assigned, use main camera
        if (cam == null)
        {
            cam = Camera.main;
        }
        if (gridCreator != null)
        {
            nodes = gridCreator.Nodes;
            anchors = gridCreator.IsAnchored;
        }
    }

    void Update()
    {
        // Re-enable drawing automatically after zero-length line
        if (autoEnableAfterZeroLength)
        {
            canDraw = true;
            autoEnableAfterZeroLength = false;
        }

        if (!canDraw || gridCreator == null || gridCreator.Nodes == null)
            return;

        // Start drawing
        bool start = false;
        bool end = false;
        if (Mouse.current.leftButton.wasPressedThisFrame && !isDrawing)
        {
            isDrawing = true;
            startPoint2D = SnapToGrid(GetMouseWorldPos2D(),out start);
            endPoint2D = startPoint2D;

            // Create LineRenderer
            GameObject lineObj = new GameObject("Line_" + lineBuffer.Count);
            lineObj.transform.parent = transform;

            currentLine = lineObj.AddComponent<LineRenderer>();
            currentLine.positionCount = 2;
            currentLine.material = lineMaterial;
            currentLine.textureMode = LineTextureMode.Tile;
            currentLine.startWidth = lineWidth;
            currentLine.endWidth = lineWidth;
            currentLine.startColor = Color.white;
            currentLine.endColor = Color.white;
            //currentLine.numCapVertices = 4;
            //currentLine.numCornerVertices = 4;

            currentLine.SetPosition(0, new Vector3(startPoint2D.x, startPoint2D.y, 0f));
            currentLine.SetPosition(1, new Vector3(endPoint2D.x, endPoint2D.y, 0f));

            objBuffer.Add(lineObj);
        }

        // Update line while dragging
        if (Mouse.current.leftButton.isPressed && isDrawing)
        {
            Vector2 mousePos = GetMouseWorldPos2D();

            // Find the nearest grid node
            bool temp;
            Vector2 closestNode = SnapToGrid(mousePos, out temp);

            // If the mouse is close enough to the grid node, snap to it
            if (Vector2.Distance(mousePos, closestNode) <= snapDistance)
            {
                endPoint2D = closestNode;
            }
            else
            {
                // Otherwise, follow the actual mouse position
                endPoint2D = mousePos;
            }

            // Update the LineRenderer
            if (currentLine != null)
                currentLine.SetPosition(1, new Vector3(endPoint2D.x, endPoint2D.y, 0f));
        }

        // Finish line
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDrawing)
        {
            endPoint2D = SnapToGrid(GetMouseWorldPos2D(), out end);

            Vector2[] line = new Vector2[] { startPoint2D, endPoint2D };

            var dist = Vector2.Distance(startPoint2D, endPoint2D);

            if (dist > 0 && dist < 2 * gridCreator.sideLength - 0.001 && !lineBuffer.Exists(x =>
            (x[0] == startPoint2D && x[1] == endPoint2D)
            ||
            (x[1] == startPoint2D && x[0] == endPoint2D)))
            {
                // Valid line
                if (currentLine != null)
                    currentLine.SetPosition(1, new Vector3(endPoint2D.x, endPoint2D.y, 0f));

                lineBuffer.Add(line);
                isAnchored.Add(new bool[] { start, end });
                canDraw = false; // normal case: disable drawing until button pressed
            }
            else
            {
                // Zero-length line: destroy and re-enable drawing automatically
                if (currentLine != null)
                    Destroy(currentLine.gameObject);

                autoEnableAfterZeroLength = true;
            }

            currentLine = null;
            isDrawing = false;
        }
    }

    // Snap a point to the nearest grid node
    Vector2 SnapToGrid(Vector2 point, out bool anchored)
    {
        Vector2 closest = gridCreator.Nodes[0];
        float minDist = Vector2.Distance(point, closest);
        int minIndex = 0;
        for (int i = 1; i < nodes.Length; i++)
        {
            float dist = Vector2.Distance(point, nodes[i]);
            if (dist < minDist)
            {
                minDist = dist;
                closest = nodes[i];
                minIndex = i;
            }
        }
        anchored = anchors[minIndex];
        return closest;
    }

    // Convert mouse position to world position
    Vector2 GetMouseWorldPos2D()
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = -cam.transform.position.z; // distance from camera
        Vector3 worldPos = cam.ScreenToWorldPoint(mousePos);
        return new Vector2(worldPos.x, worldPos.y);
    }

    // Enable drawing from UI button or other script
    public void EnableDrawing()
    {
        canDraw = true;
    }

    public void HideAllObj()
    {
        foreach (var obj in objBuffer)
        {
            if (obj != null)
                obj.SetActive(false); // hide the whole GameObject
        }
    }

    public void PreviewAllObj()
    {
        foreach (var obj in objBuffer)
        {
            if (obj != null)
                obj.SetActive(true); // hide the whole GameObject
        }
    }
}
