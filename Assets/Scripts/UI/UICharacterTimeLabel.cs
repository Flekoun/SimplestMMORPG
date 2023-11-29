using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UICharacterTimeLabel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI TimeText;
    // Start is called before the first frame update
    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }

    public void Start()
    {
        Refresh();
    }
    // Update is called once per frame
    public void Refresh()
    {
        TimeText.SetText(AccountDataSO.CharacterData.currency.time + "/" + AccountDataSO.CharacterData.currency.timeMax);
    }
}
