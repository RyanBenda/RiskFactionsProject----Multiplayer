using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Mirror;

public enum TurnStates
{
    PlaceCapital,
    CalculateTroops,
    PlaceTroops,
    EarlyMove,
    Battle,
    BattleMove,
    Move,
    AdditionalMove,
    Reward,
    PlaceAirfield,
    Suspend
}

[System.Serializable]
public struct ArmiesStruct
{
    public ArmyScriptableObject _Army;

    public int _OneStars;
    public int _TwoStars;

    public bool _HasEarlyMove;
    public bool _HasAdditionalMove;
    public bool _HasAttackDie;
    public bool _HasDefenceDie;
    public bool _HasExtraTroops;
    public bool _HasGuaranteedCard;

    public bool _HasStarReward;
    public List<RewardScriptableObject> _Rewards;
    public List<RewardScriptableObject> _PossibleRewards;

    public bool _isDefeated;
    public Color _TextColour;
    public ArmyInfoComponent _Info;

    public List<CountryComponent> _ControlledCountries;
}


public class GameCanvasComponent : NetworkBehaviour
{
    public static GameCanvasComponent _GameInstance;

    public Image _CurrentArmyBanner;
    public TextMeshProUGUI _StarsDisplay;
    public Button _ProgressButton;
    public TextMeshProUGUI _ProgressButtonText;
    public TurnStates _CurrentState = TurnStates.CalculateTroops;

    public readonly SyncList<ArmiesStruct> _TurnOrder = new SyncList<ArmiesStruct>();

    int _TurnIndex = 0;
    [SyncVar(hook = nameof(OnCurArmyChange))]
    public ArmiesStruct _CurArmy;

    public GameObject _NewTroopsIcon;
    public GameObject _RewardDisplay;
    public RewardAdditionComponent _RewardAddition;

    public StarTradeComponent _StarTrade;
    public StarsAdditionComponent _StarDisplay;

    public GameObject _ObjectiveButton;

    public List<RewardScriptableObject> _RewardEffectList = new List<RewardScriptableObject>();
    public IEnumerator _RewardCoroutine;
    bool _Active = false;

    public bool _PlaceAirfield = false;

    [SyncVar]
    public bool _HasAttacked = false;
    public GameObject _Warning;

    public ScrollRect _ArmyOrder;
    public GameObject _ArmyTabPrefab;
    public bool _DisplayActive = false;
    TurnStates _LastState;

    public RiskFactionsPlayerScript _LocalPlayer;

    [SyncVar]
    public int _RewardCount = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (_GameInstance == null)
            _GameInstance = this;

        _ProgressButtonText.text = "Place Capital";
        _ProgressButton.interactable = false;
    }

    void OnCurArmyChange(ArmiesStruct old, ArmiesStruct _new)
    {
        if (_CurArmy._Army != null)
        {
            if (_CurArmy._Info == null) // Sets the Army Info for each army on first call of Army change as they needed time to be spawned by the server
                SetInfo();

            _CurrentArmyBanner.color = _CurArmy._Army._ArmyColour;
            
            RiskFactionsPlayerScript p = NetworkClient.connection.identity.GetComponent<RiskFactionsPlayerScript>(); // could this be _LocalPlayer? Come back to this
            
            if (p._Army._ArmyName == _CurArmy._Army._ArmyName)
            {
                int stars = _CurArmy._TwoStars * 2;
                stars += _CurArmy._OneStars;
                _StarsDisplay.color = _CurArmy._TextColour;
                _StarsDisplay.text = "Stars: " + stars.ToString();

                p.CmdSetTurn(true);
            }
            else
            {
                int cards = _CurArmy._TwoStars;
                cards += _CurArmy._OneStars;
                _StarsDisplay.color = _CurArmy._TextColour;
                _StarsDisplay.text = "Cards: " + cards.ToString();

                p.CmdSetTurn(false);
            }
        }
    }

    public void TurnOnOffDisplay()
    {
        if (!_DisplayActive && !ObjectiveManager._ObjectiveManagerInstance._DisplayActive && !BattleSystem._BattleSystemInstance.gameObject.activeSelf)
        {
            _DisplayActive = true;
            if (_LocalPlayer._IsTurn)
            {
                _LastState = _CurrentState;
                _CurrentState = TurnStates.Suspend;
            }
            _ArmyOrder.gameObject.SetActive(true);
        }
        else if (!BattleSystem._BattleSystemInstance.gameObject.activeSelf)
        {
            _ArmyOrder.gameObject.SetActive(false);
            if (_LocalPlayer._IsTurn && _CurrentState != TurnStates.PlaceCapital)
                _CurrentState = _LastState;
            _DisplayActive = false;
        }
    }

    public void TurnOffWarning()
    {
        _Warning.SetActive(false);
    }

    public void ProgressTurn() // For Progress Turn Button to make sure players can't click it if isn't their turn
    {
        if (_LocalPlayer._IsTurn && _CurrentState != TurnStates.Reward)
            CmdProgressTurn();
    }

    [Command(requiresAuthority = false)]
    public void CmdProgressTurn() // Command so the server can call the Rpc
    {
        RpcProgressTurn();
    }

    [ClientRpc]
    public void RpcProgressTurn() //Rpc that means it runs on every client but can only be activated by the client who's turn it is
    {
        if (!MainCameraComponent._MainCameraInstance._Tweening)
        {
            if (_CurrentState == TurnStates.CalculateTroops)
            {
                _StarTrade.gameObject.SetActive(false);
                _CurrentState = TurnStates.PlaceTroops;
                _ProgressButtonText.text = "Place Troops";
            }
            else if (_CurrentState == TurnStates.PlaceTroops)
            {
                if (BoardComponent._BoardInstance._NewTroops == 0)
                {
                    MainCameraComponent._MainCameraInstance.CmdResetCamera();

                    for (int i = 0; i < BoardComponent._BoardInstance._TroopsAdded.Count;)
                    {
                        BoardComponent._BoardInstance._TroopsAdded[i]._AddedTroops = 0;
                        BoardComponent._BoardInstance._TroopsAdded[i]._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                        if (BoardComponent._BoardInstance._TroopsAdded[i]._HasProxy)
                            BoardComponent._BoardInstance._TroopsAdded[i]._Proxy._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                        BoardComponent._BoardInstance._TroopsAdded.Remove(BoardComponent._BoardInstance._TroopsAdded[i]);
                    }

                    _NewTroopsIcon.SetActive(false);

                    if (_CurArmy._HasEarlyMove)
                    {
                        _CurrentState = TurnStates.EarlyMove;
                        _ProgressButtonText.text = "Early Move";
                    }
                    else
                    {
                        _CurrentState = TurnStates.Battle;
                        _ProgressButtonText.text = "Battle";
                    }

                    if (_CurArmy._HasGuaranteedCard)
                        _CurArmy._HasStarReward = true;
                }
            }
            else if (_CurrentState == TurnStates.EarlyMove)
            {
                for (int i = 0; i < BoardComponent._BoardInstance._TroopsAdded.Count;)
                {
                    BoardComponent._BoardInstance._TroopsAdded[i]._AddedTroops = 0;
                    BoardComponent._BoardInstance._TroopsAdded[i]._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    if (BoardComponent._BoardInstance._TroopsAdded[i]._HasProxy)
                        BoardComponent._BoardInstance._TroopsAdded[i]._Proxy._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    BoardComponent._BoardInstance._TroopsAdded.Remove(BoardComponent._BoardInstance._TroopsAdded[i]);
                }

                MainCameraComponent._MainCameraInstance.ResetSelected();

                _CurrentState = TurnStates.Battle;
                _ProgressButtonText.text = "Battle";
            }
            else if (_CurrentState == TurnStates.Battle)
            {
                if (_HasAttacked)
                {
                    MainCameraComponent._MainCameraInstance.CmdResetCamera();

                    _CurrentState = TurnStates.Move;
                    _ProgressButtonText.text = "Move";
                    _HasAttacked = false;
                    _Warning.SetActive(false);
                }
                else
                {
                    if (_LocalPlayer._IsTurn)
                        _Warning.SetActive(true);
                    _HasAttacked = true;
                }
            }
            else if (_CurrentState == TurnStates.BattleMove)
            {
                _ProgressButton.gameObject.SetActive(false);

                for (int i = 0; i < BoardComponent._BoardInstance._TroopsAdded.Count;)
                {
                    BoardComponent._BoardInstance._TroopsAdded[i]._AddedTroops = 0;
                    BoardComponent._BoardInstance._TroopsAdded[i]._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    if (BoardComponent._BoardInstance._TroopsAdded[i]._HasProxy)
                        BoardComponent._BoardInstance._TroopsAdded[i]._Proxy._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    BoardComponent._BoardInstance._TroopsAdded.Remove(BoardComponent._BoardInstance._TroopsAdded[i]);
                }

                _CurrentState = TurnStates.Battle;
                _ProgressButtonText.text = "Battle";
            }
            else if (_CurrentState == TurnStates.Move)
            {
                for (int i = 0; i < BoardComponent._BoardInstance._TroopsAdded.Count;)
                {
                    BoardComponent._BoardInstance._TroopsAdded[i]._AddedTroops = 0;
                    BoardComponent._BoardInstance._TroopsAdded[i]._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    if (BoardComponent._BoardInstance._TroopsAdded[i]._HasProxy)
                        BoardComponent._BoardInstance._TroopsAdded[i]._Proxy._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    BoardComponent._BoardInstance._TroopsAdded.Remove(BoardComponent._BoardInstance._TroopsAdded[i]);
                }

                MainCameraComponent._MainCameraInstance.ResetSelected();

                if (_CurArmy._HasAdditionalMove)
                {
                    _CurrentState = TurnStates.AdditionalMove;
                    _ProgressButtonText.text = "Additional Move";
                }
                else if (_RewardCount == 0 && !_CurArmy._HasGuaranteedCard)
                {
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    SetRewards();
                    if (isServer)
                        ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        CmdNewTurn(TurnStates.CalculateTroops);
                    }
                    else
                    {
                        CmdNewTurn(TurnStates.PlaceTroops);
                    }
                }
                else if (_RewardCount == 0 && _CurArmy._HasGuaranteedCard)
                {
                    if (isServer)
                        SetStars();
                    _CurrentState = TurnStates.Reward;
                    _ProgressButtonText.text = "Reward";
                }
                else
                {
                    _RewardDisplay.SetActive(true);
                    _CurrentState = TurnStates.Reward;
                    _ProgressButtonText.text = "Reward";
                }
            }
            else if (_CurrentState == TurnStates.AdditionalMove)
            {
                for (int i = 0; i < BoardComponent._BoardInstance._TroopsAdded.Count;)
                {
                    BoardComponent._BoardInstance._TroopsAdded[i]._AddedTroops = 0;
                    BoardComponent._BoardInstance._TroopsAdded[i]._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    if (BoardComponent._BoardInstance._TroopsAdded[i]._HasProxy)
                        BoardComponent._BoardInstance._TroopsAdded[i]._Proxy._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                    BoardComponent._BoardInstance._TroopsAdded.Remove(BoardComponent._BoardInstance._TroopsAdded[i]);
                }

                MainCameraComponent._MainCameraInstance.ResetSelected();

                if (_RewardCount == 0 && !_CurArmy._HasGuaranteedCard)
                {
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    SetRewards();
                    if (isServer)
                        ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        CmdNewTurn(TurnStates.CalculateTroops);
                    }
                    else
                    {
                        CmdNewTurn(TurnStates.PlaceTroops);
                    }
                }
                else if (_RewardCount == 0 && _CurArmy._HasGuaranteedCard)
                {
                    if (isServer)
                        SetStars();
                    _CurrentState = TurnStates.Reward;
                    _ProgressButtonText.text = "Reward";
                }
                else
                {
                    _RewardDisplay.SetActive(true);
                    _CurrentState = TurnStates.Reward;
                    _ProgressButtonText.text = "Reward";
                }
            }
            else if (_CurrentState == TurnStates.Reward)
            {
                _RewardDisplay.SetActive(false);

                if (!_PlaceAirfield)
                {
                    _CurArmy._HasStarReward = false;
                    _CurArmy._PossibleRewards.Clear();

                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    SetRewards();
                    if (isServer)
                        ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);

                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        CmdNewTurn(TurnStates.CalculateTroops);
                    }
                    else
                    {
                        CmdNewTurn(TurnStates.PlaceTroops);
                    }
                }
                else
                {
                    _CurArmy._HasStarReward = false;
                    _CurArmy._PossibleRewards.Clear();
                    BoardComponent._BoardInstance._IncreaseButton[1].gameObject.SetActive(false);
                    BoardComponent._BoardInstance._DecreaseButton[1].gameObject.SetActive(false);

                    _CurrentState = TurnStates.PlaceAirfield;
                    _ProgressButtonText.text = "Place Airfield";
                }
            }
            else if (_CurrentState == TurnStates.PlaceCapital)
            {
                _TurnIndex++;
                if (_TurnIndex == _TurnOrder.Count)
                {
                    _TurnIndex--;
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    SetRewards();
                    if (isServer)
                        ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        CmdNewTurn(TurnStates.CalculateTroops);
                    }
                    else
                    {
                        CmdNewTurn(TurnStates.PlaceTroops);
                    }

                    BoardComponent._BoardInstance._IncreaseButton[1].gameObject.SetActive(true);
                    BoardComponent._BoardInstance._DecreaseButton[1].gameObject.SetActive(true);
                    _ProgressButton.interactable = true;
                }
                else
                {
                    _TurnIndex--;
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    SetRewards();

                    if (_LocalPlayer._Army._ArmyName == _CurArmy._Army._ArmyName)
                    {
                        int stars = _CurArmy._TwoStars * 2;
                        stars += _CurArmy._OneStars;
                        _StarsDisplay.color = _CurArmy._TextColour;
                        _StarsDisplay.text = "Stars: " + stars.ToString();
                    }
                    else
                    {
                        int cards = _CurArmy._TwoStars;
                        cards += _CurArmy._OneStars;
                        _StarsDisplay.color = _CurArmy._TextColour;
                        _StarsDisplay.text = "Cards: " + cards.ToString();
                    }
                }
            }
            else if (_CurrentState == TurnStates.PlaceAirfield)
            {
                BoardComponent._BoardInstance._IncreaseButton[1].gameObject.SetActive(true);
                BoardComponent._BoardInstance._DecreaseButton[1].gameObject.SetActive(true);
                _PlaceAirfield = false;

                _CurrentArmyBanner.color = FindActiveArmy();
                _CurArmy = _TurnOrder[_TurnIndex];
                SetRewards();
                if (isServer)
                    ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                _NewTroopsIcon.SetActive(true);
                if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                {
                    CmdNewTurn(TurnStates.CalculateTroops);
                }
                else
                {
                    CmdNewTurn(TurnStates.PlaceTroops);
                }
            }
        }
    }

    Color FindActiveArmy()
    {
        _TurnIndex++;
        if (_TurnIndex == _TurnOrder.Count)
            _TurnIndex = 0;

        if (_TurnOrder[_TurnIndex]._isDefeated)
            return FindActiveArmy();
        else
            return _TurnOrder[_TurnIndex]._Army._ArmyColour;
    }

    void SetRewards() //Sets what rewards the Current army has based on what is in their Reward List as that was updating across clients reliably and quickly
    {
        foreach (RewardScriptableObject r in _CurArmy._Rewards)
        {
            if (r._EarlyMove)
                _CurArmy._HasEarlyMove = true;
            else if (r._AdditionalMove)
                _CurArmy._HasAdditionalMove = true;
            else if (r._AttackDie)
                _CurArmy._HasAttackDie = true;
            else if (r._DefenceDie)
                _CurArmy._HasDefenceDie = true;
            else if (r._ExtraTroops)
                _CurArmy._HasExtraTroops = true;
            else if (r._GuaranteedCard)
                _CurArmy._HasGuaranteedCard = true;
        }

        if (!_CurArmy._HasGuaranteedCard)
            _CurArmy._HasStarReward = false;
    }

    [Command(requiresAuthority = false)]
    public void SetInfo()
    {
        for (int i = 0; i < _TurnOrder.Count; i++)
        {
            ArmiesStruct a = _TurnOrder[i];
            if (i < _ArmyOrder.content.childCount)
            {
                a._Info = _ArmyOrder.content.GetChild(i).GetComponent<ArmyInfoComponent>();
                _TurnOrder[i] = a;
            }
        }
    }

    [Command(requiresAuthority = false)]
    void SetStars() // Adds stars for the Army with guaranteed card but didn't get another reward for it to be called that way
    {
        int val = Random.Range(0, 3);

        if (val != 2)
        {
            val = 1;

            _CurArmy._OneStars++;
            _TurnOrder[_TurnIndex] = _CurArmy;
        }
        else
        {
            _CurArmy._TwoStars++;
            _TurnOrder[_TurnIndex] = _CurArmy;
        }

        DisplayCard(val);
    }

    [ClientRpc]
    void DisplayCard(int val)
    {
        _StarDisplay.DoCardReveal(val);
    }

    [Command(requiresAuthority =false)]
    public void IncreaseRewardCount()
    {
        GameCanvasComponent._GameInstance._RewardCount++;
    }

    [Command(requiresAuthority = false)]
    void CmdNewTurn(TurnStates state) //Command for starting a new turn to force set all clients to this point in case they didn't stay up to date for some reason
    {
        _RewardCount = 0;

        RpcNewTurn(state, _TurnIndex);
    }

    [ClientRpc]
    void RpcNewTurn(TurnStates state, int index)
    {
        _CurrentState = state;
        _TurnIndex = index;

        _RewardDisplay.SetActive(false);

        for (int i = 0; i < _RewardDisplay.transform.childCount;)
        {
            GameObject temp = _RewardDisplay.transform.GetChild(i).gameObject;
            temp.transform.parent = null;
            Destroy(temp);
        }

        BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
        _NewTroopsIcon.SetActive(true);

        if (_CurrentState == TurnStates.CalculateTroops)
        {
            if (_LocalPlayer._Army._ArmyName == _TurnOrder[_TurnIndex]._Army._ArmyName)
                _StarTrade.gameObject.SetActive(true);

            _ProgressButtonText.text = "Calculate Troops";
        }
        else if (_CurrentState == TurnStates.PlaceTroops)
        {
            if (isServer)
                ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
            _ProgressButtonText.text = "Place Troops";
        }

        if (_LocalPlayer._Army._ArmyName == _TurnOrder[_TurnIndex]._Army._ArmyName)
        {
            int stars = _CurArmy._TwoStars * 2;
            stars += _CurArmy._OneStars;
            _StarsDisplay.color = _CurArmy._TextColour;
            _StarsDisplay.text = "Stars: " + stars.ToString();
        }
        else
        {
            int cards = _CurArmy._TwoStars;
            cards += _CurArmy._OneStars;
            _StarsDisplay.color = _CurArmy._TextColour;
            _StarsDisplay.text = "Cards: " + cards.ToString();
        }
    }

    public void AddRewardAffect()
    {
        Vector3 temp = _RewardAddition.transform.localPosition;
        _RewardAddition.transform.localPosition = new Vector3(_RewardAddition.transform.localPosition.x + 100, _RewardAddition.transform.localPosition.y - 100, _RewardAddition.transform.localPosition.z);
        _RewardAddition.transform.DOLocalMove(temp, 1);
        _RewardAddition.transform.DOScale(Vector3.one, 1f);
        _RewardAddition._TextMeshPro.DOColor(Color.black, 1f);
    }

    void ResetRewardEffect(int index)
    {
        _RewardAddition._Image[0].DOColor(new Color(_RewardAddition._Image[0].color.r, _RewardAddition._Image[0].color.g, _RewardAddition._Image[0].color.b, 0), 0.5f).OnComplete(() => EndRewardEffect(index));
    }

    void EndRewardEffect(int index)
    {
        _RewardAddition.transform.localScale = Vector3.zero;
        _RewardAddition._Children[index].SetActive(false);
    }

    public void PlayRewardEffect()
    {
        if (!_Active)
        {
            _RewardCoroutine = RewardEffectCoroutine();
            StartCoroutine(_RewardCoroutine);
        }
    }

    IEnumerator RewardEffectCoroutine() //Coroutine for playing the little animation when you complete an objective and get a new reward to choose from, Coroutine allows for rewards to be played back to back if unlocked at the same time
    {
        _Active = true;

        for (int i = 0; i < _RewardEffectList.Count;)
        {
            _RewardAddition._Children[_RewardEffectList[i]._Index].SetActive(true);

            Vector3 temp = _RewardAddition.transform.localPosition;
            _RewardAddition.transform.localPosition = new Vector3(_RewardAddition.transform.localPosition.x + 100, _RewardAddition.transform.localPosition.y - 100, _RewardAddition.transform.localPosition.z);
            _RewardAddition.transform.DOLocalMove(temp, 1);
            _RewardAddition.transform.DOScale(Vector3.one, 1f);
            _RewardAddition._Image[0].DOColor(Color.white, 1f).OnComplete(() => ResetRewardEffect(_RewardEffectList[i]._Index));

            yield return new WaitForSecondsRealtime(1.5f); 

            _RewardEffectList.RemoveAt(0);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        _Active = false;
        if (_RewardEffectList.Count != 0)
        {
            PlayRewardEffect();
        }
    }
}
