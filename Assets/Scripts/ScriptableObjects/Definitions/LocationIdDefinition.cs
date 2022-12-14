using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(menuName = "SimplestMMORPG/LocationIdDefinition")]
public class LocationIdDefinition : BaseIdDefinition, IHasScreenPosition
{
 
    //public string Id;
    //public string DisplayName;
    public Vector2 Position;
    public Sprite BackgroudImage;
    public bool IsTownLocation = false;
    public bool IsEncounterLocation = false;
    public bool IsDungeonLocation = false;
    public Vector2 GetScreenPosition()
    {
        return Position;
    }


}
