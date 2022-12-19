using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class TweenEffects : MonoBehaviour
{ 

    public void ImportantMessageShowTween(Transform _transform , TweenCallback _onFinished, float delay)
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

  

}
