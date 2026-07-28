using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class DiceComponent : NetworkBehaviour
{
    public Image _DiceArrow;
    public TextMeshProUGUI _RollText;
    [SyncVar(hook = nameof(OnRollChanged))]
    public int _Roll = 0;


    void OnRollChanged(int old, int _new)
    {
        if (_Roll != -1)
        {
            _RollText.text = _Roll.ToString();
            gameObject.SetActive(true);
        }
    }

    [ClientRpc]
    public void SetArrowActive(bool on)
    {
        _DiceArrow.gameObject.SetActive(on);
    }
}
