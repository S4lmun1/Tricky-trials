using UnityEngine;

// Optional inventory panel. Equipment selection belongs to WeaponManager.
public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel;
    public PlayerMovement playerMovement;
    public KeyCode toggleKey = KeyCode.I;
    bool inventoryOpen;
    void Start() { if (inventoryPanel != null) inventoryPanel.SetActive(false); }
    void Update()
    {
        if (Input.GetKeyDown(toggleKey)) SetOpen(!inventoryOpen);
    }
    public void SetOpen(bool open)
    {
        inventoryOpen = open;
        if (inventoryPanel != null) inventoryPanel.SetActive(open);
        if (playerMovement != null) playerMovement.SetControlsEnabled(!open);
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
    }
    void OnDisable() { if (inventoryOpen) SetOpen(false); }
}
