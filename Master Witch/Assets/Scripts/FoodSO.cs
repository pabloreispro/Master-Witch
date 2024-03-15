using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SO{
    [CreateAssetMenu(fileName = "FoodSO", menuName = "Game/FoodSO", order = 1)]
    public class FoodSO : ScriptableObject{
        public int foodID;
        public GameObject foodPrefab;

    }
}
