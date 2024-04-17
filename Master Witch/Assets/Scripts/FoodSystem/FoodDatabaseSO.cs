using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SO
{
    [CreateAssetMenu(fileName = "Food Database", menuName = "Game/FoodDatabase")]
    public class FoodDatabaseSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] List<FoodSO> foodContainer = new List<FoodSO>();
        [SerializeField] RecipeSO defaultRecipe;
        List<RecipeSO> recipeContainer = new List<RecipeSO>();

        #region Properties
        public List<FoodSO> FoodContainer => foodContainer;
        public List<RecipeSO> RecipeContainer { get
            {
                Debug.Log(recipeContainer.Count);
                return recipeContainer;
            }
        }
        public RecipeSO DefaultRecipe => defaultRecipe;
        #endregion

        public void OnAfterDeserialize()
        {
            recipeContainer = new List<RecipeSO>();
            for (int i = 0; i < foodContainer.Count; i++)
            {
                if (foodContainer[i] == null) continue;
                var item = foodContainer[i];
                item.foodID = i;
                if (item as RecipeSO != null)
                    recipeContainer.Add(item as RecipeSO);
            }
        }

        public void OnBeforeSerialize()
        {
        }
    }
}