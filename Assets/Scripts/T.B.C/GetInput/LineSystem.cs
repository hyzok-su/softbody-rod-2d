using UnityEngine;

public class LineSystem : MonoBehaviour
{
    public void Preview()
    {
        foreach (Transform child in transform)
        {
            LineDrawer2D ld = child.GetComponent<LineDrawer2D>();
            ld.PreviewAllObj();
        }
    }
    public void Hide()
    {
        foreach (Transform child in transform)
        {
            LineDrawer2D ld = child.GetComponent<LineDrawer2D>();
            ld.HideAllObj();
        }
    }
}
