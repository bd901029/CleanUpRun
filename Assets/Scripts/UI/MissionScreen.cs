using UnityEngine;

public class MissionScreen : FlashPage
{
	protected override void Start () 
	{
		base.Start();
		
		for(int i = 0; i < 3; ++i)
		{
			Set ("Mission"+(i+1), MissionManager.Description(i));
			if(MissionManager.Completed(i))
				Set ("Check"+(i+1), 2);
		}
		
		Set("Reward", MissionManager.Reward());
	}
}
