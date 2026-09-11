using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
	[System.Serializable]
	public class Weapon
	{
		public string weaponName;

		// The actual weapon object under the camera.
		// Leave this empty for fists.
		public GameObject weaponObject;

		// We'll use this for the hotbar later.
		public Sprite icon;
	}

	[Header("Weapons")]
	public List<Weapon> weapons = new List<Weapon>();

	[Header("Current Weapon")]
	public int currentWeaponIndex = 0;

	void Start()
	{
		EquipWeapon(currentWeaponIndex);
	}

	public void EquipWeapon(int weaponIndex)
	{
		// Make sure the number is valid
		if (weaponIndex < 0 || weaponIndex >= weapons.Count)
			return;

		currentWeaponIndex = weaponIndex;

		// Hide every weapon
		foreach (Weapon weapon in weapons)
		{
			if (weapon.weaponObject != null)
			{
				weapon.weaponObject.SetActive(false);
			}
		}

		// Turn on the selected weapon
		Weapon selectedWeapon = weapons[currentWeaponIndex];

		if (selectedWeapon.weaponObject != null)
		{
			selectedWeapon.weaponObject.SetActive(true);
		}
	}
}