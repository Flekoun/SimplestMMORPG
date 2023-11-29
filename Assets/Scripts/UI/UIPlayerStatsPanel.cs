using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPlayerStatsPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI UidText;
    public TextMeshProUGUI SatoshiumText;
    public TextMeshProUGUI SatoshiText;
    public TextMeshProUGUI ReputationText;
    public TextMeshProUGUI TrainingPointsText;
    public TextMeshProUGUI SatoshiumExchangeRate;
    public TextMeshProUGUI BitcoinPriceText;
    public UIInventory UIInventory;

    public void Awake()
    {
        AccountDataSO.OnPlayerDataChanged += Refresh;

    }


    public void OnEnable()
    {
        Refresh();
    }

    // Start is called before the first frame update
    public void Refresh()
    {
        UIInventory.Refresh(AccountDataSO.PlayerData.inventory.content);

        UidText.SetText("Uid:" + AccountDataSO.PlayerData.uid);
        PlayerNameText.SetText(AccountDataSO.PlayerData.playerName);
        //   SatoshiumText.SetText(AccountDataSO.PlayerData.currencies.satoshium.ToString());
        SatoshiText.SetText(AccountDataSO.PlayerData.satoshi.ToString());
        ReputationText.SetText(AccountDataSO.PlayerData.reputation.ToString());
        SatoshiumExchangeRate.SetText("1 Satoshium = " + (Utils.RoundToInt((float)AccountDataSO.GlobalMetadata.SATOSHIUM_SATS_ExchangeRate)).ToString() + " Satoshi");
        BitcoinPriceText.SetText("1BTC ~ " + AccountDataSO.GlobalMetadata.BTC_USD_ExchangeRate + "$");
        //    TrainingPointsText.SetText(AccountDataSO.PlayerData.currencies.trainingPoints.ToString());
    }

    public void ConvertSatoshiumToSatoshiClicked()
    {
        UIManager.instance.SpawnPromptPanel("Here, you will be able to transform your Satoshium into Bitcoin, leveraging real-time exchange rates that reflect the dynamic market value of Bitcoin. Please note, this process is irreversible. Once you've converted your Satoshium to Bitcoin, you'll have the option to seamlessly withdraw it to your personal wallet.", null, null);
    }

    public void WithdrawSatoshiClicked()
    {
        UIManager.instance.SpawnPromptPanel("Here you will be able to withdraw your Bitcoin directly into your wallet. For automatic withdrawals, we seamlessly integrate with your ZEBEDEE wallet linked to your game account, ensuring a smooth transaction. If you prefer, you also have the flexibility to manually request a withdrawal to any Bitcoin address of your choosing.", null, null);
    }
}
