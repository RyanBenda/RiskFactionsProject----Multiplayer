using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class ArmyInfoComponent : NetworkBehaviour
{
    public ArmiesClass2 _Army;
    public Image _Image;
    public TextMeshProUGUI _Name;
    public TextMeshProUGUI _Defeated;
    public NetworkIdentity _NetworkIdentity;
    // Start is called before the first frame update
    void Start()
    {
        _NetworkIdentity.enabled = false;
        transform.parent = GameCanvasComponent._GameInstance._ArmyOrder.content;
        transform.localScale = Vector3.one;
    }

    [ClientRpc]
    public void RpcCreateArmyOrderObject(ArmiesClass2 army)
    {
        _Army = army;
        _Image.color = army._Army._ArmyColour;
        _Name.text = army._Army._ArmyName;
        _Name.color = army._TextColour;


    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
