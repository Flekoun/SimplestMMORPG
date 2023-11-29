using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class TweenEffects : MonoBehaviour
{

    public void ImportantMessageShowTween(Transform _transform, TweenCallback _onFinished, float delay)
    {
        DOTween.defaultAutoPlay = AutoPlay.None;

        Tween moveDown = _transform.DOLocalMove(new Vector3(0, -250, 0), 0.5f).SetRelative(true).SetEase(Ease.OutQuad);
        Tween moveUp = _transform.DOLocalMove(new Vector3(0, 250, 0), 0.5f).SetDelay(delay).SetRelative(true).SetEase(Ease.OutQuad).OnComplete(() => _onFinished());

        // Tween scaleUp = transform.DOScale(new Vector3(1.2f, 0, 0), 0.25f).SetEase(Ease.OutQuad);
        //Tween scaleDown = transform.DOScale(new Vector3(1f, 1, 0), 0.25f).SetEase(Ease.OutQuad);

        Sequence mySequence = DOTween.Sequence();

        mySequence.Append(moveDown);
        mySequence.Append(moveUp);
        //  mySequence.Insert(0, move);

        mySequence.Play().SetUpdate(true);

    }




    public void HitEffectTween(Image _obj, Color _color)
    {
        DOTween.defaultAutoPlay = AutoPlay.None;

        Tween colorReset = DOTween.To(() => _obj.color, x => _obj.color = x, new Color(_color.r, _color.g, _color.b, 1), 0.1f);
        Tween colorFade = DOTween.To(() => _obj.color, x => _obj.color = x, new Color(_color.r, _color.g, _color.b, 0), 1f);

        Sequence mySequence = DOTween.Sequence();

        mySequence.Append(colorReset);
        mySequence.Append(colorFade);

        mySequence.Play().SetUpdate(true);
    }


    public void ShakeEffectTween(Transform _transform)
    {
        DOTween.defaultAutoPlay = AutoPlay.None;

        Tween shake = _transform.DOShakeRotation(1, 60, 7, 90, true, ShakeRandomnessMode.Full);

        Sequence mySequence = DOTween.Sequence();

        mySequence.Append(shake);
       // mySequence.Append(colorFade);

        mySequence.Play().SetUpdate(true);
    }


    public void HighlightEffect(Graphic _obj, Color _color)
    {
        _obj.gameObject.SetActive(true);
        DOTween.defaultAutoPlay = AutoPlay.None;

        _obj.color = new Color(_color.r, _color.g, _color.b, 0f);
        Tween colorFadeIn = DOTween.To(() => _obj.color, x => _obj.color = x, new Color(_color.r, _color.g, _color.b, 0.4f), 1f);
        Tween colorFadeOut = DOTween.To(() => _obj.color, x => _obj.color = x, new Color(_color.r, _color.g, _color.b, 0), 1f);

        Sequence mySequence = DOTween.Sequence();
        mySequence.SetLoops(-1);
        mySequence.Append(colorFadeIn);
        mySequence.Append(colorFadeOut);

        mySequence.SetId(_obj.gameObject);
        mySequence.Play().SetUpdate(true);
    }

    public void KillTween(GameObject _obj)
    {
        //   _obj.transform.DOKill();
        DOTween.Kill(_obj);
        _obj.gameObject.SetActive(false);
        //Sequence twn = _obj.GetComponent<Sequence>();

        //if (twn != null)
        //    twn.Kill();
    }



}
