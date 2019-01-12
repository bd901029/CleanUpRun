using UnityEngine;

public class ShopScreen : FlashPage
{
	[SerializeField]
	ShopType m_Type;
	
	[SerializeField]
	protected GameObject BackFE;
	
	[SerializeField]
	protected GameObject BackGame;
	
	int m_ShopOffset = 0;
	
	protected override void Start()
	{
		base.Start ();
		
		UpdateScreen();
		
		MenuEvents += OnMenuEvents;
		
		MetricsManager.Log("EnterShop_"+m_Type.ToString());
	}
	
	protected virtual void UpdateScreen()
	{
		ScoreHandler.Commit();
		Set("Balance", ScoreHandler.TotalCans.ToString());
		for(int i = m_ShopOffset; i < m_ShopOffset + 4; ++i)
		{
			IAPManager.ShopItem item = IAPManager.GetItem(m_Type, i);
			int id = i - m_ShopOffset + 1;
			if(item != null)
			{
				Set ("Item" + id.ToString(), item.Upgrade ? 2 + item.UpgradeLevel : 1);
				Set ("ItemDesc" + id.ToString(), item.Description, true);
				Set ("ItemExtDesc" + id.ToString(), item.ExtendedDescription, true);
				Set ("ItemPrice" + id.ToString(), item.CostString);
				Set ("ItemCan" + id.ToString(), item.Currency == CurrencyType.eSoft ? 1 : 2);
				Set ("ItemBg" + id.ToString(), 1 + (int)item.BackgroundColour);
				if(item.Icon != null)
					Set ("ItemIcon" + id.ToString(), item.Icon);
			}
			else
				Set ("Item" + id.ToString(), 8);
		}
		if(m_Type == ShopType.eUpgradesSingle)
		{
			Set ("SingleUseBtn", 2);
			Set ("UpgradesBtn", 1);
		}
		else if(m_Type == ShopType.eUpgradesPermanent)
		{
			Set ("SingleUseBtn", 1);
			Set ("UpgradesBtn", 2);
		}
		else
		{
			Set ("SingleUseBtn", 3);
			Set ("UpgradesBtn", 3);
		}
		
		Set ("NextButton", (m_ShopOffset + 4 >= IAPManager.Count(m_Type)) ? 2 : 1);
		Set ("PreviousButton", m_ShopOffset <= 0 ? 2 : 1);
		
		Invoke("UpdateScreen", 0.2f);
	}
	
	void OnDestroy()
	{
		MenuEvents -= OnMenuEvents;
		MetricsManager.Log("ExitShop_"+m_Type.ToString());
	}
	
	protected virtual void OnMenuEvents(string arg)
	{
		switch(arg)
		{
		case "Buy1":
			MetricsManager.Log("TryBuy_"+m_Type.ToString()+(m_ShopOffset+0).ToString());
			IAPManager.Buy(m_Type, m_ShopOffset + 0);
			break;
		case "Buy2":
			MetricsManager.Log("TryBuy_"+m_Type.ToString()+(m_ShopOffset+0).ToString());
			IAPManager.Buy(m_Type, m_ShopOffset + 1);
			break;
		case "Buy3":
			MetricsManager.Log("TryBuy_"+m_Type.ToString()+(m_ShopOffset+0).ToString());
			IAPManager.Buy(m_Type, m_ShopOffset + 2);
			break;
		case "Buy4":
			MetricsManager.Log("TryBuy_"+m_Type.ToString()+(m_ShopOffset+0).ToString());
			IAPManager.Buy(m_Type, m_ShopOffset + 3);
			break;
		case "Next":
			if(m_ShopOffset + 4 < IAPManager.Count(m_Type))
				m_ShopOffset += 4;
			break;
		case "Previous":
			if(m_ShopOffset > 0)
				m_ShopOffset -= 4;
			break;
		case "Home":
			LoadPage(Player.EndGame ? BackGame : BackFE);
			break;
		}
		
		SaveManager.Save();
	}
}
