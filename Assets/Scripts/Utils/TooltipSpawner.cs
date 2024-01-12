using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipSpawner : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
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
    public bool SpawnAtBottom = false;

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
        //if (OffsetY == 0)
        //    OffsetY = 200;
        if (Tooltip == null)
            Tooltip = UIManager.instance.SpawnTooltip(this.transform, stringId, Content, CombatSkill, CombatBuff, ManaLeft, OffsetY, SpawnAtBottom, Values);
        if (Tooltip.StringId != stringId)
            Tooltip = UIManager.instance.SpawnTooltip(this.transform, stringId, Content, CombatSkill, CombatBuff, ManaLeft, OffsetY, SpawnAtBottom, Values);
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

    public void OnDrag(PointerEventData eventData)
    {
        //MUSI TO TADY BYT . BEZ dragg eventu detekce se pri dragovani firuje OnPointerUp z nejakeho duvodu..
    }


}
