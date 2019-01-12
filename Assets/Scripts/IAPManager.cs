using UnityEngine;
using System;
using System.Collections.Generic;

public enum ShopType
{
	eStore,
	eUpgradesSingle,
	eUpgradesPermanent,
	eCharacter
}

public enum CurrencyType
{
	eSoft,
	eHard,
}

public enum RewardType
{
	eCans,
	eUpgrade,
	eSkipMission
}

public enum ItemType
{
	eSingleBuy,
	eMultiBuy,
	eUpgrade
}

public class IAPManager : MonoBehaviour 
{
	static public Action<ShopItem> IAPEvents;
	public static ShopItem GetItem(ShopType type, int i)
	{
		_instantiate();
		if(s_Instance)
			return s_Instance._getItem(type, i);
		return null;
	}
	
	public static ShopItem GetItem(string id)
	{
		_instantiate();
		if(s_Instance)
			return s_Instance._getItem(id);
		return null;
	}
	
	public static  void Buy(ShopType type, int i)
	{
		_instantiate();
		if(s_Instance)
			s_Instance._buy(type, i);
	}
	
	public static int Count(ShopType type)
	{
		_instantiate();
		if(s_Instance)
			return s_Instance._count(type);
		return 0;
	}
	
	[System.Serializable]
	public class ShopItem
	{
		public enum bgColour { Blue, Gold }
		
		public string 		Description;
		public string       ExtendedDescription = " ";
		public Texture2D    Icon;
		public string 		ID;
		public CurrencyType Currency;
		public float[]  	Cost;

		public ItemType		Type;
		public bgColour     BackgroundColour; 
		
		public bool Upgrade { get { return Type == ItemType.eUpgrade; } }
		
		[System.Serializable]
		public class Value
		{
			public RewardType Type;
			public float      RewardValue;
			public string     Reference;
		}
		public Value         Reward;
		
		[HideInInspector]
		public int 			UpgradeLevel;
		
		bool m_Bought;
		public bool			Bought
		{
			get
			{
				if(Reward.Type == RewardType.eSkipMission)
					switch(Reward.Reference)
					{
					case "mission1":
						return MissionManager.Completed(0);
						break;
					case "mission2":
						return MissionManager.Completed(1);
						break;
					case "mission3":
						return MissionManager.Completed(2);
						break;
					}
				return m_Bought;
			}
			
			set
			{
				if(Reward.Type != RewardType.eSkipMission)
					m_Bought = value;
			}
		}
		
		string m_FormattedPrice;
		public string CostString
		{
			get
			{
				if(Currency == CurrencyType.eHard) return m_FormattedPrice;
				if(Cost == null || Cost.Length == 0) return "0";
				switch(Type)
				{
				case ItemType.eSingleBuy:
				case ItemType.eMultiBuy:
					return Cost[0].ToString();
					break;
				case ItemType.eUpgrade:
					if(UpgradeLevel == Cost.Length) return Cost[Cost.Length - 1].ToString();
					return Cost[UpgradeLevel].ToString();
					break;
				}
				
				return "";
			}
			set
			{
				if(Currency == CurrencyType.eHard) m_FormattedPrice = value;
			}
		}
		
		public void Buy()
		{
			if(Upgrade)
				UpgradeLevel++;
			switch (Type)
			{
			case ItemType.eSingleBuy:
				Bought = true;
				break;
			case ItemType.eMultiBuy:
				break;
			case ItemType.eUpgrade:
				Bought = UpgradeLevel >= Cost.Length;
				break;
			default:
				Bought = true;
				break;
			}
			
			MetricsManager.Log("buy_"+ID);
			
			if(IAPEvents != null)
				IAPEvents(this);
		}
		
		public void Save(string root)
		{
			root += "." + ID;
			PlayerPrefs.SetInt(root + ".level", UpgradeLevel);
			PlayerPrefs.SetInt(root + ".bought", Bought ? 1 : 0);
		}
		
		public void Load(string root)
		{
			root += "." + ID;
			if(PlayerPrefs.HasKey(root + ".level"))
				UpgradeLevel = PlayerPrefs.GetInt(root + ".level");
			if(PlayerPrefs.HasKey(root + ".bought"))
				Bought = PlayerPrefs.GetInt(root + ".bought") == 1;
		}
		
		public void Reset()
		{
			UpgradeLevel = 0;
			Bought = false;
		}
	}
	
	[SerializeField]
	ShopItem[] m_StoreItems;
	
	[SerializeField]
	ShopItem[] m_SingleUpgradeItems;
	
	[SerializeField]
	ShopItem[] m_PermanentUpgradeItems;
	
	[SerializeField]
	ShopItem[] m_CharacterItems;
	
	static IAPManager s_Instance;
	void Start()
	{
		if(s_Instance != null && s_Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		
		s_Instance = this;
		DontDestroyOnLoad(gameObject);
		
		SaveManager.Save += OnSave;
		SaveManager.Load += OnLoad;
		SaveManager.Reset += OnReset;
		
		SaveManager.Load();

                // unlock the first character
		_buy(ShopType.eCharacter, 0);
		if(GameManager.CharacterRef == string.Empty)
			GameManager.CharacterRef = m_CharacterItems[0].Reward.Reference;
		
#if UNITY_IPHONE
		StoreKitManager.productListReceivedEvent += _onStoreKitListReceived;
		StoreKitManager.purchaseSuccessfulEvent += _onStoreKitSuccessful;
		StoreKitManager.purchaseFailedEvent += _onStoreKitFailed;
		
		List<string> hardItems = new List<string>();
		foreach(ShopItem i in m_StoreItems)
			if(i.Currency == CurrencyType.eHard)
				hardItems.Add(i.ID);
		foreach(ShopItem i in m_SingleUpgradeItems)
			if(i.Currency == CurrencyType.eHard)
				hardItems.Add(i.ID);
		foreach(ShopItem i in m_PermanentUpgradeItems)
			if(i.Currency == CurrencyType.eHard)
				hardItems.Add(i.ID);
		foreach(ShopItem i in m_CharacterItems)
			if(i.Currency == CurrencyType.eHard)
				hardItems.Add(i.ID);
		
		StoreKitBinding.requestProductData(hardItems.ToArray());
#endif
	}
	
	static void _instantiate()
	{
		if(s_Instance != null) return;
		
		GameObject mgr = Instantiate(Resources.Load("IAPManager")) as GameObject;
		s_Instance = mgr.GetComponent<IAPManager>();
	}
	
	ShopItem _getItem(ShopType type, int i)
	{
		if(i < 0) return null;
		switch(type)
		{
		case ShopType.eStore:
			if(i < m_StoreItems.Length)
				return m_StoreItems[i];
			break;
		case ShopType.eUpgradesSingle:
			if(i < m_SingleUpgradeItems.Length)
				return m_SingleUpgradeItems[i];
			break;
		case ShopType.eUpgradesPermanent:
			if(i < m_PermanentUpgradeItems.Length)
				return m_PermanentUpgradeItems[i];
			break;
		case ShopType.eCharacter:
			if(i < m_CharacterItems.Length)
				return m_CharacterItems[i];
			break;
		}
		return null;
	}
	
	ShopItem _getItem(string id)
	{
		foreach(ShopItem si in m_StoreItems)
			if(si.ID == id)
				return si;
		
		foreach(ShopItem si in m_SingleUpgradeItems)
			if(si.ID == id)
				return si;
		
		foreach(ShopItem si in m_PermanentUpgradeItems)
			if(si.ID == id)
				return si;
		
		foreach(ShopItem si in m_CharacterItems)
			if(si.ID == id)
				return si;
		
		return null;
	}
	
	void _buy(ShopType type, int i)
	{
		ShopItem si = _getItem(type, i);
		if(si == null) return;
		
		switch(si.Currency)
		{
		case CurrencyType.eHard:
			if(si.Bought == false)
				StoreKitBinding.purchaseProduct(si.ID, 1);
			break;
		case CurrencyType.eSoft:
			if(si.Cost == null || si.Cost.Length == 0) break;
			if(si.Bought == false && ScoreHandler.Charge(si.Cost[si.UpgradeLevel]))
				si.Buy();
			break;
		}
	}
	
	int _count(ShopType type)
	{
		switch(type)
		{
		case ShopType.eStore:
			return m_StoreItems.Length;
			break;
		case ShopType.eUpgradesSingle:
			return m_SingleUpgradeItems.Length;
			break;
		case ShopType.eUpgradesPermanent:
			return m_PermanentUpgradeItems.Length;
			break;
		case ShopType.eCharacter:
			return m_CharacterItems.Length;
			break;
		}
		return 0;
	}
	
	void OnSave()
	{
		string root = "iap";
		foreach(ShopItem si in m_StoreItems)
			si.Save(root + ".store");
		foreach(ShopItem si in m_SingleUpgradeItems)
			si.Save(root + ".store");
		foreach(ShopItem si in m_PermanentUpgradeItems)
			si.Save(root + ".store");
		foreach(ShopItem si in m_CharacterItems)
			si.Save(root + ".store");
	}
	
	void OnLoad()
	{
		string root = "iap";
		foreach(ShopItem si in m_StoreItems)
			si.Load(root + ".store");
		foreach(ShopItem si in m_SingleUpgradeItems)
			si.Load(root + ".store");
		foreach(ShopItem si in m_PermanentUpgradeItems)
			si.Load(root + ".store");
		foreach(ShopItem si in m_CharacterItems)
			si.Load(root + ".store");
	}
	
	void OnReset()
	{
		foreach(ShopItem si in m_StoreItems)
			si.Reset();
		foreach(ShopItem si in m_SingleUpgradeItems)
			si.Reset();
		foreach(ShopItem si in m_PermanentUpgradeItems)
			si.Reset();
		foreach(ShopItem si in m_CharacterItems)
			si.Reset();
	}
#if UNITY_IPHONE	
	void _onStoreKitListReceived(List<StoreKitProduct> products)
	{
		foreach(StoreKitProduct prod in products)
		{
			ShopItem si = _getItem(prod.productIdentifier);
			si.CostString = prod.formattedPrice;
		}
	}
	
	void _onStoreKitSuccessful(StoreKitTransaction transaction)
	{
		ShopItem si = _getItem(transaction.productIdentifier);
		DebugMessages.LogMessage("Transaction successful: " + si.Description);
		si.Buy();
	}
	
	void _onStoreKitFailed(string error)
	{
		DebugMessages.LogError(error);
	}
#endif
}
