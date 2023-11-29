using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static UnityEngine.Rendering.DebugUI;
using RoboRyanTron.Unite2017.Variables;

public class UIGoldAmountInput : MonoBehaviour
{
    //public GameObject Gold_GO;
    //public GameObject Silver_GO;
    //public GameObject Bronze_GO;
    public TMP_InputField GoldInput;
    public TMP_InputField SilverInput;
    public TMP_InputField BronzeInput;

    // public bool ShowAsCharacterGold = false;


    public int GetAmount()
    {
        int gold = 0;
        int silver = 0;
        int bronze = 0;

        int.TryParse(GoldInput.text, out gold);
        int.TryParse(SilverInput.text, out silver);
        int.TryParse(BronzeInput.text, out bronze);

        gold *= 10000;
        silver *= 100;


        return bronze + silver + gold;
        //Gold_GO.SetActive(gold > 0);
        //Silver_GO.SetActive(silver > 0);
        //Bronze_GO.SetActive(bronze > 0);

        //GoldText.SetText(gold.ToString());
        //SilverText.SetText(silver.ToString());
        //BronzeText.SetText(bronze.ToString());

    }

    // Start is called before the first frame update
    //void OnEnable()
    //{
    //    if (ShowAsCharacterGold)
    //    {

    //        RefreshText();
    //        CharacterGold.ListenOnChangeEvent(RefreshText);
    //    }

    //}

    //private void OnDisable()
    //{
    //    if (ShowAsCharacterGold)

    //        CharacterGold.UnlistenOnChangeEvent(RefreshText);
    //}

    //private void RefreshText()
    //{
    //    SetPrice(CharacterGold.Value);
    //}


}
