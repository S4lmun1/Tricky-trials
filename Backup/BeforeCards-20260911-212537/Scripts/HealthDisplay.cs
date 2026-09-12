using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reusable health display: screen HUD or a billboard above any health component.
[RequireComponent(typeof(DamageableTarget))]
public class HealthDisplay : MonoBehaviour
{
    public DamageableTarget source;
    public bool worldSpace;
    public Camera viewer;
    [Header("Layout")]
    public Vector2 size = new Vector2(216f, 64f);
    public Vector2 screenMargin = new Vector2(24f, 24f);
    public Vector3 worldOffset = new Vector3(0f, 2.55f, 0f);
    [Min(0.001f)] public float worldScale = 0.008f;
    public Vector2 referenceResolution = new Vector2(1920f, 1080f);
    public float padding = 12f;
    public float barHeight = 6f;
    public bool showText = true;
    public string label = "HEALTH";
    public float fontSize = 17f;
    public int sortingOrder = 20;
    [Header("Colors")]
    public Color panelColor = new Color(0.06f, 0.07f, 0.09f, 0.85f);
    public Color trackColor = new Color(1f, 1f, 1f, 0.15f);
    public Color healthyColor = new Color(0.35f, 0.85f, 0.66f, 1f);
    public Color lowHealthColor = new Color(0.95f, 0.28f, 0.24f, 1f);
    public Color textColor = Color.white;
    [Range(0f, 1f)] public float lowHealthThreshold = 0.25f;
    [Header("World Visibility")]
    public bool hideWhenDead = true;
    public bool hideBehindWalls = true;
    [Min(0f)] public float maximumDistance = 30f;
    public LayerMask visibilityLayers = ~0;
    Canvas canvas;
    RectTransform fill;
    Image fillImage;
    TextMeshProUGUI number;
    float lastHealth = float.NaN, lastMaximum = float.NaN;

    void Start()
    {
        if (source == null) source = GetComponent<DamageableTarget>();
        if (viewer == null) viewer = Camera.main;
        var root = new GameObject("Health Display", typeof(RectTransform), typeof(Canvas));
        root.transform.SetParent(transform, false);
        canvas = root.GetComponent<Canvas>();
        canvas.renderMode = worldSpace ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        canvas.worldCamera = viewer;
        var rootRect = root.GetComponent<RectTransform>();
        if (worldSpace)
        {
            rootRect.sizeDelta = size;
            rootRect.localScale = Vector3.one * worldScale;
        }
        else
        {
            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
        }
        var panel = MakeRect("Panel", root.transform);
        if (worldSpace)
        {
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = panel.offsetMax = Vector2.zero;
        }
        else
        {
            panel.anchorMin = panel.anchorMax = Vector2.zero;
            panel.pivot = Vector2.zero;
            panel.anchoredPosition = screenMargin;
            panel.sizeDelta = size;
        }
        AddImage(panel, panelColor);
        var track = MakeRect("Track", panel);
        track.anchorMin = Vector2.zero;
        track.anchorMax = new Vector2(1f, showText ? 0f : 1f);
        track.pivot = Vector2.zero;
        track.offsetMin = Vector2.one * padding;
        track.offsetMax = showText ? new Vector2(-padding, padding + barHeight) : -Vector2.one * padding;
        AddImage(track, trackColor);
        fill = MakeRect("Fill", track);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        fillImage = AddImage(fill, healthyColor);
        if (showText)
        {
            var textRect = MakeRect("Health Text", panel);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(padding, padding + barHeight);
            textRect.offsetMax = new Vector2(-padding, -padding);
            number = textRect.gameObject.AddComponent<TextMeshProUGUI>();
            number.fontSize = fontSize;
            number.color = textColor;
            number.alignment = TextAlignmentOptions.MidlineLeft;
            number.raycastTarget = false;
        }
        RefreshHealth();
    }

    static RectTransform MakeRect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }
    static Image AddImage(RectTransform rect, Color color)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }
    void RefreshHealth()
    {
        if (source == null || fill == null) return;
        float ratio = source.HealthFraction;
        fill.anchorMax = new Vector2(ratio, 1f);
        fillImage.color = ratio <= lowHealthThreshold ? lowHealthColor : healthyColor;
        if (number != null && (source.health != lastHealth || source.MaxHealth != lastMaximum))
        {
            number.text = label + "   " + Mathf.CeilToInt(source.health) + " / " + Mathf.CeilToInt(source.MaxHealth);
            lastHealth = source.health;
            lastMaximum = source.MaxHealth;
        }
    }
    void LateUpdate()
    {
        if (canvas == null || source == null) return;
        RefreshHealth();
        bool visible = !hideWhenDead || !source.IsDead;
        if (worldSpace)
        {
            if (viewer == null) viewer = Camera.main;
            if (viewer == null) { canvas.enabled = false; return; }
            Vector3 position = transform.position + worldOffset;
            canvas.transform.SetPositionAndRotation(position, viewer.transform.rotation);
            Vector3 delta = position - viewer.transform.position;
            visible &= delta.magnitude <= maximumDistance && Vector3.Dot(viewer.transform.forward, delta) > 0f;
            if (visible && hideBehindWalls)
                foreach (var hit in Physics.RaycastAll(viewer.transform.position, delta.normalized, delta.magnitude, visibilityLayers, QueryTriggerInteraction.Ignore))
                    if (!hit.transform.IsChildOf(transform) && !hit.transform.IsChildOf(viewer.transform.root)) { visible = false; break; }
        }
        canvas.enabled = visible;
    }
    void OnDisable() { if (canvas != null) canvas.enabled = false; }
    void OnDestroy() { if (canvas != null) Destroy(canvas.gameObject); }
}
