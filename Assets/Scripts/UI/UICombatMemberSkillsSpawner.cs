using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UICombatMemberSkillsSpawner : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public GameObject UICombatMemberSkillEntryPrefab;
    public Transform SkillsParent;
    //  public List<Transform> Positions = new List<Transform>();
    private const int MAX_SKILLS_COUNT = 6;

    private List<UICombatMemberSkillEntry> UISkillsList = new List<UICombatMemberSkillEntry>();

    //public UnityAction<UICombatMemberSkillEntry> OnSkillClicked;
    //public UnityAction<UICombatMemberSkillEntry> OnSkillHoldFinished;
    public UnityAction<UICombatEntity, UICombatMemberSkillEntry> OnSkillDropedOnCombatEntity;
    public UnityAction OnStartDrag;

    public CombatMember Data;



    public void Show(CombatMember _combatMember)
    {
        Data = _combatMember;

        Utils.DestroyAllChildren(SkillsParent);

        foreach (var item in Data.skillsInHand)
        {
            UICombatMemberSkillEntry skillUI = PrefabFactory.CreateGameObject<UICombatMemberSkillEntry>(UICombatMemberSkillEntryPrefab, SkillsParent);
            //skillUI.OnClicked += SkillClicked;
            //skillUI.OnHoldFinished += SkillHoldFinished;
            skillUI.DropedOnCombatEntity += SkillDropedOnCombatEntity;
            //  skillUI.StartDrag += () => { OnStartDrag?.Invoke(); };
            skillUI.SetData(item, Data.stats.mana);
            UISkillsList.Add(skillUI);
        }



    }

    private void Refresh()
    {

    }

    //public void SkillClicked(UICombatMemberSkillEntry _skill)
    //{
    //    if (OnSkillClicked != null)
    //        OnSkillClicked.Invoke(_skill);
    //}

    //public void SkillHoldFinished(UICombatMemberSkillEntry _skill)
    //{
    //    if (OnSkillHoldFinished != null)
    //        OnSkillHoldFinished.Invoke(_skill);
    //}

    public void SkillDropedOnCombatEntity(UICombatEntity _combatEntity, UICombatMemberSkillEntry _skill)
    {

        OnSkillDropedOnCombatEntity?.Invoke(_combatEntity, _skill);
    }


}
