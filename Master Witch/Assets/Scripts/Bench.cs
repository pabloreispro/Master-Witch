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
    float timeProgress=0f;

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
        if(isPlayer && food == null){
            food = foodAsset.GetComponent<Food>();
            foodAsset.SetActive(false);
        }
    }

    public void OnEndProgress(){
        if(timeProgress == food.progress){
            //colocar mudança prefab food
            //ativar foodAsset
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player"){
            if(Player.instance.id == playerID){
                isPlayer = true;
                foodAsset = other.gameObject.transform.GetChild(0).gameObject;
            }else{
                isPlayer = false;
            }
        }
    }

}
