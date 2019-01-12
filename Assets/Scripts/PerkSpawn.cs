using UnityEngine;
using System.Collections;

public class PerkSpawn : MonoBehaviour 
{
	[SerializeField]
	GameObject[] Perks;
	
	void Start()
	{
		GameObject perk = Instantiate(Perks[Random.Range(0, Perks.Length)]) as GameObject;
		perk.transform.position = transform.position;
		perk.transform.rotation = transform.rotation;
		perk.transform.parent = transform.parent;
		
		perk.rigidbody.isKinematic = true;
		
		Spinner spinner = perk.AddComponent<Spinner>();
		spinner.m_RotateSpeed = GetComponent<Spinner>().m_RotateSpeed;
		
		Lifetime life = perk.GetComponent<Lifetime>();
		if(life != null)
			Destroy(life); //mwahahaha
		
		// our job here is done
		Destroy(gameObject);
	}
}
