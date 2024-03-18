using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SO
{
    [CreateAssetMenu(fileName = "Food Database", menuName = "Game/FoodDatabase")]
    public class FoodDatabaseSO : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] List<FoodSO> foodContainer = new List<FoodSO>();
        List<RecipeSO> recipeContainer = new List<RecipeSO>();

        #region Properties
        public List<FoodSO> FoodContainer => foodContainer;
        public List<RecipeSO> RecipeContainer => recipeContainer;
        #endregion

        public void OnAfterDeserialize()
        {
            recipeContainer = new List<RecipeSO>();
            foreach (var item in foodContainer)
            {
                if (item as RecipeSO != null)
                    recipeContainer.Add(item as RecipeSO);
            }
        }

        public void OnBeforeSerialize()
        {
        }
    }
}