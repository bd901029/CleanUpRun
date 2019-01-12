using UnityEngine;
using System.Collections;

public class CharacterShopScreen : ShopScreen
{
	int m_SelectedCharacter;
	protected override void UpdateScreen ()
	{
		ScoreHandler.Commit();
		Set("Balance", ScoreHandler.TotalCans.ToString());
		
		for(int i = 0; i < 3; ++i)
			Set ("Character"+(i+1).ToString(), i+1);
		
		Set ("Character", m_SelectedCharacter+1);
		
		IAPManager.ShopItem character = IAPManager.GetItem(ShopType.eCharacter, m_SelectedCharacter);
		Set ("CharacterName", character.Description);
		
		Set ("Cost", character.CostString);
		if(!character.Bought)
			Set ("CharacterState", 3);
		else if(character.Reward.Reference != GameManager.CharacterRef)
			Set ("CharacterState", 2);
		else
			Set ("CharacterState", 1);
		
		Invoke("UpdateScreen", 0.2f);
	}
	
	protected override void OnMenuEvents(string arg)
	{
		switch(arg)
		{
		case "Character1":
			m_SelectedCharacter = 0;
			break;
		case "Character2":
			m_SelectedCharacter = 1;
			break;
		case "Character3":
			m_SelectedCharacter = 2;
			break;
		case "Select":
			{
				IAPManager.ShopItem character = IAPManager.GetItem(ShopType.eCharacter, m_SelectedCharacter);
				if(!character.Bought)
					IAPManager.Buy(ShopType.eCharacter, m_SelectedCharacter);
				else
					GameManager.CharacterRef = character.Reward.Reference;
			}
			break;
		case "Home":
			LoadPage(Player.EndGame ? BackGame : BackFE);
			break;
		}
	}
}
