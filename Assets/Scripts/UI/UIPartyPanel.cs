using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIPartyPanel : MonoBehaviour
{
    public FirebaseCloudFunctionSO FirebaseCloudFunctionSO;

    public void LeaveParty()
    {
        UIManager.instance.SpawnPromptPanel("Do you want to leave party?", "Leave party", () => { FirebaseCloudFunctionSO.LeaveParty(); }, null);
    }

}
