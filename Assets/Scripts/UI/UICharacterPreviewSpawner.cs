using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UICharacterPreviewSpawner : UISelectableSpawner
{
    public PrefabFactory PrefabFactory;
    public AccountDataSO AccountDataSO;
    public Transform CharacterPreviewParent;
    public GameObject CharacterListEntryPrefab;
    // public ListenOnAccountData ListenOnAccountData;
    public UnityAction<UICharacterPreviewEntry> OnEntryClicked;

    // Start is called before the first frame update
    public void Spawn()
    {
        Utils.DestroyAllChildren(CharacterPreviewParent);
        foreach (var character in AccountDataSO.PlayerData.characters)
        {
            var charPrev = PrefabFactory.CreateGameObject<UICharacterPreviewEntry>(CharacterListEntryPrefab, CharacterPreviewParent);
            charPrev.SetData(character);
            charPrev.OnClicked += OnCharacterPreviewClicked;
        }
    }



    private void OnCharacterPreviewClicked(UICharacterPreviewEntry _entry)
    {
        base.OnUISelectableItemClicked(_entry);

        if (OnEntryClicked != null)
            OnEntryClicked.Invoke(_entry);
        // ListenOnAccountData.StartListeningOnCharacter(_entry.Data.uid);
    }
}
