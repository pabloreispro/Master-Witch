using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static RecipeCondition;

namespace Game.SO
{
    [CreateAssetMenu(fileName = "ChefSO", menuName = "Game/ChefSO")]
    public class ChefSO : ScriptableObject
    {
        int id;
        [SerializeField] GameObject prefab;
        [SerializeField] ReviewCondition[] conditions;

        public float ReviewRecipe(RecipeData recipe)
        {
            float score = 0;
            foreach (var item in recipe.UtilizedIngredients)
            {
                float modifier = 1;
                for (int i = 0; i < conditions.Length; i++)
                {
                    switch (conditions[i].type)
                    {
                        case ReviewType.FoodPreference:
                            if (conditions[i].foods.Contains(item.TargetFood))
                            {
                                modifier += conditions[i].preferenceModifier;
                                Debug.Log($"Adding {conditions[i].preferenceModifier} modifier by food preference in {item.TargetFood.name}. Total {modifier}");
                            }
                            break;
                        case ReviewType.CategoryPreference:
                            for (int j = 0; j < item.TargetFood.category.Length; j++)
                            {
                                if (item.TargetFood.category[j] == conditions[i].category)
                                {
                                    modifier += conditions[i].categoryModifier;
                                    Debug.Log($"Adding {conditions[i].preferenceModifier} modifier by category preference in {item.TargetFood.name}. Total {modifier}");
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
                score += item.TargetFood.score * modifier;
                Debug.Log($"Adding {item.TargetFood.score * modifier} points to score by {item.TargetFood.name} recipe. Total {score}");
            }
            score += (recipe.TargetFood as RecipeSO).GetScore(recipe.UtilizedIngredients);
            return score;
        }
    }
    [Serializable]
    public class ReviewCondition
    {
        public ReviewType type;
        public List<FoodSO> foods = new List<FoodSO>();
        public float preferenceModifier;
        public Category category;
        public float categoryModifier;
    }
    public enum ReviewType
    {
        FoodPreference,
        CategoryPreference,
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

            switch ((ReviewType)type.intValue)
            {
                case ReviewType.FoodPreference:
                    var preferenceModifier = property.FindPropertyRelative("preferenceModifier");
                    preferenceModifier.floatValue = EditorGUI.FloatField(secondRect, "Preference Modifier", preferenceModifier.floatValue);
                    var food = property.FindPropertyRelative("foods");
                    EditorGUI.LabelField(thirdRect, "Food List");
                    var newPos = EditorGUI.PrefixLabel(thirdRect, GUIUtility.GetControlID(FocusType.Passive), label);
                    EditorGUI.indentLevel++;
                    EditorGUI.PropertyField(newPos, food, true);
                    EditorGUI.indentLevel--;
                    break;
                case ReviewType.CategoryPreference:
                    var category = property.FindPropertyRelative("category");
                    category.intValue = EditorGUI.Popup(secondRect, "Category", category.intValue, category.enumNames);
                    var categoryModifier = property.FindPropertyRelative("categoryModifier");
                    categoryModifier.floatValue = EditorGUI.FloatField(thirdRect, "Category Modifier", categoryModifier.floatValue);
                    break;
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
                case ReviewType.CategoryPreference:
                    return base.GetPropertyHeight(property, label) * 3;
                default:
                    return base.GetPropertyHeight(property, label) * 2;
            }
            //return (20 - EditorGUIUtility.singleLineHeight) + (EditorGUIUtility.singleLineHeight * 2);
        }
    }
#endif
    #endregion

}
