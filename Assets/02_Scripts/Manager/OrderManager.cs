using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OrderManager : NetworkBehaviour
{
    public static OrderManager instance;

    public RecipeSO[] allRecipes;

    private Dictionary<int, int> activeOrdersDic = new Dictionary<int, int>();

    private int nextOrderId = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }  

    void Update()
    {
        if(IsServer && Input.GetKeyDown(KeyCode.O))
        {
            CreateNewOrder();
        }
    }
    void CreateNewOrder()
    {
        if (allRecipes.Length == 0) return;

        int randomIndex = Random.Range(0, allRecipes.Length);
        int newOrderId = nextOrderId++;

        RecipeSO newOrderRecipe = allRecipes[randomIndex];

        activeOrdersDic.Add(newOrderId, randomIndex);
        ReceiveOrderClientRpc(newOrderId, randomIndex);

        Debug.Log($"New Order Created: ID {newOrderId}, Recipe {newOrderRecipe.foodName}");        
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ReceiveOrderClientRpc(int orderId, int recipeIndex)
    {
        if(!IsServer)
        {
            activeOrdersDic.Add(orderId, recipeIndex);           
        }

        RecipeSO orderedRecipe = allRecipes[recipeIndex];
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void CompleteOrderClientRpc(int orderId)
    {
        if(activeOrdersDic.ContainsKey(orderId))
        {
            activeOrdersDic.Remove(orderId);
        }
    }

    public bool TryCompleteOrder(RecipeSO completedRecipe)
    {
        if(!IsServer) return false;

        int targetOrderId = -1;

        foreach (var order in activeOrdersDic)
        {
            if (allRecipes[order.Value] == completedRecipe)
            {
                targetOrderId = order.Key;
                break;
            }
        }
        if(targetOrderId != -1)
        {           
            CompleteOrderClientRpc(targetOrderId);
            return true;
        }

        return false;
    }
}
