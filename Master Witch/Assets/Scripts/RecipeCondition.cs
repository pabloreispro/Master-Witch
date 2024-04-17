using UnityEngine;
using Game.SO;
using static RecipeCondition;
using System.Collections.Generic;
using UnityEditor;
using Unity.Collections.LowLevel.Unsafe;

[System.Serializable]
public class RecipeCondition 
{
    public ConditionType type;
    public BenchType benchType;
    public List<FoodSO> foods = new List<FoodSO>();
    public Category category;
    public float categoryPoints;
    [Tooltip("False if it's not allowed, true if it's Obrigatory")] public bool isAllowed;

    public bool CheckCondition(List<FoodSO> ingredients, BenchType bench)
    {
        switch (type)
        {
            case ConditionType.BenchType:
                return bench == benchType;
            case ConditionType.Food:
                if (isAllowed)
                {
                    foreach (var item in foods)
                    {
                        if (!ingredients.Contains(item))
                        {
                            Debug.Log($"Doesn't have food Item ({item}) that is obrigatory");
                            return false;
                        }
                    }
                }
                else 
                {
                    foreach (var item in ingredients)
                    {
                        if (foods.Contains(item))
                        {
                            Debug.Log($"Have Food Item ({item}) that is not allowed ");
                            return false;
                        }
                    }
                }
                break;
            case ConditionType.Category:
                bool hasCategory = false;
                foreach (var item in ingredients)
                {
                    for (int i = 0; i < item.category.Length; i++)
                    {
                        if (item.category[i] == category)
                        {
                            hasCategory = true;
                            break;
                        }
                    }
                }
                if (isAllowed)
                {
                    Debug.Log($"{category} Category is obrigatory. Have? {hasCategory}");
                    return hasCategory;
                }
                else
                {
                    Debug.Log($"{category} Category is not allowed. Have? {hasCategory}");
                    return !hasCategory;
                }
        }
        return true;
    }

    public enum ConditionType
    {
        BenchType,
        Food,
        Category
    }
}
#region Editor
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(RecipeCondition))]
class RecipeConditionPropertyDrawer : PropertyDrawer
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
        var isAllowed = property.FindPropertyRelative("isAllowed");

        type.intValue = EditorGUI.Popup(typeRect, "Type", type.intValue, type.enumNames);

        switch ((ConditionType)type.intValue)
        {
            case ConditionType.BenchType:
                var benchType = property.FindPropertyRelative("benchType");
                benchType.intValue = EditorGUI.Popup(secondRect, "Bench Type", benchType.intValue, benchType.enumNames);
                break;
            case ConditionType.Food:
                var food = property.FindPropertyRelative("foods");
                isAllowed.boolValue = EditorGUI.Toggle(secondRect, isAllowed.boolValue ? "Obrigatory" : "Not Allowed", isAllowed.boolValue);
                EditorGUI.LabelField(thirdRect, "Food List");
                var newPos = EditorGUI.PrefixLabel(thirdRect, GUIUtility.GetControlID(FocusType.Passive), label);
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(newPos, food, true);
                EditorGUI.indentLevel--;
                break;
            case ConditionType.Category:
                var category = property.FindPropertyRelative("category");
                category.intValue = EditorGUI.Popup(secondRect, "Category", category.intValue, category.enumNames);
                isAllowed.boolValue = EditorGUI.Toggle(thirdRect, isAllowed.boolValue ? "Obrigatory" : "Not Allowed", isAllowed.boolValue);
                if (isAllowed.boolValue)
                {
                    var categoryPoints = property.FindPropertyRelative("categoryPoints");
                    categoryPoints.floatValue = EditorGUI.FloatField(fourthRect, "Category Points", categoryPoints.floatValue);
                }
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
        switch ((ConditionType)type.intValue)
        {
            case ConditionType.Food:
                SerializedProperty foods = property.FindPropertyRelative("foods");
                if (foods.isExpanded)
                {
                    return EditorGUI.GetPropertyHeight(foods, true) + EditorGUIUtility.singleLineHeight * 3;
                }
                return base.GetPropertyHeight(property, label) * 3;
            case ConditionType.Category:
                var isAllowed = property.FindPropertyRelative("isAllowed");
                if (isAllowed.boolValue)
                    return base.GetPropertyHeight(property, label) * 4;
                else
                    return base.GetPropertyHeight(property, label) * 3;
            default:
                return base.GetPropertyHeight(property, label) * 2;
        }
        //return (20 - EditorGUIUtility.singleLineHeight) + (EditorGUIUtility.singleLineHeight * 2);
    }
}
#endif
#endregion
