using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{
    public RectTransform RectTransform;
  //  public Vector2Int gridSize;
    public List<Vector2> points;

    float width;
    float height;
    float unitWidth=1;
    float unitHeight=1;
    public float thickness = 10f;


    protected override void OnPopulateMesh(VertexHelper _helper)
    {
        _helper.Clear();
        width = rectTransform.rect.width;
        height = rectTransform.rect.height;

       // unitWidth = width / (float)gridSize.x;
       // unitHeight = height / (float)gridSize.y;


        if (points.Count < 2)
        {
            return;
        }

       // points.Add(new Vector2(RectTransform.localPosition.x, RectTransform.localPosition.y));

        //ebug.Log("XXX");
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 point = points[i];
            DrawVerticesForPoint(point, _helper);
        }

        for (int i = 0; i < points.Count - 1; i++)
        {
            int index = i * 2;
            _helper.AddTriangle(index + 0, index + 1, index + 3);
            _helper.AddTriangle(index + 3, index + 2, index + 0);
        }

    }

    void DrawVerticesForPoint(Vector2 point, VertexHelper _helper)
    {
       // Debug.Log("drawing");
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;
        vertex.position = new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(unitWidth * point.x, unitHeight * point.y);
//        Debug.Log("vertex: " + vertex.position.x + "x " +vertex.position.y);
        _helper.AddVert(vertex);
        vertex.position = new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(unitWidth * point.x, unitHeight * point.y);
        _helper.AddVert(vertex);
    }
}
