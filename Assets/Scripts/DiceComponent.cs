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
    public bool _Airfield = false;

    void OnRollChanged(int old, int _new)
    {
        if (_Roll != -1)
        {
            if (!_Airfield) // Making sure the dice displays the original roll value and not the increased value by the Airfield as that is displayed through a +1 logo on the die
                _RollText.text = _Roll.ToString();
            else
                _RollText.text = (_Roll - 1).ToString();
            gameObject.SetActive(true);

            _Airfield = false;
        }
    }

    [ClientRpc]
    public void SetArrowActive(bool on)
    {
        _DiceArrow.gameObject.SetActive(on);
    }
}
