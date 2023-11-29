using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using simplestmmorpg.data;

public class UIFoodEffectSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public GameObject FoodEffectPrefab;
    public Transform Parent;

    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterDataChanged += Refresh;
    //}

    public void Refresh()
    {
        Utils.DestroyAllChildren(Parent);

        foreach (var foodEffect in AccountDataSO.CharacterData.foodEffects)
        {
            var effectUI = PrefabFactory.CreateGameObject<UIFoodEffect>(FoodEffectPrefab, Parent);
            effectUI.Setup(foodEffect);
        }

    }
}
