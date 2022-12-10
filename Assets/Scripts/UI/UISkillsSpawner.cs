using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISkillsSpawner : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public GameObject UISkillPrefab;
    public List<Transform> Positions = new List<Transform>();
    private const int MAX_SKILLS_COUNT = 4;
    //public UIEncounterDetail SelectedEncounter;

    private List<UISkill> UISkillsList = new List<UISkill>();

    public void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDestroy()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }


    void Refresh()
    {
        Debug.Log("character data changed");


      

        int UISkillsCount = UISkillsList.Count;
        for (int i = 0; i < MAX_SKILLS_COUNT - UISkillsCount; i++)
        {
            UISkill skillUI = PrefabFactory.CreateGameObject<UISkill>(UISkillPrefab, Positions[i]);
            //    skillUI.SetData(AccountDataSO.CharacterData.skills[i]);
            UISkillsList.Add(skillUI);
        }

        foreach (var item in UISkillsList)
        {
            item.gameObject.SetActive(false);
        }
      

        //foreach (var equip in AccountDataSO.CharacterData.equipment)
        //{
        //    if (equip.equip.skill.equipSlot != 0)
        //    {
        //        UISkillsList[equip.equip.skill.equipSlot - 1].SetData(equip.equip.skill);
        //        UISkillsList[equip.equip.skill.equipSlot - 1].gameObject.SetActive(true);
        //    }
        //}


     


    }
}
