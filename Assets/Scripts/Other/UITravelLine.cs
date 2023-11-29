using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UITravelLine : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public TextMeshProUGUI TimePriceText;
    public TextMeshProUGUI TravelPointsPriceText;
    public GameObject TravelPricesGO;
    public GameObject TravelPointsPriceGO;
    public GameObject TimePriceGO;
    public void Setup(int _timeWeight)
    {
        if (AccountDataSO == null)
            return;

        if (TravelPricesGO != null)
            TravelPricesGO.SetActive(false);


        //bool enoughTravelPoints = AccountDataSO.CharacterData.currency.travelPoints >= _timeWeight;

        //if (TravelPricesGO != null)
        //    TravelPricesGO.SetActive(_timeWeight > 1 || !enoughTravelPoints);

        //if (TimePriceGO != null)
        //    TimePriceGO.SetActive(false);

        //if (TravelPointsPriceGO != null)
        //    TravelPointsPriceGO.SetActive(false);

        //if (enoughTravelPoints)
        //{
        //    TravelPointsPriceGO.SetActive(true);

        //    TravelPointsPriceText.SetText(_timeWeight.ToString());
        //}
        //else
        //{
        //    int priceInTravelPoints = Mathf.FloorToInt(AccountDataSO.CharacterData.currency.travelPoints) * AccountDataSO.OtherMetadataData.constants.timePerTravelPoint;
        //    int lefotverPriceInTime = (_timeWeight * AccountDataSO.OtherMetadataData.constants.timePerTravelPoint) - priceInTravelPoints;

        //    TravelPointsPriceGO.SetActive(priceInTravelPoints > 0);
        //    TimePriceGO.SetActive(lefotverPriceInTime > 0);

        //    TravelPointsPriceText.SetText((priceInTravelPoints/ AccountDataSO.OtherMetadataData.constants.timePerTravelPoint).ToString());
        //    TimePriceText.SetText(lefotverPriceInTime.ToString());

        //}

    }
}
