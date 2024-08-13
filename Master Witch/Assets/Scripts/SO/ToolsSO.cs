using System.Collections;
using System.Collections.Generic;
using Game.SO;
using UnityEngine;

[CreateAssetMenu(fileName = "ToolsSO", menuName = "Game/ToolsSO")]
    public class ToolsSO : ScriptableObject
    {
        public GameObject prefab;
        public BenchType benchType;
    }

