using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;

public class UIPendingRewardPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public AccountDataSO AccountDataSO;
    public PrefabFactory PrefabFactory;
    public GameObject PerkPrefab;
    public GameObject ClaimAllButtonGO;
    public Button ClaimAllButton;
    public UIPriceScavengePointsLabel ClaimAllUIPriceScavengePointsLabel;
    public UIPriceTimeLabel UIPriceScavengeTimePrice;
    public Transform Parent;
    public GameObject Model;

    private UIPortrait lastlyClickedEntry;
    //private List<UIPerk> PerksList = new List<UIPerk>();


    public void Show()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;

        Refresh();

        Model.gameObject.SetActive(true);
    }

    private void Refresh()
    {
        //  PerksList.Clear();
        //   int claimablPerksCount = 0;
        Utils.DestroyAllChildren(Parent);

        foreach (var perk in AccountDataSO.CharacterData.pendingRewards)
        {
            var perkUi = PrefabFactory.CreateGameObject<UIPendingReward>(PerkPrefab, Parent);
            perkUi.Setup(perk);

        }

        //ClaimAllUIPriceScavengePointsLabel.SetPrice(AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_ALL_COST);
        //UIPriceScavengeTimePrice.SetPrice(AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_ALL_COST_TIME);

        //bool enoughtScavengePoints = AccountDataSO.CharacterData.currency.scavengePoints >= AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_ALL_COST;
        //bool enoughtTime = AccountDataSO.CharacterData.currency.time >= AccountDataSO.OtherMetadataData.constants.SCAVENGE_CLAIM_ALL_COST_TIME;

        //ClaimAllUIPriceScavengePointsLabel.gameObject.SetActive(enoughtScavengePoints);
        //UIPriceScavengeTimePrice.gameObject.SetActive(!enoughtScavengePoints);

        ClaimAllButton.interactable = AccountDataSO.CharacterData.lastClaimedGameDay < AccountDataSO.GlobalMetadata.gameDay;
        ClaimAllButtonGO.SetActive(true);

    }


    public async void ClaimAllClicked()
    {
        var result = await FirebaseCloudFunctionSO.PendingRewardClaim("");
        if (result.Result)
            Hide();
    }


    public void Hide()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
        Model.gameObject.SetActive(false);
    }

}
