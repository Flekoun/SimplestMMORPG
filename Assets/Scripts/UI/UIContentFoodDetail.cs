using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIContentFoodDetail : MonoBehaviour
{
    //  public ImageIdDefinitionSOSet AllImageIdDefinitionSOSet;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UIContentDetail UIContentDetail;
    public TextMeshProUGUI DescText;
    public UnityAction OnHideClicked;
    //   public TextMeshProUGUI TimeBonusText;
    public GameObject Model;

    private ContentFood Data;


    public void Awake()
    {
        UIContentDetail.OnHideClicked += HideClicked;
    }

    public void Show(ContentFood _data)
    {
        Data = _data;
        UIContentDetail.Show(Data);
        if (Data.timeBonus > 0 && Data.fatigueRecoveryBonus > 0)
            DescText.SetText("Recovers <color=\"orange\">" + Data.fatigueRecoveryBonus + "%</color> fatigue and grants <color=\"orange\">" + Data.timeBonus + "</color> travel hours when eaten");
        else if (Data.fatigueRecoveryBonus > 0)
            DescText.SetText("Recovers <color=\"orange\">" + Data.fatigueRecoveryBonus + "%</color> fatigue when eaten");
        else if (Data.timeBonus > 0)
            DescText.SetText("Grants <color=\"orange\">" + Data.timeBonus + "%</color> travel hours when eaten");

        Model.gameObject.SetActive(true);
    }

    public void Hide()
    {
        Model.gameObject.SetActive(false);
    }

    private void HideClicked()
    {
        if (OnHideClicked != null)
            OnHideClicked.Invoke();
    }


    public void ConsumeClicked()
    {
        FirebaseCloudFunctionSO.EatFood(Data.uid);
    }
}
