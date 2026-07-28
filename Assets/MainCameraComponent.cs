using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;

public class MainCameraComponent : NetworkBehaviour
{
    public static MainCameraComponent _MainCameraInstance;

    public Vector3 _StartingPos;
    public Vector3 _StartingRot;
    public bool _Tweening = false;

    public CountryComponent _AttackingCountry;
    public CountryComponent _DefendingCountry;

    public CountryComponent _HoveredCountry;
    public void Awake()
    {
        if (_MainCameraInstance == null)
        {
            _MainCameraInstance = this;
        }

        _StartingPos = transform.position;
        _StartingRot = transform.eulerAngles;
    }

    /*[Command(requiresAuthority = false)]
    void CmdSetCountry(bool isAttacking)
    {
        if (isAttacking)
            RpcSetCounty(_AttackingCountry, isAttacking);
        else
            RpcSetCounty(_DefendingCountry, isAttacking);
    }

    [ClientRpc]

    void RpcSetCounty(CountryComponent c, bool isAttacking)
    {
        if (isAttacking)
            _AttackingCountry = c;
        else
            _DefendingCountry = c;
    }*/
    public void Update()
    {
        if (GameCanvasComponent._GameInstance._LocalPlayer != null && GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameCanvasComponent._GameInstance._CurrentState != TurnStates.BattleMove && GameCanvasComponent._GameInstance._CurrentState != TurnStates.Move && GameCanvasComponent._GameInstance._CurrentState != TurnStates.AdditionalMove && GameCanvasComponent._GameInstance._CurrentState != TurnStates.EarlyMove && !BattleSystem._BattleSystemInstance._ActiveBattle)
                    CmdResetCamera();
                else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
                {
                    if (_AttackingCountry != null)
                        ResetTroopMove();
                }
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.BattleMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.Move || GameCanvasComponent._GameInstance._CurrentState == TurnStates.AdditionalMove || GameCanvasComponent._GameInstance._CurrentState == TurnStates.EarlyMove)
                {
                    GameCanvasComponent._GameInstance.CmdProgressTurn();
                }
                else if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.PlaceTroops)
                    CmdResetCamera();
            }
        }
        /*if (isServer)
        {
            if (Input.GetKey(KeyCode.H))
            {
                CmdSetCountry(true);
            }
        }*/

    }

    public void ActivateBattleSystem()
    {
        _Tweening = false;

        BattleSystem._BattleSystemInstance.SetUpFight();
        BattleSystem._BattleSystemInstance.gameObject.SetActive(true);
    }
    
    [Command(requiresAuthority = false)]
    public void CmdResetCamera()
    {
        if (!_Tweening && _AttackingCountry != null)
        {
            RpcResetCamera();
        }
    }

    [ClientRpc]
    void RpcResetCamera()
    {
        _Tweening = true;

        //Debug.Log(BattleSystem._BattleSystemInstance);

        if (_DefendingCountry == null)
        {
            this.transform.DOMove(_StartingPos, 1).OnComplete(() => ResetSelected());
            this.transform.DORotate(_StartingRot, 1);
        }
        else
        {

            this.transform.DOMove(new Vector3(_AttackingCountry.transform.position.x, _AttackingCountry.transform.position.y, -350), 2).OnComplete(() => this._Tweening = false);
            this.transform.DORotate(_StartingRot, 2);


            _DefendingCountry._Selected = false;
            _DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            _DefendingCountry._MouseHoverTracker = false;
            _DefendingCountry = null;
        }

        BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
        BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
        BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(false);
        BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(false);

        if (BattleSystem._BattleSystemInstance != null)
            BattleSystem._BattleSystemInstance.ResetFight();
    }

    public void ResetSelected()
    {
        if (_AttackingCountry != null)
        {
            _AttackingCountry._Selected = false;
            _AttackingCountry._HoverObject.SetActive(_AttackingCountry._MouseHoverTracker);
            _AttackingCountry._MouseHoverTracker = false;
            _AttackingCountry = null;
        }


        BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
        BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
        BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(false);
        BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(false);

        if (_DefendingCountry != null)
        {
            _DefendingCountry._Selected = false;
            _DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            _DefendingCountry._MouseHoverTracker = false;

            if (_DefendingCountry._HasProxy)
            {
                _DefendingCountry._Proxy._Selected = false;
                _DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                _DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            _DefendingCountry = null;
        }

        _Tweening = false;
    }

    void ResetTroopMove()
    {
        if (_DefendingCountry != null)
        {
            _AttackingCountry._TroopsCount += _DefendingCountry._AddedTroops;
            _DefendingCountry._TroopsCount -= _DefendingCountry._AddedTroops;
            _DefendingCountry._AddedTroops = 0;
            _DefendingCountry._TroopDisplay.color = _DefendingCountry._OccupyingArmy._TextColour;
            BoardComponent._BoardInstance._TroopsAdded.Remove(_DefendingCountry);

            _AttackingCountry._TroopDisplay.text = _AttackingCountry._TroopsCount.ToString();
            _DefendingCountry._TroopDisplay.text = _DefendingCountry._TroopsCount.ToString();

            BoardComponent._BoardInstance._IncreaseButtonValue[0].text = "";
            BoardComponent._BoardInstance._DecreaseButtonValue[0].text = "";
            BoardComponent._BoardInstance._IncreaseButton[0].gameObject.SetActive(false);
            BoardComponent._BoardInstance._DecreaseButton[0].gameObject.SetActive(false);

            _DefendingCountry._Selected = false;
            _DefendingCountry._HoverObject.SetActive(_DefendingCountry._MouseHoverTracker);
            _DefendingCountry._MouseHoverTracker = false;

            if (_DefendingCountry._HasProxy)
            {
                _DefendingCountry._Proxy._Selected = false;
                _DefendingCountry._Proxy._HoverObject.SetActive(_DefendingCountry._Proxy._MouseHoverTracker);
                _DefendingCountry._Proxy._MouseHoverTracker = false;
            }

            _DefendingCountry = null;
        }
        else
        {
            _AttackingCountry._Selected = false;
            _AttackingCountry._HoverObject.SetActive(_AttackingCountry._MouseHoverTracker);
            _AttackingCountry._MouseHoverTracker = false;
            _AttackingCountry = null;
        }
    }
}
