using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using Mirror;
public class ProxyCountryComponent : NetworkBehaviour // Proxy country on the edge of the map used for non quick battles
{
    public CountryComponent _AttackerCountry;
    public CountryComponent _ActualCountry;

    public GameObject _HoverObject;
    public bool _Selected = false;
    public bool _MouseHoverTracker = false;

    public TextMeshProUGUI _TroopDisplay;

    public Color _CurColour;
    public Image[] _CountryColour;

    public void MouseEnter()
    {
        _HoverObject.SetActive(true);

        if (_Selected)
            _MouseHoverTracker = true;
    }

    public void MouseExit()
    {
        if (!_Selected)
            _HoverObject.SetActive(false);
        else
            _MouseHoverTracker = false;
    }

    public void MouseClick()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.Battle)
        {
            if (!MainCameraComponent._MainCameraInstance._Tweening && MainCameraComponent._MainCameraInstance._AttackingCountry != null && MainCameraComponent._MainCameraInstance._AttackingCountry == _AttackerCountry && MainCameraComponent._MainCameraInstance._DefendingCountry == null && MainCameraComponent._MainCameraInstance._AttackingCountry._CurColour != _ActualCountry._CurColour && MainCameraComponent._MainCameraInstance._AttackingCountry._TroopsCount > 1)
            {
                CmdProxyCamera();
            }
        }
    }

    [Command(requiresAuthority = false)]
    void CmdProxyCamera()
    {
        ProxyCamera();
    }

    [ClientRpc]
    void ProxyCamera()
    {
        MainCameraComponent._MainCameraInstance._Tweening = true;
        MainCameraComponent._MainCameraInstance._DefendingCountry = _ActualCountry;
        if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
            _MouseHoverTracker = true;
        _Selected = true;
        _HoverObject.SetActive(true);

        MainCameraComponent._MainCameraInstance.transform.DOMove(MainCameraComponent._MainCameraInstance._AttackingCountry._CameraPositions[0].position, 1.5f).OnComplete(() => MainCameraComponent._MainCameraInstance.ActivateBattleSystem());
        MainCameraComponent._MainCameraInstance.transform.DORotate(MainCameraComponent._MainCameraInstance._AttackingCountry._CameraPositions[0].eulerAngles, 1.5f);
    }

    public void UpdateDetails()
    {
        _TroopDisplay.text = _ActualCountry._TroopDisplay.text;
        _TroopDisplay.color = _ActualCountry._TroopDisplay.color;

        for (int j = 0; j < this._CountryColour.Length; j++)
        {
            this._CountryColour[j].color = _ActualCountry._CurColour;
        }
    }
}
