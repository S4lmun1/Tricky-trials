using UnityEngine;

public class Gun : MonoBehaviour
{
	public float damage = 25f;
	public float range = 100f;

	public Camera playerCamera;

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			Shoot();
		}
	}

	void Shoot()
	{
		RaycastHit hit;

		if (Physics.Raycast(
			playerCamera.transform.position,
			playerCamera.transform.forward,
			out hit,
			range))
		{
			Debug.Log("You hit: " + hit.transform.name);
		}
	}
}