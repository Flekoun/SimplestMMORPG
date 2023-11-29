using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using RoboRyanTron.Unite2017.Variables;

public class UIPriceLabel : MonoBehaviour
{
    public IntVariable CharacterGold;
    public GameObject Gold_GO;
    public GameObject Silver_GO;
    public GameObject Bronze_GO;
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI SilverText;
    public TextMeshProUGUI BronzeText;

    //   public bool ColorizeTextByBalance = false;
    public bool ShowAsCharacterGold = false;



    public void SetPrice(int _amount)
    {
        int gold = _amount / 10000;
        int silver = (_amount % 10000) / 100;
        int bronze = _amount % 100;

        Gold_GO.SetActive(gold != 0);
        Silver_GO.SetActive(silver != 0);
        Bronze_GO.SetActive(bronze != 0);

        GoldText.SetText(gold.ToString());
        SilverText.SetText(silver.ToString());
        BronzeText.SetText(bronze.ToString());


    }

    public void SetColor(Color _color)
    {
        GoldText.color = _color;
        SilverText.color = _color;
        BronzeText.color = _color;
    }

    // Start is called before the first frame update
    void OnEnable()
    {
        if (ShowAsCharacterGold)
        {

            RefreshText();
            CharacterGold.ListenOnChangeEvent(RefreshText);
        }

    }

    private void OnDisable()
    {
        if (ShowAsCharacterGold)

            CharacterGold.UnlistenOnChangeEvent(RefreshText);
    }

    private void RefreshText()
    {
        SetPrice(CharacterGold.Value);
    }


}
