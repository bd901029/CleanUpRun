using UnityEngine;
using System.IO;

public class DebugFunctions : MonoBehaviour 
{
	void LoginFacebook()
	{
		SocialManager.Authenticate(SocialNetwork.eFacebook);
	}
	
	void PostTestFacebook()
	{
		SocialManager.PostScore(999, SocialNetwork.eFacebook);
	}
	
	void DumpTextureData()
	{
#if UNITY_EDITOR
		string fileName = Application.persistentDataPath + "/" + "TextureData.txt";
		var fileWriter = File.CreateText(fileName);
		Object[] objs = Resources.FindObjectsOfTypeAll(typeof(Texture));
		
		foreach(Object o in objs)
		{
			uint size = (uint)Profiler.GetRuntimeMemorySize(o);
			bool hasName = o.name != string.Empty;
			fileWriter.WriteLine(string.Format("{2} ({1}) {0}",
												hasName ? o.name : "<NO NAME>", 
												DebugMemWatcher.FormatMemory(size),
												size
												));
		}
		fileWriter.Close();
		
		Debug.Log("Written Texture dump to: " + fileName);
#endif
	}
	
	void ClearData()
	{
		SaveManager.Reset();
	}
	
	void CompleteMission1()
	{
		MissionManager.CompleteMission(0);
	}
	
	void CompleteMission2()
	{
		MissionManager.CompleteMission(1);
	}
	
	void CompleteMission3()
	{
		MissionManager.CompleteMission(2);
	}
	
	void _givePerk(PerkType p)
	{
		if(GameManager.Instance == null || GameManager.Instance.LocalPlayer == null) return;
		PerkManager perk = GameManager.Instance.LocalPlayer.GetComponent<PerkManager>();
		if(perk == null) return;
		perk.OnCollisionObject(CollisionObject.CollisionMessage.ePerk, 1, p.ToString());
	}
	
	void PerkHealth()
	{
		_givePerk(PerkType.eHealth);
	}
	void PerkMagnet()
	{
		_givePerk(PerkType.eMagnet);
	}
	void PerkDoubleCoins()
	{
		_givePerk(PerkType.eDoubleCoins);
	}
	void PerkStopItems()
	{
		_givePerk(PerkType.eStopItems);
	}
	void PerkShield()
	{
		_givePerk(PerkType.eShield);
	}
	
	void _setLocation(string loc)
	{
		GameManager.DebugLocationName = loc;
		Course.DebugDirty = true;
	}
	
	void SetAlley()
	{
		_setLocation("AlleySettings");
	}
	
	void SetStores()
	{
		_setLocation("StoresSettings");			
	}
	
	void SetGreen()
	{
		_setLocation("GreenSettings");			
	}
	
	void ClearLocation()
	{
		_setLocation("");
	}
	
	void _setPiece(string pce)
	{
		EnvironmentSettings.DebugSectionName = pce;
	}
	
	void SetStraight()
	{
		_setPiece("Straight");
	}
	
	void SetUp()
	{
		_setPiece("Up");
	}
	
	void SetDown()
	{
		_setPiece("Down");
	}
	
	void SetLeft()
	{
		_setPiece("Left");
	}
	
	void SetRight()
	{
		_setPiece("Right");
	}
	
	void ClearPiece()
	{
		_setPiece("");
	}
	
	void GiveCoins()
	{
		int? val = DebugMenu.Get("DebugCoins") as int?;
		ScoreHandler.AddCans(val.HasValue ? (float)val.Value : 0);
		DebugMenu.Set("DebugCoins", 0);
	}
	
	void RateApp()
	{
		SocialManager.RequestReview(true);
	}
	
	public static DebugFunctions Instance { get { _initialise(); return s_instance; } }
	
	static DebugFunctions s_instance;
	void Start()
	{
		if(s_instance != null && s_instance != this)
		{
			Destroy(gameObject);
			return;
		}
		s_instance = this;
	}
	
	static void _initialise()
	{
		if(s_instance != null) return;
		
		GameObject obj = Instantiate(Resources.Load("DebugManager")) as GameObject;
		s_instance = obj.GetComponent<DebugFunctions>();
		DontDestroyOnLoad(obj);
	}
}
