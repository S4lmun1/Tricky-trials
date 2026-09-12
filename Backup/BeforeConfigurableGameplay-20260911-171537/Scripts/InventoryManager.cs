using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
	public GameObject inventoryPanel;

	[Header("Player")]
	public PlayerMovement playerMovement;

	[Header("Gun")]
	public GameObject hgun;
	public Button hGunButton;

	private bool inventoryOpen = false;
	private bool gunEquipped = true;

	void Start()
	{
		inventoryPanel.SetActive(false);

		hgun.SetActive(true);

		hGunButton.onClick.AddListener(ToggleGun);
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			inventoryOpen = !inventoryOpen;

			inventoryPanel.SetActive(inventoryOpen);

			if (inventoryOpen)
			{
				// Stop movement and camera controls
				playerMovement.enabled = false;

				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				// Turn movement and camera controls back on
				playerMovement.enabled = true;

				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}
	}

	void ToggleGun()
	{
		gunEquipped = !gunEquipped;

		hgun.SetActive(gunEquipped);
	}
}