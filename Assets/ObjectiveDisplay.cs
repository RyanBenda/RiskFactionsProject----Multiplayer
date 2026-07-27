using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ObjectiveDisplay : NetworkBehaviour
{
    public ObjectiveManager _Manager;
    [SyncVar]
    public ObjectiveScriptableObject _Objective;
    public GameObject _HoverObject;
    public TextMeshProUGUI _Claimed;

    public void MouseEnter()
    {
        _HoverObject.SetActive(true);
        _Manager._ObjectiveDescription.text = _Objective._Name;
    }

    public void MouseExit()
    {
        _HoverObject.SetActive(false);
    }
}
