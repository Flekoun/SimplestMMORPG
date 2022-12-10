using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
public class UIContentContainerDetail : MonoBehaviour
{
    [Header("Button")]
    public bool IsButtonVisible = false;
    public string ButtonText = "Close";
    [Space]
    public GameObject ButtonGO;
    public TextMeshProUGUI ButtonTMPText;
    public ContentFitterRefresh ContentFitterRefresh;
    public UIEquipDetail UIEquipDetail;
    public UIContentDetail UIContentDetail;
    public UIContentFoodDetail UIContentFoodDetail;

    public GameObject ActionButton;

    public UnityAction OnHideClicked;
    public UnityAction OnActionButtonClicked;

    //pridat listenery na vsechny hide z tech kontentu nebo udelat tady jeden hide button pro vsechny kontenty
    //    proste ted se HideButtonClicked nikde nevola
    // Start is called before the first frame update
    void Start()
    {

    }

    public void Awake()
    {
        UIEquipDetail.OnHideClicked += HideButtonClicked;
        UIContentDetail.OnHideClicked += HideButtonClicked;
        UIContentFoodDetail.OnHideClicked += HideButtonClicked;
    }

    public void ShowActionButton(bool _show)
    {
        IsButtonVisible = _show;
    }

    public void SetButtonText(string _text)
    {
        ButtonText = _text;
    }

    public void Show(ContentContainer _contentContainer)
    {
        Show(_contentContainer.GetContent());
    }

    public void Show(Content _content)
    {
        if (_content is Equip)
        {
            UIEquipDetail.Show(_content as Equip);
            UIContentDetail.Hide();
            UIContentFoodDetail.Hide();
        }
        else if (_content is ContentItem )
        {
            UIContentDetail.Show(_content as ContentItem);
            UIEquipDetail.Hide();
            UIContentFoodDetail.Hide();
        }

        else if (_content is ContentCurrency)
        {
            UIContentDetail.Show(_content as ContentCurrency);
            UIEquipDetail.Hide();
            UIContentFoodDetail.Hide();
        }

        else if (_content is ContentFood)
        {
            UIContentFoodDetail.Show(_content as ContentFood);
            UIContentDetail.Hide();
            UIEquipDetail.Hide();
        }

       

        ButtonTMPText.SetText(ButtonText);
        ContentFitterRefresh.RefreshContentFitters();
        ButtonGO.SetActive(IsButtonVisible);
    }

    public void Hide()
    {
        UIEquipDetail.Hide();
        UIContentDetail.Hide();
        UIContentFoodDetail.Hide();
        ButtonGO.SetActive(false);
    }

    public void HideButtonClicked()
    {
        Hide();
        if (OnHideClicked != null)
            OnHideClicked.Invoke();
    }

    public void ActionButtonClicked()
    {
        Hide();
        if (OnActionButtonClicked != null)
            OnActionButtonClicked.Invoke();
    }
}
