using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CookingStation : NetworkBehaviour
{
    public enum CookingType { pot, pan, Board, plate }
    public CookingType cookingType;

    public RecipeSO currentRecipe;
    public List<RecipeSO.IngredientType> addIngredients = new List<RecipeSO.IngredientType>();
    public List<RecipeSO> recipeData;

    public NetworkVariable<bool> isCooking = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isFoodReady = new NetworkVariable<bool>(false);

    public void AddIngredient(RecipeSO.IngredientType newIngredient)
    {
        if (!IsServer) return;
        if (isCooking.Value || isFoodReady.Value) return;

        addIngredients.Add(newIngredient);
        SuccessCheck();

    }
    public void SuccessCheck()
    {
        if (isCooking.Value) return;

        foreach (var recipe in recipeData)
        {
            if (IsMatching(recipe))
            {
                currentRecipe = recipe;
                StartCoroutine(CookingCo(currentRecipe));
                break;
            }
        }
    }

    bool IsMatching(RecipeSO recipe)
    {
        if (addIngredients.Count != recipe.ingredients.Count) return false;

        List<RecipeSO.IngredientType> tempCheck = new List<RecipeSO.IngredientType>(addIngredients);
        foreach (var ingredient in recipe.ingredients)
        {
            if (!tempCheck.Remove(ingredient)) return false;
        }

        return tempCheck.Count == 0;
    }

    IEnumerator CookingCo(RecipeSO recipe)
    {
        isCooking.Value = true;

        if (recipe.cookingTime > 0)
        {
            yield return new WaitForSeconds(recipe.cookingTime);
        }

        isCooking.Value = false;
        isFoodReady.Value = true;
    }

    public void Interact()
    {
        SpawnFoodRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void SpawnFoodRpc()
    {
        if (isFoodReady.Value && currentRecipe != null)
        {
            GameObject food = PoolManager.instance.Get(currentRecipe.resultFoodPrefab, transform.position + Vector3.up, Quaternion.identity);
            
            if(food.TryGetComponent(out NetworkObject netObj) && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }           

            addIngredients.Clear();
            currentRecipe = null;
            isFoodReady.Value = false;
        }
    }
}
