using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public string stringId;
    public float OffsetY = 0f;
    private UITooltip Tooltip = null;
    private int[] Values;
    private IContentDisplayable Content = null;
    private CombatSkill CombatSkill = null;
    private CombatBuff CombatBuff = null;
    private int ManaLeft = 0;
    public bool IsFunctional = true;

    public void SetString(string _stringId, int[] _values = null)
    {
        Values = _values;
        stringId = _stringId;

    }

    public void SetContentCointainer(IContentDisplayable _data)
    {
        Content = _data;
    }

    public void SetCombatSkill(CombatSkill _data, int _manaLeft)
    {
        CombatSkill = _data;
        ManaLeft = _manaLeft;
    }

    public void SetCombatBuff(CombatBuff _data)
    {
        CombatBuff = _data;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsFunctional)
            return;

        //if (!string.IsNullOrEmpty(stringId))
        //{
        if (OffsetY == 0)
            OffsetY = 200;
        if (Tooltip == null)
            Tooltip = UIManager.instance.SpawnTooltip(this.transform, stringId, Content, CombatSkill, CombatBuff, ManaLeft, OffsetY, Values);
        if (Tooltip.StringId != stringId)
            Tooltip = UIManager.instance.SpawnTooltip(this.transform, stringId, Content, CombatSkill, CombatBuff, ManaLeft, OffsetY, Values);
        else Tooltip.Show();
        //}
        //else if (Content != null)
        //{
        //    if (Tooltip == null)
        //        Tooltip = UIManager.instance.SpawnTooltip(this.transform, stringId, Content, Values);
        //    if (Tooltip.StringId != stringId)
        //        Tooltip = UIManager.instance.SpawnTooltip(this.transform, stringId, Content, Values);
        //    else Tooltip.Show();
        //}
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (Tooltip != null)
            Tooltip.Hide();
    }



    //private void Reset()
    //{


    //    // If UI, nothing else needs to be done
    //    if (GetComponent<RectTransform>())
    //        return;

    //    //// If has a collider, nothing else needs to be done
    //    //if (GetComponent<Collider>())
    //    //    return;

    //    //// There were no colliders found when the component is added so we'll add a box collider by default
    //    //// If you are making a 2D game you can change this to a BoxCollider2D for convenience
    //    //// You can obviously still swap it manually in the editor but this should speed up development
    //    //gameObject.AddComponent<BoxCollider2D>();
    //}
}
