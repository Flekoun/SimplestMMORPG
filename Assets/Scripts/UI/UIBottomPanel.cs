using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class UIBottomPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public GameObject CraftingGO;
    public GameObject PerksGO;
    public GameObject PerkMarkGO;
    public TextMeshProUGUI PerkMarkText;
    public TextMeshProUGUI ProfessionText;

    // Start is called before the first frame update
    void Awake()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    // Update is called once per frame
    void Refresh()
    {
        //  if (AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.ALCHEMY) != null || AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING) != null)
        CraftingGO.SetActive(AccountDataSO.CharacterData.craftingRecipesUnlocked.Count > 0);

        PerksGO.SetActive(AccountDataSO.CharacterData.pendingRewards.Count > 0);
        // int perksToClaim = AccountDataSO.CharacterData.GetUnclaimedPerksCount(AccountDataSO.GlobalMetadata.gameDay);
        PerkMarkGO.SetActive(AccountDataSO.CharacterData.lastClaimedGameDay < AccountDataSO.GlobalMetadata.gameDay);
        PerkMarkText.SetText(AccountDataSO.CharacterData.pendingRewards.Count.ToString());

        if (AccountDataSO.CharacterData.professions.Count > 0)
            ProfessionText.SetText(Utils.DescriptionsMetadata.GetProfessionMetadata(AccountDataSO.CharacterData.professions[0].id).title.GetText());
    }
}
