using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

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
public class ArmiesClass
{
    public ArmyScriptableObject _Army;

    public int _OneStars = 0;
    public int _TwoStars = 0;

    public bool _HasEarlyMove = false;
    public bool _HasAdditionalMove = false;
    public bool _HasAttackDie = false;
    public bool _HasDefenceDie = false;
    public bool _HasExtraTroops = false;
    public bool _HasGuaranteedCard = false;

    public bool _HasStarReward = false;
    public List<RewardScriptableObject> _Rewards = new List<RewardScriptableObject>();
    public List<RewardScriptableObject> _PossibleRewards = new List<RewardScriptableObject>();

    public bool _isDefeated = false;
    public Color _TextColour = Color.black;
    public ArmyInfoComponent _Info;

    public List<CountryComponent> _ControlledCountries = new List<CountryComponent>();
}


public class GameCanvasComponent : MonoBehaviour
{
    public static GameCanvasComponent _GameInstance;

    public Image _CurrentArmyBanner;
    public TextMeshProUGUI _StarsDisplay;
    public Button _ProgressButton;
    public TextMeshProUGUI _ProgressButtonText;
    public TurnStates _CurrentState = TurnStates.CalculateTroops;

    public List<ArmiesClass> _TurnOrder;
    int _TurnIndex = 0;
    public ArmiesClass _CurArmy;

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

    public bool _HasAttacked = false;
    public GameObject _Warning;

    public ScrollRect _ArmyOrder;
    public GameObject _ArmyTabPrefab;
    public bool _DisplayActive = false;
    TurnStates _LastState;

    // Start is called before the first frame update
    void Start()
    {
        if (_GameInstance == null)
            _GameInstance = this;

        _ProgressButtonText.text = "Place Capital";
        _ProgressButton.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha9))
            _CurArmy._TwoStars++;

        if (Input.GetKeyDown(KeyCode.Alpha8))
            _CurArmy._OneStars++;

        if (Input.GetKeyDown(KeyCode.Alpha7))
            _CurArmy._TwoStars--;

        if (Input.GetKeyDown(KeyCode.Alpha6))
            _CurArmy._OneStars--;
    }

    public void TurnOnOffDisplay()
    {
        if (!_DisplayActive && !ObjectiveManager._ObjectiveManagerInstance._DisplayActive)
        {
            _DisplayActive = true;
            _LastState = _CurrentState;
            _CurrentState = TurnStates.Suspend;
            _ArmyOrder.gameObject.SetActive(true);
        }
        else
        {
            _ArmyOrder.gameObject.SetActive(false);
            _CurrentState = _LastState;
            _DisplayActive = false;
        }
    }

    public void TurnOffWarning()
    {
        _Warning.SetActive(false);
    }

    public void ProgressTurn()
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
                    MainCameraComponent._MainCameraInstance.ResetCamera();

                    for (int i = 0; i < BoardComponent._BoardInstance._TroopsAdded.Count;)
                    {
                        BoardComponent._BoardInstance._TroopsAdded[i]._AddedTroops = 0;
                        BoardComponent._BoardInstance._TroopsAdded[i]._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                        if (BoardComponent._BoardInstance._TroopsAdded[i]._HasProxy)
                            BoardComponent._BoardInstance._TroopsAdded[i]._Proxy._TroopDisplay.color = BoardComponent._BoardInstance._TroopsAdded[i]._OccupyingArmy._TextColour;

                        BoardComponent._BoardInstance._TroopsAdded.Remove(BoardComponent._BoardInstance._TroopsAdded[i]);
                    }

                    _NewTroopsIcon.SetActive(false);

                    if (_TurnOrder[_TurnIndex]._HasEarlyMove)
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
                    MainCameraComponent._MainCameraInstance.ResetCamera();

                    _CurrentState = TurnStates.Move;
                    _ProgressButtonText.text = "Move";
                    _HasAttacked = false;
                    _Warning.SetActive(false);
                }
                else
                {
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

                if (_TurnOrder[_TurnIndex]._HasAdditionalMove)
                {
                    _CurrentState = TurnStates.AdditionalMove;
                    _ProgressButtonText.text = "Additional Move";
                }
                else if (_CurArmy._PossibleRewards.Count == 0 && !_CurArmy._HasGuaranteedCard)
                {
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        _StarTrade.gameObject.SetActive(true);
                        _CurrentState = TurnStates.CalculateTroops;
                        _ProgressButtonText.text = "Calculate Troops";
                    }
                    else
                    {
                        _CurrentState = TurnStates.PlaceTroops;
                        _ProgressButtonText.text = "Place Troops";
                        ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
                    }
                    int stars = _CurArmy._TwoStars * 2;
                    stars += _CurArmy._OneStars;

                    _StarsDisplay.color = _CurArmy._TextColour;
                    _StarsDisplay.text = "Stars: " + stars.ToString();
                }
                else if (_CurArmy._PossibleRewards.Count == 0 && _CurArmy._HasGuaranteedCard)
                {
                    int val = Random.Range(0, 3);

                    if (val != 2)
                    {
                        val = 1;

                        _CurArmy._OneStars++;
                    }
                    else
                        _CurArmy._TwoStars++;

                    _StarDisplay.DoCardReveal(val);
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

                if (_CurArmy._PossibleRewards.Count == 0)
                {
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        _StarTrade.gameObject.SetActive(true);
                        _CurrentState = TurnStates.CalculateTroops;
                        _ProgressButtonText.text = "Calculate Troops";
                    }
                    else
                    {
                        _CurrentState = TurnStates.PlaceTroops;
                        _ProgressButtonText.text = "Place Troops";
                        ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
                    }
                    int stars = _CurArmy._TwoStars * 2;
                    stars += _CurArmy._OneStars;

                    _StarsDisplay.color = _CurArmy._TextColour;
                    _StarsDisplay.text = "Stars: " + stars.ToString();
                }
                else
                {
                    _RewardDisplay.SetActive(true);
                    _CurrentState = TurnStates.Reward;
                    _ProgressButtonText.text = "Reward";
                }
            }
            else if (_CurrentState == TurnStates.Reward && _RewardDisplay.activeSelf == false)
            {
                if (!_PlaceAirfield)
                {
                    _CurArmy._HasStarReward = false;
                    _CurArmy._PossibleRewards.Clear();

                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];
                    ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        _StarTrade.gameObject.SetActive(true);
                        _CurrentState = TurnStates.CalculateTroops;
                        _ProgressButtonText.text = "Calculate Troops";
                    }
                    else
                    {
                        _CurrentState = TurnStates.PlaceTroops;
                        _ProgressButtonText.text = "Place Troops";
                        ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
                    }
                    int stars = _CurArmy._TwoStars * 2;
                    stars += _CurArmy._OneStars;

                    _StarsDisplay.color = _CurArmy._TextColour;
                    _StarsDisplay.text = "Stars: " + stars.ToString();
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
                    ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                    BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                    _NewTroopsIcon.SetActive(true);
                    if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                    {
                        _StarTrade.gameObject.SetActive(true);
                        _CurrentState = TurnStates.CalculateTroops;
                        _ProgressButtonText.text = "Calculate Troops";
                    }
                    else
                    {
                        _CurrentState = TurnStates.PlaceTroops;
                        _ProgressButtonText.text = "Place Troops";
                        ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
                    }
                    int stars = _CurArmy._TwoStars * 2;
                    stars += _CurArmy._OneStars;

                    _StarsDisplay.color = _CurArmy._TextColour;
                    _StarsDisplay.text = "Stars: " + stars.ToString();

                    BoardComponent._BoardInstance._IncreaseButton[1].gameObject.SetActive(true);
                    BoardComponent._BoardInstance._DecreaseButton[1].gameObject.SetActive(true);
                    _ProgressButton.interactable = true;
                }
                else
                {
                    _TurnIndex--;
                    _CurrentArmyBanner.color = FindActiveArmy();
                    _CurArmy = _TurnOrder[_TurnIndex];

                    int stars = _CurArmy._TwoStars * 2;
                    stars += _CurArmy._OneStars;

                    _StarsDisplay.color = _CurArmy._TextColour;
                    _StarsDisplay.text = "Stars: " + stars.ToString();
                }
            }
            else if (_CurrentState == TurnStates.PlaceAirfield)
            {
                BoardComponent._BoardInstance._IncreaseButton[1].gameObject.SetActive(true);
                BoardComponent._BoardInstance._DecreaseButton[1].gameObject.SetActive(true);
                _PlaceAirfield = false;

                _CurrentArmyBanner.color = FindActiveArmy();
                _CurArmy = _TurnOrder[_TurnIndex];
                ObjectiveManager._ObjectiveManagerInstance.ResetManager();
                BoardComponent._BoardInstance.CalculateNewTroops(_TurnIndex);
                _NewTroopsIcon.SetActive(true);
                if (_CurArmy._OneStars >= 2 || _CurArmy._TwoStars >= 1)
                {
                    _StarTrade.gameObject.SetActive(true);
                    _CurrentState = TurnStates.CalculateTroops;
                    _ProgressButtonText.text = "Calculate Troops";
                }
                else
                {
                    _CurrentState = TurnStates.PlaceTroops;
                    _ProgressButtonText.text = "Place Troops";
                    ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
                }
                int stars = _CurArmy._TwoStars * 2;
                stars += _CurArmy._OneStars;

                _StarsDisplay.color = _CurArmy._TextColour;
                _StarsDisplay.text = "Stars: " + stars.ToString(); 
            }
        }
    }

    Color FindActiveArmy() // Shouldn't be possible to have all the armies defeated in a game cause then there wouldn't be a winner, but probs should add a check at some point
    {
        _TurnIndex++;
        if (_TurnIndex == _TurnOrder.Count)
            _TurnIndex = 0;

        if (_TurnOrder[_TurnIndex]._isDefeated)
            return FindActiveArmy();
        else
            return _TurnOrder[_TurnIndex]._Army._ArmyColour;
    }

    public void AddRewardAffect()
    {
        Vector3 temp = _RewardAddition.transform.localPosition;
        _RewardAddition.transform.localPosition = new Vector3(_RewardAddition.transform.localPosition.x + 100, _RewardAddition.transform.localPosition.y - 100, _RewardAddition.transform.localPosition.z);
        _RewardAddition.transform.DOLocalMove(temp, 1);
        _RewardAddition.transform.DOScale(Vector3.one, 1f);
        //_RewardAddition._Image.DOColor(Color.white, 1f).OnComplete(() => ResetRewardAffect());
        _RewardAddition._TextMeshPro.DOColor(Color.black, 1f);
        
    }

    void ResetRewardEffect(int index)
    {
        _RewardAddition._Image[0].DOColor(new Color(_RewardAddition._Image[0].color.r, _RewardAddition._Image[0].color.g, _RewardAddition._Image[0].color.b, 0), 0.5f).OnComplete(() => EndRewardEffect(index));
        //_RewardAddition._TextMeshPro.DOColor(new Color(_RewardAddition._TextMeshPro.color.r, _RewardAddition._TextMeshPro.color.g, _RewardAddition._TextMeshPro.color.b, 0), 0.5f).OnComplete(() => EndRewardEffect(index));
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


    IEnumerator RewardEffectCoroutine()
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
            //_RewardAddition._TextMeshPro.DOColor(Color.black, 1f);

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
