using System.Collections;
using System.Collections.Generic;
using simplestmmorpg.data;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class UICharacterPreviewSpawner : UISelectableSpawner
{


    public PrefabFactory PrefabFactory;
    public AccountDataSO AccountDataSO;
    public Transform CharacterPreviewParent;
    public GameObject CharacterListEntryPrefab;
    public GameObject MoreDataPrefab;
    public UnityAction<UICharacterPreviewEntry> OnEntryClicked;
    public UnityAction OnMoreDataClicked;

    private bool moreButtonWasSpawnedLastTime = false;
    // Start is called before the first frame update
    public void Spawn()
    {
        Spawn(AccountDataSO.PlayerData.characters);
    }

    // TODO: tyhle parametry sou tu zbytecne imo, kdysi kdyz sem to pouzival i na leaderboards sem to otreboval
    public void Spawn(List<CharacterPreview> _data, bool _destroyOldEntries = true, bool _showMoreButton = false)
    {
        if (_destroyOldEntries)
            Utils.DestroyAllChildren(CharacterPreviewParent);
        else if (moreButtonWasSpawnedLastTime)
            Destroy(CharacterPreviewParent.GetChild(CharacterPreviewParent.childCount - 1).gameObject);

        foreach (var character in _data)
        {
            var charPrev = PrefabFactory.CreateGameObject<UICharacterPreviewEntry>(CharacterListEntryPrefab, CharacterPreviewParent);
            charPrev.SetData(character);
            charPrev.OnClicked += OnCharacterPreviewClicked;
        }

        if (_showMoreButton)
        {
            var button = PrefabFactory.CreateGameObject<Button>(MoreDataPrefab, CharacterPreviewParent);
            button.onClick.AddListener(OnMoreDataClicked);
            moreButtonWasSpawnedLastTime = true;
        }
    }


    private void OnCharacterPreviewClicked(UICharacterPreviewEntry _entry)
    {
        base.OnUISelectableItemClicked(_entry);

        if (OnEntryClicked != null)
            OnEntryClicked.Invoke(_entry);
    }
}
