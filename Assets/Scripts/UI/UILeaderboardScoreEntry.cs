using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using simplestmmorpg.data;
using UnityEngine.Events;

public class UILeaderboardScoreEntry : MonoBehaviour
{
    public TextMeshProUGUI CharacterNameText;
    public TextMeshProUGUI CharacterClassText;
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI RankText;
    public UIPortrait Portrait;

    public UnityAction<UILeaderboardScoreEntry> OnClicked;
    public CharacterPreview Data;



    public void SelectButtonClicked()
    {
        if (OnClicked != null)
            OnClicked.Invoke(this);
    }


    public void SetData(LeaderboardScoreEntry _data, int _rank)
    {
        Data = _data.character;
        CharacterNameText.SetText(Data.name);
        CharacterNameText.color = Utils.GetClassColor(Data.characterClass);
        CharacterClassText.SetText(Data.characterClass);
        ScoreText.SetText(_data.score.ToString());
        if (_rank == -1)
            RankText.SetText("You");
        else
            RankText.SetText(_rank.ToString());
        Portrait.SetPortrait(Data.portrait, Data.characterClass);
    }

    //public void SetData(CharacterPreview _data)
    //{
    //    Data = _data;
    //    CharacterNameText.SetText(Data.name);
    //    CharacterNameText.color = Utils.GetClassColor(Data.characterClass);
    //    CharacterClassText.SetText(Data.characterClass);
    //    ScoreText.SetText("Level " + Data.level.ToString());
    //    Portrait.SetPortrait(Data.portrait);
    //}
}
