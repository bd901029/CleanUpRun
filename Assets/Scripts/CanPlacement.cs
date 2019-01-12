using UnityEngine;

public class CanPlacement : MonoBehaviour 
{
	void Start () 
	{
		GameObject go = Instantiate(Resources.Load("Can")) as GameObject;
		go.transform.position = transform.position;
		go.transform.rotation = transform.rotation;
		go.transform.parent = transform.parent;
		//go.transform.localScale = Vector3.one;
		
		// we've loaded the can here, we don't need ourselves any more
		Destroy(gameObject);
	}
}
