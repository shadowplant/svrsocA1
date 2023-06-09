using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] TextMeshProUGUI Msg;
    [SerializeField] TextMeshProUGUI coinsValueText;
    [SerializeField] TextMeshProUGUI shopDisplayText;

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text = msg.ToString();
    }

    void Updatecoins(string coins)
    {
        Debug.Log(coins);
        coinsValueText.text = coins.ToString(); 
    }


    void Updateshop(string shop)
    {
        Debug.Log(shop);
        shopDisplayText.text = shop.ToString();
    }

    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void LoadScene(string scn)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scn);
    }

    public void GetVirtualCurrencies()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
        r =>
        {
            int coins = r.VirtualCurrency["GD"];
            Updatecoins("GOLD:" + coins);
        } , OnError
            );
    }

    public void GetCatalog()
    {
        var catreq = new GetCatalogItemsRequest
        {
            CatalogVersion = "Main"
        };
        PlayFabClientAPI.GetCatalogItems(catreq,
            result =>
            {
                List<CatalogItem> items = result.Catalog;
                Updateshop("Catalog Items");
                foreach(CatalogItem i in items)
                {
                    Updateshop(i.DisplayName + "," + i.VirtualCurrencyPrices["GD"]);
                }
            } , OnError
            );
    }

    public void GetPlayerInventory()
    {
        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv,
            result => {
            List<ItemInstance> ii = result.Inventory;
            UpdateMsg("Player Inventory");
                foreach (ItemInstance i in ii)
                {
                    Updateshop(i.DisplayName + "," + i.ItemId + "," + i.ItemInstanceId);
                }
            }, OnError);
    }

    public void Buy()
    {
        var buyreq = new PurchaseItemRequest
        {
            CatalogVersion="Main",
            ItemId= "Apple",
            VirtualCurrency = "GD",
            Price=5
        };
        PlayFabClientAPI.PurchaseItem(buyreq,
            result =>{ UpdateMsg("Bought!"); }
            , OnError);
    }

    public void Back()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }
}
