using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bench : MonoBehaviour
{
    public static Bench instance;
    public int playerID;
    public BenchType type;
    public Food food;
    bool isPlayer;
    public GameObject foodAsset;
    public float timeProgress=0f;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        Progress();
    }

    public enum BenchType{
        oven,
        stove,
        board
    }

    public void Progress(){
        if(food != null){
            timeProgress += Time.deltaTime;
            OnEndProgress();
        }
    }

    public void AddIngredient(){
        if(Player.instance.id == playerID && food == null){
            food = foodAsset.GetComponent<Food>();
            foodAsset.SetActive(false);
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    public void OnEndProgress(){
        if(timeProgress == food.progress){
            gameObject.transform.GetChild(0).gameObject.SetActive(true);
            foodAsset = null;
            //colocar mudança prefab food
            //ativar foodAsset
        }
    }

}
