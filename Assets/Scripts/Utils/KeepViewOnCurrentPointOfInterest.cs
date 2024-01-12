using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepViewOnCurrentPointOfInterest : MonoBehaviour
{
    public AccountDataSO AccountDataSO;
    public Transform ScrollContent;
    public UIPointsOfInterestSpawner UIPointsOfInterestSpawner;
    public bool IsFunctional = true;
    private Coroutine ViewMoveCoroutine = null;

    // Start is called before the first frame update
    public void OnEnable()
    {
        if (!IsFunctional) return;
        AccountDataSO.OnPointOfInterestDataChanged += Refresh;
        Refresh();
    }


    // Update is called once per frame
    public void OnDisable()
    {
        if (!IsFunctional) return;
        AccountDataSO.OnPointOfInterestDataChanged -= Refresh;
    }

    private void Refresh()
    {
        if (!IsFunctional) return;
        var CurrentPoi = UIPointsOfInterestSpawner.GetPointOfInterestButtonAtCharacterPosition();
        Debug.Log("CurrentPoi : " + CurrentPoi.WorldPosition.pointOfInterestId);
        if (CurrentPoi != null)
        {
            Vector3 pos1 = CurrentPoi.transform.localPosition;
            Vector3 pos2 = new Vector3((-1) * pos1.x, (-1) * pos1.y, pos1.z);
            //   ScrollContent.localPosition = pos2;

            if (ViewMoveCoroutine != null)
                StopCoroutine(ViewMoveCoroutine);

            ViewMoveCoroutine = StartCoroutine(MoveView(pos2));
        }
        else
            Debug.LogError("Jaktoze nejsem na zadnem PoI buttonu? Kde sem?");
    }


    private IEnumerator MoveView(Vector3 targetPosition)
    {
        float startTime = Time.time;
        while (Vector3.Distance(ScrollContent.localPosition, targetPosition) > 10 && (Time.time - startTime) < 1f)
        {
            //            Debug.Log(ScrollContent.localPosition);
            //           Debug.Log(targetPosition);
            ScrollContent.localPosition = Vector3.Lerp(ScrollContent.localPosition, targetPosition, 2f * Time.deltaTime);
            yield return null;
        }
    }

}


