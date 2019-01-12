using UnityEngine;

public class PerkPlacement : MonoBehaviour 
{
	void Update()
	{
		if(GameManager.Instance == null || GameManager.Instance.LocalPlayer == null) return;
		GameObject player = GameManager.Instance.LocalPlayer;
		PerkManager perks = player.GetComponent<PerkManager>();
		if(perks == null) return;
		if(perks.PerksAvailable)
		{
			GameObject prefab = Resources.Load("Perk") as GameObject;
			if(prefab != null)
			{
				GameObject go = Instantiate(prefab) as GameObject;
				go.transform.position = transform.position;
				go.transform.rotation = transform.rotation;
				go.transform.parent = transform.parent;
				go.transform.localScale = Vector3.one;
			}
		}
		// regardless of whether we were allowed to spawn a perk or not,
		// clean up after ourselves
		Destroy(gameObject);
	}
}
