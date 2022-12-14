using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICombatEntity : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public GameObject BuffPrefab;
    public Transform BuffsList;
    public GameObject DeadIndicator;
    public UIPortrait Portrait;
    public TextMeshProUGUI NameText;
    public UIProgressBar HealthProgressBar;

    public GameObject SelectedImage;
    public FloatingTextSpawner FloatingTextSpawner;
    public Transform FloatingTextsParent;

    public TextMeshProUGUI LevelText;
    public CombatEntity Data;
    protected CombatEntity OldData;
    public UnityAction<UICombatEntity> OnClicked;



    public virtual void SetData(CombatEntity _data, EncounterData _encounter)
    {

        OldData = Data;
        Data = _data;

        if (OldData == null) //pokud je to prvni inicializace
        {
//            Debug.Log("PRVNI!!");
            OldData = Data;
        }


        NameText.SetText(Data.GetDisplayName());
        HealthProgressBar.SetValues(Data.stats.healthMax, Data.stats.health);
        LevelText.SetText(Data.level.ToString());
        Portrait.SetPortrait(Data.GetPortraitId());
        DeadIndicator.SetActive(Data.stats.health <= 0);
        Utils.DestroyAllChildren(BuffsList);


        foreach (var item in Data.buffs)
        {
            var buff = PrefabFactory.CreateGameObject<UIBuff>(BuffPrefab, BuffsList);
            buff.Setup(item);
        }

       // Debug.Log("Data.stats.health : " + Data.stats.health);
       // Debug.Log("OldData.stats.health : " + OldData.stats.health);
        if (Data.stats.health < OldData.stats.health)
        {
            Debug.Log("HEALTH TAKEN EFFECT!: " + this.transform.name);
            FloatingTextSpawner.Spawn("-" + (OldData.stats.health - Data.stats.health).ToString(), Color.red, FloatingTextsParent);
        }

        if (Data.stats.health > OldData.stats.health)
            FloatingTextSpawner.Spawn((Data.stats.health - OldData.stats.health).ToString(), Color.green, FloatingTextsParent);

      
        List<CombatBuff> newBuffs = new List<CombatBuff>();
        List<CombatBuff> expiredBuffs = new List<CombatBuff>();


        foreach (var newbuff in Data.buffs)
        {
            bool isNewBuff = true;
            foreach (var oldBuff in OldData.buffs)
            {
                if (oldBuff.buffId == newbuff.buffId)
                    isNewBuff = false;
            }

            if (isNewBuff)
                newBuffs.Add(newbuff);
        }


        foreach (var oldBuff in OldData.buffs)
        {
            bool isExpiredBuff = true;
            foreach (var newBuff in Data.buffs)
            {
                if (oldBuff.buffId == newBuff.buffId)
                    isExpiredBuff = false;
            }

            if (isExpiredBuff)
                expiredBuffs.Add(oldBuff);
        }



        foreach (var item in expiredBuffs)
        {
            FloatingTextSpawner.Spawn(item.buffId.ToString() + " expired", Color.gray, FloatingTextsParent);
        }

        foreach (var item in newBuffs)
        {
            FloatingTextSpawner.Spawn(item.buffId.ToString(), Color.white, FloatingTextsParent);
        }

    }

    public void Clicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }

    public void SetAsSelected(bool _selected)
    {
        SelectedImage.gameObject.SetActive(_selected);
    }


}
