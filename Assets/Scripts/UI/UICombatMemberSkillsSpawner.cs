using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;

public class UICombatMemberSkillsSpawner : MonoBehaviour
{
    public PrefabFactory PrefabFactory;
    public GameObject UICombatMemberSkillEntryPrefab;
    public List<Transform> Positions = new List<Transform>();
    private const int MAX_SKILLS_COUNT = 6;

    private List<UICombatMemberSkillEntry> UISkillsList = new List<UICombatMemberSkillEntry>();

    public UnityAction<UICombatMemberSkillEntry> OnUICombatMemberSkillEntryClicked;

    public CombatMember Data; 

    //public void Awake()
    //{
    //    AccountDataSO.OnCharacterDataChanged += Refresh;
    //}

    //public void OnDestroy()
    //{
    //    AccountDataSO.OnCharacterDataChanged -= Refresh;
    //}


    public void Show(CombatMember _combatMember, UIEncounterDetailPanel _spawner)
    {
        Data = _combatMember;


        int UISkillsCount = UISkillsList.Count;
        for (int i = 0; i < MAX_SKILLS_COUNT - UISkillsCount; i++)
        {
            UICombatMemberSkillEntry skillUI = PrefabFactory.CreateGameObject<UICombatMemberSkillEntry>(UICombatMemberSkillEntryPrefab, Positions[i]);
            UISkillsList.Add(skillUI);
        }

        foreach (var item in UISkillsList)
        {
            item.gameObject.SetActive(false);
        }

        foreach (var skill in Data.skillsInHand)
        {
            // if (skill.equipSlot != 0)
            //  {
           
            UISkillsList[skill.handSlotIndex].SetData(skill, this);
            UISkillsList[skill.handSlotIndex].gameObject.SetActive(true);
            //  }
        }

    }

    private void Refresh()
    {

    }

    public void SkillClicked(UICombatMemberSkillEntry _skill)
    {
        if (OnUICombatMemberSkillEntryClicked != null)
            OnUICombatMemberSkillEntryClicked.Invoke(_skill);
    }


}
