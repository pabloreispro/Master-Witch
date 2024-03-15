using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SO{
    [CreateAssetMenu(fileName = "FoodSO", menuName = "Game/FoodSO")]
    public class FoodSO : ScriptableObject{
        public int foodID;
        public Category[] category;
        public GameObject foodPrefab;

    }
}
