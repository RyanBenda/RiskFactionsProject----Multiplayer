using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Type", menuName = "ScriptableObjects/Reward", order = 2)]
public class RewardScriptableObject : ScriptableObject
{
    public string _Name;

    public bool _Card = false;
    public bool _Airfield = false;

    public bool _EarlyMove = false;
    public bool _AdditionalMove = false;
    public bool _AttackDie = false;
    public bool _DefenceDie = false;
    public bool _ExtraTroops = false;
    public bool _GuaranteedCard = false;

    public int _Index = 0;


    public void DetermineReward(/*ArmiesClass2 Army*/)
    {
        if (_Airfield == true)
        {
            GameCanvasComponent._GameInstance._PlaceAirfield = true;
        }
        else if (_EarlyMove)
            GameCanvasComponent._GameInstance._CurArmy._HasEarlyMove = true;
        else if (_AdditionalMove)
            GameCanvasComponent._GameInstance._CurArmy._HasAdditionalMove = true;
        else if (_AttackDie)
            GameCanvasComponent._GameInstance._CurArmy._HasAttackDie = true;
        else if (_DefenceDie)
            GameCanvasComponent._GameInstance._CurArmy._HasDefenceDie = true;
        else if (_ExtraTroops)
            GameCanvasComponent._GameInstance._CurArmy._HasExtraTroops = true;
        else if (_GuaranteedCard)
            GameCanvasComponent._GameInstance._CurArmy._HasGuaranteedCard = true;
        
    }
    public int DetermineRewardCard(/*ArmiesClass Army*/)
    {
        int value = Random.Range(0, 3);

        if (value != 2)
            value = 1;

        return value;

        /*if (_Card == true)
        {
            if (CardValue() == 1)
            {
                Army._OneStars++;
            }
            else
                Army._TwoStars++;
        }
        else*/
    }
}
