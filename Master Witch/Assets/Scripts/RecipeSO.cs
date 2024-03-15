using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.SO;

namespace Game.SO {

[CreateAssetMenu(fileName = "RecipeSO", menuName = "Game/RecipeSO")]
    public class RecipeSO : ScriptableObject{
       
        public CategoryModifier categoryModifier;
        public RecipeCondition [] recipeConditions;
        public bool CheckConditions()
        {
            return false;
        }
    }
}
