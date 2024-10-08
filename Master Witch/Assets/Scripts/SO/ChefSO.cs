using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Game.SO
{
    [CreateAssetMenu(fileName = "ChefSO", menuName = "Game/ChefSO")]
    public class ChefSO : ScriptableObject
    {
        public const float BASE_RECIPE_SCORE = 70;
        const float FOOD_MATCH_SCORE = 10;
        const float MODIFIER_MATCH_SCORE = 2.5f;
        int id;
        public GameObject prefab;
        public FoodModifiers foodModifiers;
        public ReviewCondition[] conditions; 

        public float ReviewRecipe(RecipeData recipe)
        {
            float score = recipe.TargetFood == GameManager.Instance.TargetRecipe ? BASE_RECIPE_SCORE : 0;
            score += recipe.CalculateScore((food) =>
            {
                float foodScore = 0;
                for (int i = 0; i < conditions.Length; i++)
                {
                    switch (conditions[i].type)
                    {
                        case ReviewType.FoodPreference:
                            if (conditions[i].foods.Contains(food))
                            {
                                if (conditions[i].isAllowed)
                                {
                                    foodScore += FOOD_MATCH_SCORE;
                                    Debug.Log($"Adding {FOOD_MATCH_SCORE} score by food preference in {food.name}. Total {foodScore}");
                                }
                                else
                                {
                                    foodScore -= FOOD_MATCH_SCORE;
                                    Debug.Log($"Removing {FOOD_MATCH_SCORE} score by food preference in {food.name}. Total {foodScore}");
                                }
                            }
                            break;
                        //case ReviewType.CategoryPreference:
                        //for (int j = 0; j < food.category.Length; j++)
                        //{
                        //    if (food.category[j] == conditions[i].category)
                        //    {
                        //        if (conditions[i].isAllowed)
                        //        {
                        //            foodScore += CATEGORY_MATCH_SCORE;
                        //            Debug.Log($"Adding {CATEGORY_MATCH_SCORE} score by category preference in {food.name}. Total {foodScore}");
                        //        }
                        //        else
                        //        {
                        //            foodScore -= CATEGORY_MATCH_SCORE;
                        //            Debug.Log($"Removing {CATEGORY_MATCH_SCORE} score by category preference in {food.name}. Total {foodScore}");
                        //        }
                        //    }
                        //}
                        //    break;
                        default:
                            break;
                    }
                }
                Debug.Log($"Returning {food.name} score. Total {foodScore}");
                return foodScore;

            });
            var igneousScore = recipe.FoodModifiers.IgneousValue * foodModifiers.IgneousValue * MODIFIER_MATCH_SCORE;
            score += igneousScore;
            Debug.Log($"Adding {igneousScore} to {recipe.TargetFood.name}, chef modifier: {foodModifiers.IgneousValue}, recipe modifier: {recipe.FoodModifiers.IgneousValue}");
            var poisonousScore = recipe.FoodModifiers.PoisonousValue * foodModifiers.PoisonousValue * MODIFIER_MATCH_SCORE;
            score += poisonousScore;
            Debug.Log($"Adding {poisonousScore} to {recipe.TargetFood.name}, chef modifier: {foodModifiers.PoisonousValue}, recipe modifier: {recipe.FoodModifiers.PoisonousValue}");
            var curativeScore = recipe.FoodModifiers.CurativeValue * foodModifiers.CurativeValue * MODIFIER_MATCH_SCORE;
            score += curativeScore;
            Debug.Log($"Adding {curativeScore} to {recipe.TargetFood.name}, chef modifier: {foodModifiers.CurativeValue}, recipe modifier: {recipe.FoodModifiers.CurativeValue}");

            //score += (recipe.TargetFood as RecipeSO).GetScore(recipe.UtilizedIngredients);
            return score;
        }
    }
    [Serializable]
    public class ReviewCondition
    {
        public ReviewType type;
        public bool isAllowed;
        public List<FoodSO> foods = new List<FoodSO>();
        //public Category category;
    }
    public enum ReviewType
    {
        FoodPreference,
        //CategoryPreference,
    }
    #region Editor
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReviewCondition))]
    class ReviewConditionPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            var typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var secondRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, position.width, EditorGUIUtility.singleLineHeight);
            var thirdRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 2, position.width, EditorGUIUtility.singleLineHeight);
            var fourthRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight * 3, position.width, EditorGUIUtility.singleLineHeight);

            var type = property.FindPropertyRelative("type");

            type.intValue = EditorGUI.Popup(typeRect, "Type", type.intValue, type.enumNames);
            var isAllowed = property.FindPropertyRelative("isAllowed");
            switch ((ReviewType)type.intValue)
            {
                case ReviewType.FoodPreference:
                    isAllowed.boolValue = EditorGUI.Toggle(secondRect, "Is Allowed", isAllowed.boolValue);
                    var food = property.FindPropertyRelative("foods");
                    EditorGUI.LabelField(thirdRect, "Food List");
                    var newPos = EditorGUI.PrefixLabel(thirdRect, GUIUtility.GetControlID(FocusType.Passive), label);
                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(newPos, food, true);
                    EditorGUI.indentLevel--;
                    break;
                //case ReviewType.CategoryPreference:
                //    isAllowed.boolValue = EditorGUI.Toggle(secondRect, "Is Allowed", isAllowed.boolValue);
                //    var category = property.FindPropertyRelative("category");
                //    category.intValue = EditorGUI.Popup(thirdRect, "Category", category.intValue, category.enumNames);
                //    break;
                default:
                    break;
            }

            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var type = property.FindPropertyRelative("type");
            switch ((ReviewType)type.intValue)
            {
                case ReviewType.FoodPreference:
                    SerializedProperty foods = property.FindPropertyRelative("foods");
                    if (foods.isExpanded)
                    {
                        return EditorGUI.GetPropertyHeight(foods, true) + EditorGUIUtility.singleLineHeight * 3;
                    }
                    return base.GetPropertyHeight(property, label) * 3;
                //case ReviewType.CategoryPreference:
                //    return base.GetPropertyHeight(property, label) * 3;
                default:
                    return base.GetPropertyHeight(property, label) * 2;
            }
            //return (20 - EditorGUIUtility.singleLineHeight) + (EditorGUIUtility.singleLineHeight * 2);
        }
    }
#endif
    #endregion

}
