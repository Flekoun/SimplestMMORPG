using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIInterestMapButton : MonoBehaviour
{
    public Button Button;
    public Image Portrait;
    public GameObject FrontDisableImageGO;
    public TextMeshProUGUI Text;

    public void SetPortrait(Sprite _sprite)
    {
        Portrait.sprite = _sprite;
    }
    public void SetEnabled(bool _enabled)
    {
        Button.interactable = _enabled;
        FrontDisableImageGO.SetActive(!_enabled);

        if (_enabled)
            Text.color = Color.white;
        else
            Text.color = Color.gray;


    }
}
