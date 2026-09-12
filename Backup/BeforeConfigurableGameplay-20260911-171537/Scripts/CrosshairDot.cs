using UnityEngine;
using UnityEngine.UI;

// A screen-space dot shared by all weapons, including empty hands.
public class CrosshairDot : MaskableGraphic
{
    public static void Create(Transform owner)
    {
        var hud = new GameObject("Crosshair", typeof(RectTransform), typeof(Canvas));
        hud.transform.SetParent(owner, false);
        var canvas = hud.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var dot = new GameObject("Center Dot", typeof(RectTransform), typeof(CanvasRenderer), typeof(CrosshairDot));
        dot.transform.SetParent(hud.transform, false);
        var graphic = dot.GetComponent<CrosshairDot>();
        graphic.raycastTarget = false;
        graphic.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        graphic.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        graphic.rectTransform.pivot = new Vector2(0.5f, 0.5f);
        graphic.rectTransform.anchoredPosition = Vector2.zero;
        graphic.rectTransform.sizeDelta = new Vector2(6f, 6f);
    }

    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        // A thin dark border keeps the white dot visible against bright surfaces.
        AddCircle(mesh, 3f, new Color32(0, 0, 0, 180));
        AddCircle(mesh, 2f, new Color32(255, 255, 255, 255));
    }

    static void AddCircle(VertexHelper mesh, float radius, Color32 tint)
    {
        const int segments = 24;
        int center = mesh.currentVertCount;
        mesh.AddVert(Vector3.zero, tint, Vector2.zero);
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            mesh.AddVert(new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * radius, tint, Vector2.zero);
        }
        for (int i = 0; i < segments; i++)
            mesh.AddTriangle(center, center + 1 + i, center + 1 + (i + 1) % segments);
    }
}
