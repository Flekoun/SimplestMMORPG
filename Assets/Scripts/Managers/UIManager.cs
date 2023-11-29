using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{


    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public GameObject LoadingScreen;
    public GameObject LoadingScreenHiddenScreenLock;

    public PrefabFactory PrefabFactory;
    [Header("Prefabs")]
    public GameObject UIErrorTextPrefab;
    public GameObject UIPromptWindowPrefab;
    public GameObject TooltipPrefab;
    public GameObject ParticlePrefab;

    [Header("Parents")]
    public Transform MessagesParent;
    public Transform PromptWindowParent;
    public Transform ParticleEffectsParent;


    [Header("Other")]
    public GameObject MainScreen;
    public GameObject LoginScreen;
    public Canvas MainCanvas;


    public ImportantMessage ImportantMessage;

    public UIQuestgiverDetailPanel UIQuestgiverDetailPanel;
    public UIEncounterResultDetailPanel UIEncounterResultDetailPanel;
    public UIEncounterDetailSwitcher UIEncounterDetailSwitcher;
    public UIVendorDetailPanel UIVendorDetailPanel;
    public UITrainerDetailPanel UITrainerDetailPanel;
    public UILeaderboards UILeaderboardsPanel;

    public ContextInfoPanel ContextInfoPanel;

    public void SpawnParticleEffectUIPosition(Vector3 _position)
    {
        PrefabFactory.CreateGameObject<ParticleSystem>(ParticlePrefab, ParticleEffectsParent, _position);
    }

    public UIPromptWindow SpawnPromptPanel(string _description, string _title, UnityAction _onAcceptClicked, UnityAction _onDeclineClicked)
    {
        var window = PrefabFactory.CreateGameObject<UIPromptWindow>(UIPromptWindowPrefab, PromptWindowParent);
        window.Setup(_description, _title, _onAcceptClicked, _onDeclineClicked);
        return window;
    }

    public UIPromptWindow SpawnPromptPanel(string _description, UnityAction _onAcceptClicked, UnityAction _onDeclineClicked)
    {
        var window = PrefabFactory.CreateGameObject<UIPromptWindow>(UIPromptWindowPrefab, PromptWindowParent);
        window.Setup(_description, _onAcceptClicked, _onDeclineClicked);
        return window;
    }

    public UITooltip SpawnTooltip(Transform _parent, string _stringId, IContentDisplayable _contentContainer, CombatSkill _combatSkill, CombatBuff _combatBuff, int _manaLeft, float _offSetY, int[] _values = null)
    {
        var tooltip = PrefabFactory.CreateGameObject<UITooltip>(TooltipPrefab, PromptWindowParent);
        tooltip.Setup(_stringId, _contentContainer, _combatSkill, _combatBuff, _manaLeft, _parent, _offSetY, _values);
        return tooltip;
    }



    public void ShowMainScreen()
    {
        MainScreen.SetActive(true);
        LoginScreen.SetActive(false);
        //    CharacterScreen.SetActive(false);

    }

    public void ShowLoginScreen()
    {
        MainScreen.SetActive(false);
        LoginScreen.SetActive(true);
        ///    CharacterScreen.SetActive(false);
    }
    //Here is a private reference only this class can access
    private static UIManager _instance;

    //This is the public reference that other classes will use
    public static UIManager instance
    {
        get
        {
            //If _instance hasn't been set yet, we grab it from the scene!
            //This will only happen the first time this reference is used.
            if (_instance == null)
                _instance = GameObject.FindObjectOfType<UIManager>();
            return _instance;
        }
    }

    //public void ShowUISkillChooser(UISkillEquipSlot _slot)
    //{

    //    UISkillChooserSpawner.Show(_slot);
    //    UISkillChooserSpawner.gameObject.SetActive(true);
    //}

    public void SpawnErrorText(string _text)
    {
        PrefabFactory.CreateGameObject<FloatingText>(UIErrorTextPrefab, MessagesParent).Show(_text);

    }

    public void ShowLoadingScreen(bool _hiddenScreenLock)
    {

        if (_hiddenScreenLock)
            LoadingScreenHiddenScreenLock.SetActive(true);
        else
            LoadingScreen.SetActive(true);

    }

    public void HideLoadingScreen()
    {
        LoadingScreen.SetActive(false);
        LoadingScreenHiddenScreenLock.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        FirebaseCloudFunctionSO.OnCloudFunctionInProgress += ShowLoadingScreen;
        FirebaseCloudFunctionSO.OnCloudFunctionFinished += HideLoadingScreen;

    }


    // Update is called once per frame
    void Update()
    {

    }
}
