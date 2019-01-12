using UnityEngine;
using System.Collections;

public class MenuCharacters : MonoBehaviour 
{
    [SerializeField]
    GameObject[] Characters;
    
    void Start()
    {
        UpdateCharacters();
    }
    
    void UpdateCharacters()
    {
        if(Characters == null) return;
        for(int i = 0; i < Characters.Length; ++i)
        {
            IAPManager.ShopItem item = IAPManager.GetItem(ShopType.eCharacter, i);
            
            Characters[i].SetActive(item != null && item.Bought);
        }
        
        Invoke("UpdateCharacters", 0.2f);
    }
}
