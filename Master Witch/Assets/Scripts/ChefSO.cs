using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SO
{
    [CreateAssetMenu(fileName = "ChefSO", menuName = "Game/ChefSO")]
    public class ChefSO : MonoBehaviour
    {
        int id;
        [SerializeField] GameObject prefab;

    }
    public class ReviewConditions
    {
        FoodPreference[] foodPreferences;


        public class FoodPreference
        {
            public FoodSO food;
            public bool isFavorite;
        }
        public class CategoryPreference
        {
            public Category category;
            public float modifier;
        }
    }
}