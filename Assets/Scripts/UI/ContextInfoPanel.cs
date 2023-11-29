using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ContextInfoPanel : MonoBehaviour
{

    public UICombatSkillVisuals UICombatSkillVisuals;
    public UIContentContainerDetail UIContentContainerDetail;
    public GameObject Model;
    public LayoutElement Layout;



    public void Show()
    {
        Model.gameObject.SetActive(true);
        Layout.ignoreLayout = false;

        OnShowPanel.Invoke();

    }
    public void Hide()
    {
        UICombatSkillVisuals.gameObject.SetActive(false);
        UIContentContainerDetail.gameObject.SetActive(false);
        Model.gameObject.SetActive(false);
        Layout.ignoreLayout = true;

        OnHidePanel.Invoke();

    }

    public void ShowContextCombatSkill(CombatSkill _data, int _manaLeft)
    {
        UICombatSkillVisuals.gameObject.SetActive(true);
        UICombatSkillVisuals.SetData(_data, _manaLeft);
        this.Show();
    }



    public void ShowContentContainerDetail(IContentDisplayable _data)
    {
        UIContentContainerDetail.gameObject.SetActive(true);
        UIContentContainerDetail.Show(_data);

        this.Show();
    }

    public void HideContentContainerDetail()
    {
        UIContentContainerDetail.Hide();

        this.Hide();
    }


    public UnityEvent OnShowPanel;
    public UnityEvent OnHidePanel;
}
