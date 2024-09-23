using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.SO{
    [CreateAssetMenu(fileName = "FoodSO", menuName = "Game/FoodSO")]
    public class FoodSO : ScriptableObject{
        public int foodID;
        public Category[] category;
        public GameObject foodPrefab;
        public Sprite imageFood;
        public float timeProgress;
        [SerializeField] FoodModifiers modifiers;
        public FoodModifiers Modifiers => modifiers;
    }

    [Serializable]
    public class FoodModifiers
    {
        const float MIN_MODIFIER_VALUE = -2;
        const float MAX_MODIFIER_VALUE = 2;
        [SerializeField][Range(MIN_MODIFIER_VALUE, MAX_MODIFIER_VALUE)] float igneousValue;
        [SerializeField][Range(MIN_MODIFIER_VALUE, MAX_MODIFIER_VALUE)] float poisonousValue;
        [SerializeField][Range(MIN_MODIFIER_VALUE, MAX_MODIFIER_VALUE)] float curativeValue;
        public float IgneousValue => igneousValue;
        public float PoisonousValue => poisonousValue;
        public float CurativeValue => curativeValue;

        public FoodModifiers(float igneous, float poisonous, float curative)
        {
            igneousValue = igneous;
            poisonousValue = poisonous;
            curativeValue = curative;
        }
    }
}
