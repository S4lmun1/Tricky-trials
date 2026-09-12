using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public ItemData definition;
        public GameObject weaponObject;
        public Sprite icon;
        public KeyCode selectKey = KeyCode.None;
    }
    [Header("Equipment (add entries and assign selection keys)")]
    public List<Weapon> weapons = new List<Weapon>();
    public int currentWeaponIndex;
    public int emptyHandsIndex = -1;
    [Header("Main Pickup Slot")]
    [FormerlySerializedAs("knifePrefab")] public PickupItem startingItem;
    public Camera playerCamera;
    public PlayerMovement player;
    public Button mainSlot;
    public Transform heldSocket;
    [Header("Controls")]
    public KeyCode mainSlotKey = KeyCode.Alpha1;
    public KeyCode pickupKey = KeyCode.E;
    public KeyCode dropKey = KeyCode.Q;
    public KeyCode attackKey = KeyCode.Mouse0;
    [Header("Interaction")]
    [Min(0f)] public float pickupRange = 3f;
    [Min(0.1f)] public float dropDistance = 1.1f;
    [Min(0.01f)] public float dropClearance = 0.3f;
    public float dropSpeed = 2f;
    public LayerMask interactionLayers = ~0;
    [Header("HUD")]
    public bool showCrosshair = true;
    [Min(0.5f)] public float crosshairRadius = 2f;
    [Min(0f)] public float crosshairOutlineWidth = 1f;
    public Color crosshairColor = Color.white;
    public Color crosshairOutlineColor = new Color(0f, 0f, 0f, 0.7f);
    public int crosshairSortOrder = 100;
    public string emptySlotText = "";
    public float slotFontSize = 20f;
    public Color slotTextColor = Color.black;
    PickupItem mainItem;
    TextMeshProUGUI slotLabel;
    int mainItemIndex = -1;

    void Start()
    {
        if (showCrosshair) CrosshairDot.Create(transform, crosshairRadius, crosshairOutlineWidth, crosshairColor, crosshairOutlineColor, crosshairSortOrder);
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (playerCamera == null && player != null && player.playerCamera != null) playerCamera = player.playerCamera.GetComponent<Camera>();
        if (playerCamera == null) playerCamera = Camera.main;
        if (mainSlot != null)
        {
            var label = new GameObject("ItemLabel", typeof(RectTransform));
            label.transform.SetParent(mainSlot.transform, false);
            slotLabel = label.AddComponent<TextMeshProUGUI>();
            slotLabel.fontSize = slotFontSize;
            slotLabel.color = slotTextColor;
            slotLabel.alignment = TextAlignmentOptions.Center;
            slotLabel.raycastTarget = false;
            slotLabel.rectTransform.anchorMin = Vector2.zero;
            slotLabel.rectTransform.anchorMax = Vector2.one;
            slotLabel.rectTransform.offsetMin = Vector2.zero;
            slotLabel.rectTransform.offsetMax = Vector2.zero;
            mainSlot.onClick.AddListener(SelectMainSlot);
        }
        foreach (var weapon in weapons)
        {
            if (weapon.weaponObject == null && weapon.definition != null && weapon.definition.heldPrefab != null && playerCamera != null)
                weapon.weaponObject = Instantiate(weapon.definition.heldPrefab, heldSocket != null ? heldSocket : playerCamera.transform);
            if (weapon.weaponObject != null && weapon.weaponObject.TryGetComponent<PickupItem>(out var item) && playerCamera != null && player != null)
                item.Equip(playerCamera, player.transform, heldSocket);
        }
        EquipWeapon(currentWeaponIndex);
        if (startingItem != null && playerCamera != null && player != null) TryPickup(Instantiate(startingItem));
        RefreshSlot();
    }

    void OnDestroy() { if (mainSlot != null) mainSlot.onClick.RemoveListener(SelectMainSlot); }

    void Update()
    {
        if (playerCamera == null || player == null || !player.ControlsEnabled || Cursor.lockState != CursorLockMode.Locked) return;
        if (Input.GetKeyDown(mainSlotKey)) SelectMainSlot();
        for (int i = 0; i < weapons.Count; i++)
            if (weapons[i].selectKey != KeyCode.None && Input.GetKeyDown(weapons[i].selectKey)) EquipWeapon(i);
        if (Input.GetKeyDown(dropKey)) DropCurrentItem();
        if (Input.GetKeyDown(pickupKey) && mainItem == null &&
            CombatRaycast.TryHit(playerCamera, player.transform, pickupRange, interactionLayers, out var hit))
        {
            var item = hit.collider.GetComponentInParent<PickupItem>();
            if (item != null && item.CanPickup) TryPickup(item);
        }
        if (Input.GetKeyDown(attackKey) && currentWeaponIndex >= 0 && currentWeaponIndex < weapons.Count)
        {
            var weapon = weapons[currentWeaponIndex].weaponObject;
            if (weapon != null)
                foreach (var component in weapon.GetComponents<MonoBehaviour>())
                    if (component is IWeaponAction action) { action.Use(playerCamera, player.transform); break; }
        }
    }

    public bool TryPickup(PickupItem item)
    {
        if (item == null || !item.CanPickup || mainItem != null || playerCamera == null || player == null) return false;
        mainItem = item;
        item.Equip(playerCamera, player.transform, heldSocket);
        if (mainItemIndex < 0)
        {
            mainItemIndex = weapons.Count;
            weapons.Add(new Weapon());
        }
        weapons[mainItemIndex].weaponName = item.DisplayName;
        weapons[mainItemIndex].definition = item.itemData;
        weapons[mainItemIndex].weaponObject = item.gameObject;
        EquipWeapon(mainItemIndex);
        return true;
    }

    public bool DropCurrentItem()
    {
        if (currentWeaponIndex < 0 || currentWeaponIndex >= weapons.Count || playerCamera == null || player == null) return false;
        var selected = weapons[currentWeaponIndex];
        if (selected.weaponObject == null || !selected.weaponObject.TryGetComponent<PickupItem>(out var item)) return false;
        Vector3 direction = player.ViewYawRotation * Vector3.forward;
        Vector3 origin = playerCamera.transform.position;
        foreach (var obstruction in Physics.SphereCastAll(origin, Mathf.Max(0.01f, dropClearance), direction, Mathf.Max(0.1f, dropDistance), interactionLayers, QueryTriggerInteraction.Ignore))
            if (!obstruction.transform.IsChildOf(player.transform)) return false;
        item.Drop(origin + direction * dropDistance, direction * dropSpeed);
        selected.weaponObject = null;
        if (item == mainItem) mainItem = null;
        EquipWeapon(emptyHandsIndex);
        RefreshSlot();
        return true;
    }

    public void SelectMainSlot() { EquipWeapon(mainItem != null ? mainItemIndex : emptyHandsIndex); }
    void RefreshSlot() { if (slotLabel != null) slotLabel.text = mainItem != null ? mainItem.DisplayName : emptySlotText; }
    public void EquipWeapon(int weaponIndex)
    {
        foreach (var weapon in weapons)
            if (weapon.weaponObject != null) weapon.weaponObject.SetActive(false);
        currentWeaponIndex = weaponIndex >= 0 && weaponIndex < weapons.Count ? weaponIndex : -1;
        if (currentWeaponIndex >= 0 && weapons[currentWeaponIndex].weaponObject != null)
            weapons[currentWeaponIndex].weaponObject.SetActive(true);
        RefreshSlot();
    }
}
