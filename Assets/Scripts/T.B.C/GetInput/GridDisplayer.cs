using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class EditModeTriangularGrid : MonoBehaviour
{

    // assign in Inspector
    [Header("Reference")]
    public GridCreator gridScript;
    public AnchorNodeCollector anchorScript;

    // Private properties to store values
    private int width { get; set; }
    private int height { get; set; }
    private float sideLength { get; set; }
    private Vector2 gridOrigin { get; set; }
    private float scale { get; set; }

    private List<Vector2> anchors = new List<Vector2>();
    private float radius { get; set; }

    void Update()
    {
        if (gridScript != null)
        {
            // Retrieve values from GridGenerator
            width = gridScript.width;
            height = gridScript.height;
            sideLength = gridScript.sideLength;
            gridOrigin = gridScript.gridOrigin;
            scale = gridScript.scale;

        }
        if (anchorScript != null)
        {
            if (anchorScript.Nodes != null)
            {
                anchors = anchorScript.Nodes;
            }
            radius = anchorScript.radius;
        }
    }

    private void OnDrawGizmos()
    {
        if (width <= 0 || height <= 0) return;

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
                Vector3 pos = new Vector3(gridOrigin.x + x * sideLength + offsetX, rowY, 0f);

                bool anchored = false;

                // Only check anchors within Y-range
                for (int i = anchorIndexStart; i < anchorIndexEnd; i++)
                {
                    Vector3 anchor = anchors[i];

                    // Optional X-range check (skip anchors too far horizontally)
                    if (Mathf.Abs(anchor.x - pos.x) > radius)
                        continue;

                    if ((pos - anchor).sqrMagnitude < radiusSqr)
                    {
                        anchored = true;
                        break;
                    }
                }

                // Draw sphere
                Gizmos.color = anchored ? new Color(1f, 0f, 0f, 0.5f) : new Color(1f, 1f, 1f, 0.3f);
                Gizmos.DrawSphere(pos, anchored ? scale * 0.8f : scale);
            }
        }


        Gizmos.color = new Color(1f, 1f, 1f, 0.2f);
        for (int i = 0; i < anchors.Count; i++)
        {
            Vector3 anchor = anchors[i];
            DrawGizmoCircle(anchor, radius);
        }
    }

    private void DrawGizmoCircle(Vector3 center, float radius, int segments = 12)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }
}
