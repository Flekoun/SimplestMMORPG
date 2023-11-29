using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using simplestmmorpg.data;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICombatEntity : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public GameObject BuffPrefab;
    public Transform BuffsList;
    public GameObject DeadIndicator;
    public UIPortrait Portrait;
    public TextMeshProUGUI NameText;
    public UIProgressBar HealthProgressBar;
    public GameObject BlockGO;
    public TextMeshProUGUI BlockAmountText;

    public GameObject DropZoneGO;

    public Image HitEffectImage;
    public Transform GetHitShakeEffectTarget;
    //public GameObject SelectedImage;
    public FloatingTextSpawner FloatingTextSpawner;
    public TweenEffects TweenEffects;

    public Transform FloatingTextsParent;

    public List<DOTweenAnimation> HitAnim;


    public TextMeshProUGUI LevelText;
    public CombatEntity Data;
    protected CombatEntity OldData;
    public UnityAction<UICombatEntity> OnClicked;
    public UnityAction<UIBuff, UICombatEntity> OnBuffClicked;
    private string oldUid;

    public bool IsValidTargetForSkill(CombatSkill _skill)
    {
        if (this is UICombatEnemy)
        {
            if (!_skill.validTarget_AnyEnemy)
            {
                return false;
            }
        }
        else if (this is UICombatMember)
        {
            if (!_skill.validTarget_AnyAlly && Data.uid != AccountDataSO.CharacterData.uid) //jsem ally ale anyally je false
            {
                return false;
            }
            else if (!_skill.validTarget_Self && Data.uid == AccountDataSO.CharacterData.uid) //jsem hrac ale valid target self je false
            {
                return false;
            }
        }

        return true;
    }
    public void ShowDropZone(GameObject _dragAndDropObject)
    {

        UICombatMemberSkillEntry skillEntry = _dragAndDropObject.GetComponent<UICombatMemberSkillEntry>();

        DropZoneGO.gameObject.SetActive(IsValidTargetForSkill(skillEntry.Data));

        //if (this is UICombatEnemy)
        //{
        //    if (!skillEntry.Data.validTarget_AnyEnemy)
        //    {
        //        DropZoneGO.gameObject.SetActive(false);
        //        return;
        //    }
        //}
        //else if (this is UICombatMember)
        //{
        //    if (!skillEntry.Data.validTarget_AnyAlly)
        //    {
        //        DropZoneGO.gameObject.SetActive(false);
        //        return;
        //    }
        //    else if (!skillEntry.Data.validTarget_Self && Data.uid != AccountDataSO.CharacterData.uid)
        //    {
        //        DropZoneGO.gameObject.SetActive(false);
        //        return;
        //    }
        //}

        //DropZoneGO.gameObject.SetActive(true);
    }

    public void HideDropZone(GameObject _dragAndDropObject)
    {
        DropZoneGO.gameObject.SetActive(false);
    }

    public void ShowDropZoneAsActive()
    {
        DropZoneGO.GetComponent<Image>().color = Color.green;
    }

    public void ShowDropZoneAsInActive()
    {
        DropZoneGO.GetComponent<Image>().color = Color.white;
    }

    public void SpawnFloatingTexts(CombatFlowEntry _flowData)
    {
        if (_flowData.isSpecialEffect)
        {
            if (_flowData.isPositive)
            {
                FloatingTextSpawner.Spawn(_flowData.amount.ToString() + " " + Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata("{" + _flowData.effectId + "}"), Color.green, FloatingTextsParent);
                TweenEffects.HitEffectTween(HitEffectImage, Color.green);
            }
            else
            {
                FloatingTextSpawner.Spawn(_flowData.amount.ToString() + " " + Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata("{" + _flowData.effectId + "}"), Color.red, FloatingTextsParent);
                TweenEffects.HitEffectTween(HitEffectImage, Color.red);

            }
        }
        else if (_flowData.amount > 0)
        {
            if (_flowData.isPositive)
            {
                FloatingTextSpawner.Spawn(_flowData.amount.ToString(), Color.green, FloatingTextsParent);
                TweenEffects.HitEffectTween(HitEffectImage, Color.green);
            }
            else
            {
                FloatingTextSpawner.Spawn("-" + _flowData.amount.ToString(), Color.red, FloatingTextsParent);
                TweenEffects.HitEffectTween(HitEffectImage, Color.red);
                TweenEffects.ShakeEffectTween(GetHitShakeEffectTarget);
            }
        }
        else if (_flowData.amount == 0)
        {
            FloatingTextSpawner.Spawn("Blocked", Color.white, FloatingTextsParent);
            TweenEffects.HitEffectTween(HitEffectImage, Color.white);
        }

        //if (Data.stats.health < OldData.stats.health)
        //{
        //    FloatingTextSpawner.Spawn("-" + (OldData.stats.health - Data.stats.health).ToString(), Color.red, FloatingTextsParent);
        //    TweenEffects.HitEffectTween(HitEffectImage, Color.red);
        //}

        //if (Data.stats.health > OldData.stats.health)
        //{
        //    FloatingTextSpawner.Spawn((Data.stats.health - OldData.stats.health).ToString(), Color.green, FloatingTextsParent);
        //    TweenEffects.HitEffectTween(HitEffectImage, Color.green);
        //}
    }

    public void ShowHitEffect()
    {
        foreach (var item in HitAnim)
        {
            item.DORestart();
        }
    }


    public virtual void SetData(CombatEntity _data, EncounterData _encounter, bool _initSetup)
    {

        //        Debug.Log("Nastavujue : " + _data.GetDisplayName() + "intiSetup je : " + _initSetup);
        OldData = Data;
        Data = _data;

        if (OldData == null) //pokud jeste nebyl nataven, tak aby old data nebyly null, tak = datum
        {
            OldData = Data;
        }


        if (_initSetup) //zmenil se entity, takze nejaky novy, inicializuju ho poprve
        {
            HealthProgressBar.SetValues(Data.stats.healthMax, Data.stats.health, Data.stats.healthFatiguePenalty, true);
        }
        else
            HealthProgressBar.SetValues(Data.stats.healthMax, Data.stats.health, Data.stats.healthFatiguePenalty, false);

        // HealthProgressBar.SetPenaltyValue(Data.stats.healthFatiguePenalty);
        HealthProgressBar.SetLeastValueImage(Data.stats.leastHealth);

        BlockGO.SetActive(Data.blockAmount > 0);
        BlockAmountText.SetText(Data.blockAmount.ToString());
        NameText.SetText(Data.GetDisplayName());
        LevelText.SetText(Data.level.ToString());
        Portrait.SetPortrait(Data.GetPortraitId(), Data.GetCharacterClassId());
        DeadIndicator.SetActive(Data.stats.health <= 0);
        Utils.DestroyAllChildren(BuffsList);


        foreach (var item in Data.buffs)
        {
            var buff = PrefabFactory.CreateGameObject<UIBuff>(BuffPrefab, BuffsList);
            buff.Setup(item);
            //  buff.OnClicked += BuffClicked;

        }

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
            FloatingTextSpawner.Spawn(Utils.DescriptionsMetadata.GetSkillMetadata(item.buffId).title.GetText() + " expired", Color.gray, FloatingTextsParent);
        }

        foreach (var item in newBuffs)
        {
            FloatingTextSpawner.Spawn(Utils.DescriptionsMetadata.GetSkillMetadata(item.buffId).title.GetText(), Color.white, FloatingTextsParent);
        }

        oldUid = Data.uid;
    }

    //public void BuffClicked(UIBuff _buff)
    //{
    //    OnBuffClicked.Invoke(_buff, this);
    //}

    public void Clicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }

    //public void SetAsSelected(bool _selected)
    //{
    //    SelectedImage.gameObject.SetActive(_selected);
    //}

    //public override string GetUid()
    //{
    //    return Data.uid;
    //}
}
