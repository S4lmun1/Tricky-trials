using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string weaponName;
        public GameObject weaponObject;
        public Sprite icon;
    }
    public List<Weapon> weapons = new List<Weapon>();
    public int currentWeaponIndex;
    [Header("Main slot (1)")]
    public Knife knifePrefab;
    public Camera playerCamera;
    public PlayerMovement player;
    public Button mainSlot;
    public float pickupRange = 3f;
    Knife heldKnife;
    TextMeshProUGUI slotLabel;
    int knifeIndex = -1;

    void Start()
    {
        CrosshairDot.Create(transform);
        if (playerCamera == null) playerCamera = Camera.main;
        if (player == null) player = FindFirstObjectByType<PlayerMovement>();
        if (mainSlot != null)
        {
            var label = new GameObject("ItemLabel", typeof(RectTransform));
            label.transform.SetParent(mainSlot.transform, false);
            slotLabel = label.AddComponent<TextMeshProUGUI>();
            slotLabel.fontSize = 20f;
            slotLabel.color = Color.black;
            slotLabel.alignment = TextAlignmentOptions.Center;
            slotLabel.raycastTarget = false;
            slotLabel.rectTransform.anchorMin = Vector2.zero;
            slotLabel.rectTransform.anchorMax = Vector2.one;
            slotLabel.rectTransform.offsetMin = Vector2.zero;
            slotLabel.rectTransform.offsetMax = Vector2.zero;
            mainSlot.onClick.AddListener(SelectMainSlot);
        }
        EquipWeapon(currentWeaponIndex);
        if (knifePrefab != null && playerCamera != null && player != null) Pickup(Instantiate(knifePrefab));
        RefreshSlot();
    }

    void Update()
    {
        if (playerCamera == null || player == null || !player.enabled || Cursor.lockState != CursorLockMode.Locked) return;
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectMainSlot();
        if (Input.GetKeyDown(KeyCode.Alpha2) && weapons.Count > 1) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha0)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Q) && heldKnife != null && currentWeaponIndex == knifeIndex)
        {
            Vector3 direction = Vector3.ProjectOnPlane(playerCamera.transform.forward, Vector3.up).normalized;
            if (direction.sqrMagnitude < 0.1f) direction = player.transform.forward;
            Vector3 origin = playerCamera.transform.position;
            // Reject obstructed drops instead of spawning inside a wall.
            foreach (var obstruction in Physics.SphereCastAll(origin, 0.3f, direction, 1.1f, ~0, QueryTriggerInteraction.Ignore))
                if (!obstruction.transform.IsChildOf(player.transform)) return;
            heldKnife.Drop(origin + direction * 1.1f, direction * 2f);
            weapons[knifeIndex].weaponObject = null;
            heldKnife = null;
            EquipWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.E) && heldKnife == null &&
            Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out var hit, pickupRange, ~0, QueryTriggerInteraction.Ignore))
        {
            var knife = hit.collider.GetComponentInParent<Knife>();
            if (knife != null && knife.CanPickup) Pickup(knife);
        }
    }

    void Pickup(Knife knife)
    {
        if (heldKnife != null) return;
        heldKnife = knife;
        knife.Equip(playerCamera, player.transform);
        if (knifeIndex < 0)
        {
            knifeIndex = weapons.Count;
            weapons.Add(new Weapon { weaponName = "Knife" });
        }
        weapons[knifeIndex].weaponObject = knife.gameObject;
        EquipWeapon(knifeIndex);
    }

    void SelectMainSlot() { EquipWeapon(heldKnife != null ? knifeIndex : 0); }
    void RefreshSlot()
    {
        if (slotLabel != null) slotLabel.text = heldKnife != null ? "Knife" : "Empty";
    }
    public void EquipWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weapons.Count) return;
        currentWeaponIndex = weaponIndex;
        foreach (Weapon weapon in weapons)
            if (weapon.weaponObject != null) weapon.weaponObject.SetActive(false);
        var selected = weapons[currentWeaponIndex];
        if (selected.weaponObject != null) selected.weaponObject.SetActive(true);
        RefreshSlot();
    }
}
