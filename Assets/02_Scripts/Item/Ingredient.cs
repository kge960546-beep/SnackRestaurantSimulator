using Unity.Netcode;
using UnityEngine;

public class Ingredient : NetworkBehaviour
{
    public RecipeSO.IngredientType type;

    public GameObject originPrefab;
}
