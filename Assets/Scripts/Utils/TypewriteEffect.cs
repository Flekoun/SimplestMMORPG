using System;
using System.Collections;
using System.Collections.Generic;
using RoboRyanTron.Unite2017.Variables;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TypewriteEffect : MonoBehaviour
{
    public TextMeshProUGUI TextToEffect;
    //    public string stringId;
    public AudioClipSO TypeAudioEffect;
    public FloatReference TypeSpeed;

    // Start is called before the first frame update
    private Coroutine CoroutineType;
    private int characterCount;
    // string textAfterLocalization;


    public delegate void TypewriterOver();
    public event TypewriterOver OnTypewriterOver;

    public void SetTypeSpeed(float _speed)
    {
        TypeSpeed.Value = _speed;
    }

    void Start()
    {
        if (TextToEffect == null)
        {
            var text = this.gameObject.GetComponent<TextMeshProUGUI>();
            if (text != null)
                TextToEffect = text;

        }
        TextToEffect.ForceMeshUpdate();
        characterCount = TextToEffect.textInfo.characterCount;

        if (CoroutineType == null)
            CoroutineType = StartCoroutine(TypewritePrint());
    }

    //private void Refresh()
    //{
    //    //textAfterLocalization = Utils.DescriptionsMetadata.GetDescriptionMetadataForId(stringId).title.EN;
    //    //textAfterLocalization += "\n" + Utils.DescriptionsMetadata.GetDescriptionMetadataForId(stringId).description.EN;
    //    //Text.text = textAfterLocalization;


    //}

    //public void SetText(string _text)
    //{
    //    stringId = _text;
    //    Refresh();
    //}


    private IEnumerator TypewritePrint()
    {
        TextToEffect.gameObject.SetActive(false);
        yield return new WaitForSecondsRealtime(TypeSpeed.Value); //mam to tu aby skript chvilku pockal nez se textmesh pro ktery ma mit tento efekt nastavi v Startu/Awaku atd...
        TextToEffect.gameObject.SetActive(true);

        for (int i = 0; i <= characterCount; i++)
        {
            TextToEffect.maxVisibleCharacters = i;  //PODLE NE length != maxvisiblechars!?? divne?

            if (i > 0)
                if (TextToEffect.textInfo.characterInfo[i - 1].character != ' ')
                    TypeAudioEffect.Play();

            yield return new WaitForSecondsRealtime(TypeSpeed.Value);

        }
        if (OnTypewriterOver != null)
            OnTypewriterOver.Invoke();

        OnTypewriteOver.Invoke();
    }


    public UnityEvent OnTypewriteOver;

}
