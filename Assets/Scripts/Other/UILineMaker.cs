using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UILineMaker : MonoBehaviour
{
    public Sprite lineImage;
    public Vector2 graphScale;
    public int lineWidth;
    public GameObject LinePrefab;

    // Start is called before the first frame update
    public void MakeLine(float ax, float ay, float bx, float by, Color col)
    {

        GameObject NewObj = null;
        NewObj = new GameObject();
        NewObj.name = "line from " + ax + " to " + bx;
        Image NewImage = NewObj.AddComponent<Image>();
        NewImage.sprite = lineImage;
        NewImage.color = col;


        RectTransform rect = NewObj.GetComponent<RectTransform>();
        rect.SetParent(transform);
        rect.localScale = Vector3.one;

        Vector3 a = new Vector3(ax * graphScale.x, ay * graphScale.y, 0);
        Vector3 b = new Vector3(bx * graphScale.x, by * graphScale.y, 0);


        rect.localPosition = (a + b) / 2;
        Vector3 dif = a - b;
        rect.sizeDelta = new Vector3(dif.magnitude, lineWidth);
        rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
    }

    // Start is called before the first frame update

    public void DestroyAllLines()
    {
        Utils.DestroyAllChildren(transform);
    }
    public void MakeLineFromPrefab(float ax, float ay, float bx, float by, int _travelTimeWeight)
    {


        UITravelLine NewObj = ((GameObject)Instantiate(LinePrefab)).GetComponent<UITravelLine>();

        NewObj.Setup(_travelTimeWeight);

        RectTransform rect = NewObj.gameObject.GetComponent<RectTransform>();
        rect.SetParent(transform);
        rect.localScale = Vector3.one;

        Vector3 a = new Vector3(ax * graphScale.x, ay * graphScale.y, 0);
        Vector3 b = new Vector3(bx * graphScale.x, by * graphScale.y, 0);


        rect.localPosition = (a + b) / 2;
        Vector3 dif = a - b;
        rect.sizeDelta = new Vector3(dif.magnitude, lineWidth);
        rect.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan(dif.y / dif.x) / Mathf.PI));
    }

    // Update is called once per frame
    void Start()
    {
        //   MakeLine(-223, 625, 178, -71, Color.red);
    }
}
