using Unity.Netcode;
using UnityEngine;

public class DeliveryStation : NetworkBehaviour
{
    public void Interact(GameObject heldItem)
    {
        if (heldItem == null) return;

        if (heldItem.TryGetComponent(out Food food))
        {
            RequestDeliverServerRpc(food.NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    void RequestDeliverServerRpc(ulong foodNetworkId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(foodNetworkId, out NetworkObject foodObj))
        {
            Food foodComponent = foodObj.GetComponent<Food>();

            RecipeSO sumbittedRecipe = foodComponent.recipeData;

            bool isSuccess = OrderManager.instance.TryCompleteOrder(sumbittedRecipe);

            if (isSuccess)
            {
                foodObj.TryRemoveParent();
                foodObj.Despawn(false);

                PoolManager.instance.ReturnIt(foodComponent.originPrefab, foodObj.gameObject);
            }
        }
    }
}
