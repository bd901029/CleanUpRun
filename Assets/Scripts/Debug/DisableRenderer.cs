using UnityEngine;

public class DisableRenderer : MonoBehaviour 
{
	void Start () 
	{
		if(renderer != null)
			renderer.enabled = false;
	}
}
