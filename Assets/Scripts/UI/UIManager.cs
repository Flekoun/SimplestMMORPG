using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{

    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;
    public GameObject LoadingScreen;

    public PrefabFactory PrefabFactory;
    [Header("Prefabs")]
    public GameObject UIErrorTextPrefab;
    public GameObject UIPromptWindowPrefab;
    [Header("Parents")]
    public Transform MessagesParent;
    public Transform PromptWindowParent;

    [Header("Other")]
    public GameObject MainScreen;
    public GameObject LoginScreen;

    public ImportantMessage ImportantMessage;
    //   public GameObject CharacterScreen;
    //    public UISkillChooserSpawner UISkillChooserSpawner;

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

    public void ShowLoadingScreen()
    {

        LoadingScreen.SetActive(true);
    }

    public void HideLoadingScreen()
    {
        LoadingScreen.SetActive(false);
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
