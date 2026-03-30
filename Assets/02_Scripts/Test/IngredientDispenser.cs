using Unity.Netcode;
using UnityEngine;

public class IngredientDispenser : NetworkBehaviour
{
    public GameObject ingredientPrefab; // 스폰할 재료 프리팹 (Sphere)
    public Transform spawnPoint;
    
    public void Interact()
    {
        RequestSpawnRpc();
    }

    void RequestSpawnRpc()
    {
        GameObject obj = PoolManager.instance.Get(ingredientPrefab, spawnPoint.position, spawnPoint.rotation);

        if (obj.TryGetComponent(out Ingredient ingredient))
        {
            ingredient.originPrefab = ingredientPrefab;
        }
    }
}
