using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bezier : MonoBehaviour
{
    public Transform[] controlPoints;
    public LineRenderer lineRenderer;

    private int curveCount = 0;
    private int layerOrder = 0;
    private int SEGMENT_COUNT = 50;

    private Transform MyTarget;

//    private float offset = 0;
    public void SetTarget(Transform _target)
    {
        // if (_target.position.x < this.transform.position.x)
        // {
             MyTarget = _target;
        RefreshGraphics();
        // }
        // else
        // {
        // controlPoints[2].position = _target.position;
        // controlPoints[3].position = _target.position;
        // }

        // offset = controlPoints[3].position.x - _target.position.x;
        //   Debug.Log("offter : " + offset);
        //controlPoints[2].position.Set(controlPoints[2].position.x, offset, controlPoints[2].position.z);
        //  controlPoints[1].position.Set(controlPoints[1].position.x, offset, controlPoints[1].position.z);


    }

    private void RefreshGraphics()
    {
        controlPoints[2].position = MyTarget.position;
        controlPoints[3].position = MyTarget.position;

        //float offset = controlPoints[3].position.x - _target.position.x;

        controlPoints[2].position = new Vector3(controlPoints[2].position.x, 5f, controlPoints[2].position.z);
    }

    void Start()
    {
        if (!lineRenderer)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }
        lineRenderer.sortingLayerID = layerOrder;
        curveCount = (int)controlPoints.Length / 3;
    }

    void Update()
    {

        //DrawCurve();
        DrawStreightCurve();
        RefreshGraphics();
    }

    void DrawStreightCurve()
    {
        lineRenderer.positionCount =2;
        lineRenderer.SetPosition(0, this.gameObject.transform.position);
        lineRenderer.SetPosition(1, MyTarget.position);
    }


    void DrawCurve()
    {
        for (int j = 0; j < curveCount; j++)
        {
            for (int i = 1; i <= SEGMENT_COUNT; i++)
            {
                float t = i / (float)SEGMENT_COUNT;
                int nodeIndex = j * 3;
                Vector3 pixel = CalculateCubicBezierPoint(t, controlPoints[nodeIndex].position, controlPoints[nodeIndex + 1].position, controlPoints[nodeIndex + 2].position, controlPoints[nodeIndex + 3].position);

                lineRenderer.positionCount = (((j * SEGMENT_COUNT) + i));
                lineRenderer.SetPosition((j * SEGMENT_COUNT) + (i - 1), pixel);
            }

        }
    }

    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }
}
