using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelRewards : MonoBehaviour
{
    public WeaponManager equipment;
    public PlayerMovement player;
    public RunUpgrades upgrades;
    public EnemyGroup[] encounters;
    public UpgradePool emptyHandPool;
    [Header("Presentation")]
    [Min(1)] public int cardCount = 5;
    public Vector2 referenceResolution = new Vector2(1600, 900);
    public Vector2 cardSize = new Vector2(230, 330);
    public float cardSpacing = 24f;
    [Min(0.01f)] public float dealDuration = 0.3f, dealStagger = 0.09f, flipDuration = 0.32f;
    public Color backdrop = new Color(0.025f, 0.035f, 0.055f, 0.94f);
    public Color cardBack = new Color(0.105f, 0.13f, 0.18f);
    public string heading = "LEVEL COMPLETE", instruction = "Choose one card. Your upgrade lasts for this run.";
    readonly HashSet<EnemyGroup> rewarded = new HashSet<EnemyGroup>();
    readonly Queue<(EnemyGroup group, ItemData item)> pending = new Queue<(EnemyGroup, ItemData)>();
    GameObject overlay;
    bool showing, selected, canChoose;
    float previousTimeScale;
    bool previousControls, previousCursorVisible;
    CursorLockMode previousLock;
    void Start()
    {
        if (equipment == null) equipment = GetComponent<WeaponManager>();
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (upgrades == null && player != null) upgrades = player.GetComponent<RunUpgrades>();
        if (encounters == null || encounters.Length == 0) encounters = FindObjectsByType<EnemyGroup>(FindObjectsSortMode.None);
    }
    public void OnPortalArrived(EnemyGroup group, PlayerMovement traveler)
    {
        if (traveler != player || group == null || !group.IsCleared || rewarded.Contains(group)) return;
        if (encounters != null && encounters.Length > 0 && System.Array.IndexOf(encounters, group) < 0) return;
        // Level completion is crossing its exit. Snapshot equipment at that moment.
        if (equipment == null) { Debug.LogError("Assign the player's WeaponManager to Level Rewards.", this); return; }
        var item = equipment.EquippedItem;
        if (equipment.EquippedObject != null && item == null)
        {
            Debug.LogError("The equipped object needs ItemData before it can receive item rewards.", equipment.EquippedObject);
            return;
        }
        rewarded.Add(group);
        group.RewardPending = true;
        pending.Enqueue((group, item));
    }
    void LateUpdate()
    {
        if (!showing && pending.Count > 0 && player != null && player.ControlsEnabled)
        {
            var reward = pending.Dequeue();
            StartCoroutine(Show(reward.group, reward.item));
        }
    }
    IEnumerator Show(EnemyGroup group, ItemData item)
    {
        if (upgrades == null) { Debug.LogError("Level Rewards needs a Run Upgrades component.", this); group.RewardPending = false; yield break; }
        var pool = item != null ? item.upgradePool : emptyHandPool;
        var cards = new UpgradeCard[Mathf.Max(1, cardCount)];
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i] = pool != null ? pool.Draw(card => upgrades.Eligible(item, card)) : null;
            if (cards[i] == null)
            {
                string owner = item != null ? item.itemName : "Empty hands";
                string detail = pool == null ? "no pool assigned" : pool.name + " has " + (pool.cards != null ? pool.cards.Length : 0) + " entries but none are eligible";
                Debug.LogError("Cannot offer " + owner + " upgrades: " + detail + ". Check that item's Upgrade Pool asset.", this);
                rewarded.Remove(group);
                group.RewardPending = false;
                yield break;
            }
        }
        showing = true; selected = canChoose = false;
        previousTimeScale = Time.timeScale; previousControls = player.ControlsEnabled;
        previousLock = Cursor.lockState; previousCursorVisible = Cursor.visible;
        player.SetControlsEnabled(false); Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
        overlay = new GameObject("Level Reward Cards", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = overlay.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 500;
        var scaler = overlay.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = referenceResolution; scaler.matchWidthOrHeight = 0.5f;
        var shade = Rect("Backdrop", overlay.transform, Vector2.zero, Vector2.zero);
        shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.sizeDelta = Vector2.zero;
        shade.gameObject.AddComponent<Image>().color = backdrop;
        Label("Title", overlay.transform, new Vector2(0, 285), new Vector2(1300, 65), heading, 36);
        Label("Subtitle", overlay.transform, new Vector2(0, 228), new Vector2(1300, 50), item != null ? item.itemName + "  /  " + instruction : "NO ITEM  /  " + instruction, 19);
        var rects = new RectTransform[cards.Length];
        var buttons = new Button[cards.Length];
        var texts = new TextMeshProUGUI[cards.Length];
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i;
            rects[i] = Rect("Card", overlay.transform, new Vector2(0, -550), cardSize);
            var image = rects[i].gameObject.AddComponent<Image>(); image.color = cardBack;
            var outline = rects[i].gameObject.AddComponent<Outline>(); outline.effectColor = new Color(0.8f, 0.9f, 1f, 0.35f); outline.effectDistance = Vector2.one;
            buttons[i] = rects[i].gameObject.AddComponent<Button>(); buttons[i].targetGraphic = image;
            texts[i] = Label("Face", rects[i], Vector2.zero, cardSize - new Vector2(28, 28), "<size=58>?</size>\n\n<size=15>UNKNOWN UPGRADE</size>", 22);
            buttons[i].onClick.AddListener(() => { if (canChoose && !selected) { selected = true; StartCoroutine(Reveal(group, item, cards[index], index, rects, buttons, texts)); } });
        }
        float duration = dealDuration + (cards.Length - 1) * dealStagger;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                float f = Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - i * dealStagger) / Mathf.Max(0.01f, dealDuration)));
                rects[i].anchoredPosition = Vector2.Lerp(new Vector2(0, -550), new Vector2((i - (cards.Length - 1) * 0.5f) * (cardSize.x + cardSpacing), -5), f);
                rects[i].localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(-12, 0, f));
            }
            yield return null;
        }
        for (int i = 0; i < cards.Length; i++) { rects[i].anchoredPosition = new Vector2((i - (cards.Length - 1) * 0.5f) * (cardSize.x + cardSpacing), -5); rects[i].localRotation = Quaternion.identity; }
        canChoose = true;
    }
    IEnumerator Reveal(EnemyGroup group, ItemData item, UpgradeCard card, int chosen, RectTransform[] rects, Button[] buttons, TextMeshProUGUI[] texts)
    {
        foreach (var button in buttons) button.interactable = false;
        bool applied = false;
        for (float t = 0; t < flipDuration; t += Time.unscaledDeltaTime)
        {
            float f = t / Mathf.Max(0.01f, flipDuration);
            rects[chosen].localScale = new Vector3(Mathf.Abs(1f - 2f * f), 1f, 1f);
            if (f >= 0.5f && !applied) { ApplyFace(); applied = true; }
            yield return null;
        }
        if (!applied) ApplyFace();
        rects[chosen].localScale = Vector3.one;
        var proceed = Rect("Continue", overlay.transform, new Vector2(0, -270), new Vector2(240, 54));
        proceed.gameObject.AddComponent<Image>().color = card.accent;
        var buttonContinue = proceed.gameObject.AddComponent<Button>();
        var label = Label("Text", proceed, Vector2.zero, new Vector2(240, 54), "CONTINUE", 19); label.color = Color.black;
        buttonContinue.onClick.AddListener(() => StartCoroutine(Close(group)));
        void ApplyFace()
        {
            upgrades.Apply(item, card, player);
            rects[chosen].GetComponent<Image>().color = new Color(0.16f, 0.2f, 0.25f);
            texts[chosen].color = card.accent;
            texts[chosen].text = "<b>" + card.title + "</b>\n\n<size=18>" + card.description + "</size>\n\n<size=14>APPLIED FOR THIS RUN</size>";
        }
    }
    IEnumerator Close(EnemyGroup group)
    {
        Destroy(overlay); overlay = null;
        // Consume the UI click before re-enabling weapon input.
        yield return null;
        Restore();
        if (group != null) group.RewardPending = false;
        showing = false;
    }
    void Restore()
    {
        Time.timeScale = previousTimeScale;
        if (player != null) player.SetControlsEnabled(previousControls);
        Cursor.lockState = previousLock; Cursor.visible = previousCursorVisible;
    }
    void OnDestroy()
    {
        if (showing) Restore();
        if (overlay != null) Destroy(overlay);
    }
    static RectTransform Rect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f); rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
    }
    static TextMeshProUGUI Label(string name, Transform parent, Vector2 position, Vector2 size, string content, float fontSize)
    {
        var text = Rect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>(); text.text = content; text.fontSize = fontSize; text.alignment = TextAlignmentOptions.Center; text.color = Color.white; text.raycastTarget = false; return text;
    }
}
