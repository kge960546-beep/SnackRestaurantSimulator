using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipiSO", menuName = "SO")]
public class RecipeSO : ScriptableObject
{
    public enum IngredientType
    {
        None,
        riceCake,
        fishCake,
        redPepperPaste,
        seaweed,
        rice,
        bun,
    }
    public enum FoodType
    {
        tteokbokki,
        kimbap,
        sundae,
        hamburger
    }

    public string foodName;
    public FoodType resultFood;

    public GameObject resultFoodPrefab;

    public List<IngredientType> ingredients;

    public float cookingTime;
    public int price;

}
