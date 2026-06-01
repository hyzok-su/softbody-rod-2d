using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class AnchorNodeCollector : MonoBehaviour
{
    public float radius = 1.2f;
    private List<Vector2> nodes = new List<Vector2>();
    public List<Vector2> Nodes => nodes;

    // Editor: auto-update
#if UNITY_EDITOR
    void Update()
    {
        nodes.Clear();

        foreach (Transform child in transform)
        {
            nodes.Add((Vector2)child.position);
        }

        nodes.Sort((a, b) => a.y.CompareTo(b.y));
    }
#endif

    // Runtime: collect only once
    void Awake()
    {
        nodes.Clear();

        foreach (Transform child in transform)
        {
            nodes.Add((Vector2)child.position);
        }

        nodes.Sort((a, b) => a.y.CompareTo(b.y));
    }
}
