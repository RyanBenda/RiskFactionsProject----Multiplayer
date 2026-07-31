using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class ArmyInfoComponent : NetworkBehaviour
{
    public ArmiesStruct _Army;
    public Image _Image;
    public TextMeshProUGUI _Name;
    public TextMeshProUGUI _Defeated;
    public NetworkIdentity _NetworkIdentity;

    [SyncVar(hook = nameof(OnDefeated))]
    public bool _IsDefeated = false;
    // Start is called before the first frame update
    void Start()
    {
        _NetworkIdentity.enabled = false; //Set false on start becuase it needs a network identity to be spawned but parent has network identity
        transform.parent = GameCanvasComponent._GameInstance._ArmyOrder.content;
        transform.localScale = Vector3.one;
    }

    [ClientRpc]
    public void RpcCreateArmyOrderObject(ArmiesStruct army)
    {
        _Army = army;
        _Image.color = army._Army._ArmyColour;
        _Name.text = army._Army._ArmyName;
        _Name.color = army._TextColour;
    }

    [Command(requiresAuthority = false)]
    public void SetDefeated()
    {
        _IsDefeated = true;
    }

    void OnDefeated(bool old, bool _new)
    {
        if (_IsDefeated)
        {
            _Defeated.color = GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour;
            _Defeated.gameObject.SetActive(true);

        }
    }
}
