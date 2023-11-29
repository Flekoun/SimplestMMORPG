using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UICraftingPanel : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public UICraftingRecipesSpawner UICraftingRecipesSpawner;
    public UICraftingRecipeDetail UICraftingRecipeDetail;
    //  public UIContentContainerDetail UIContentContainerDetail;
    public UIProgressBar ProfessionProgressBar;
    public TextMeshProUGUI ProfressionText;

    public Button CraftButton;
    public TextMeshProUGUI CraftButtonText;
    public GameObject Model;

    //   private UICraftingRecipeEntry choosenRecipe;


    public void OnEnable()
    {
        AccountDataSO.OnCharacterDataChanged += Refresh;
    }

    public void OnDisable()
    {
        AccountDataSO.OnCharacterDataChanged -= Refresh;
    }


    public void Awake()
    {

        UICraftingRecipesSpawner.OnRecipeClicked += OnRecipeClicked;
        UICraftingRecipeDetail.OnMaterialOrProductClicked += OnMaterialOrProductClicked;
    }

    public void Show()
    {
        Refresh();
        Model.SetActive(true);

    }

    public void Hide()
    {
        Model.SetActive(false);
    }

    private void Refresh()
    {
        UICraftingRecipesSpawner.Refresh();


        if (AccountDataSO.CharacterData.professions.Count > 0)
        {
            ProfressionText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(AccountDataSO.CharacterData.professions[0].id));
            ProfessionProgressBar.SetValues(AccountDataSO.CharacterData.professions[0].countMax, AccountDataSO.CharacterData.professions[0].count);


        }

        //if (AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.ALCHEMY) != null)
        //{
        //    ProfressionText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.PROFESSIONS.ALCHEMY));
        //    ProfessionProgressBar.SetValues(AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.ALCHEMY).countMax, AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.ALCHEMY).count);
        //}
        //else if (AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING) != null)
        //{
        //    ProfressionText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.PROFESSIONS.BLACKSMITHING));
        //    ProfessionProgressBar.SetValues(AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING).countMax, AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING).count);
        //}
        //else if (AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING) != null)
        //{
        //    ProfressionText.SetText(Utils.ReplacePlaceholdersInTextWithDescriptionFromMetadata(Utils.PROFESSIONS.BLACKSMITHING));
        //    ProfessionProgressBar.SetValues(AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING).countMax, AccountDataSO.CharacterData.GetProfessionById(Utils.PROFESSIONS.BLACKSMITHING).count);
        //}

        if (UICraftingRecipesSpawner.LastChoosenRecipe == null)
            UICraftingRecipesSpawner.ClickOnFirstEntry();

    }



    public void OnRecipeClicked(UICraftingRecipeEntry _entry)
    {
        bool hasEnoughTime = AccountDataSO.CharacterData.currency.time >= _entry.Data.timePrice;
        //CraftButton.interactable = hasEnoughTime;

        //  choosenRecipe = _entry;
        UICraftingRecipeDetail.SetData(_entry.Data);
        CraftButton.interactable = _entry.Data.CanBeCrafted(AccountDataSO.CharacterData) && hasEnoughTime;

        if (hasEnoughTime)
            CraftButtonText.SetText("Craft(" + _entry.Data.timePrice + ")");
        else
            CraftButtonText.SetText("Craft(<color=\"red\">" + _entry.Data.timePrice + "</color>)");

    }



    public void OnMaterialOrProductClicked(UIContentItem _entry)
    {
        // UIContentContainerDetail.Show(_entry.GetData());
       // UIManager.instance.ContextInfoPanel.ShowContentContainerDetail(_entry.GetData());
    }

    public async void CraftRecipe()
    {
        if (UICraftingRecipesSpawner.LastChoosenRecipe != null)
        {
            var oldChoosenItemName = UICraftingRecipesSpawner.LastChoosenRecipe.Data.GetDisplayName(); //mam to tu protoze po vyrobe se buhvico stane a vybrany recipe uz muze byt jiny
            await FirebaseCloudFunctionSO.CraftRecipe(UICraftingRecipesSpawner.LastChoosenRecipe.Data.id);
            UIManager.instance.ImportantMessage.ShowMesssage(oldChoosenItemName + " crafted!");
        }


    }
}
