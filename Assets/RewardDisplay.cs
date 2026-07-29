using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class RewardDisplay : NetworkBehaviour
{
    public GameObject _ButtonPrefab;
    public ObjectiveDisplay[] _Objectives;

    int _Val = 0;

    public StarsAdditionComponent _StarDisplay;

    private void OnEnable()
    {
        if (isClient)
        {
            _Val = 0;
            foreach (RewardScriptableObject r in GameCanvasComponent._GameInstance._CurArmy._PossibleRewards)
            {
                GameObject newButton = Instantiate(_ButtonPrefab);
                RewardButton rb = newButton.GetComponent<RewardButton>();
                rb.value = _Val;
                newButton.transform.parent = this.transform;
                rb._Parent = this;
                newButton.transform.localScale = Vector3.one;
                newButton.SetActive(true);

                rb._Icons[r._Index].SetActive(true);


                _Val++;
            }
        }
    }
    
    [Command(requiresAuthority = false)]
    public void SelectReward(int value)
    {
        if (!GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._Card)
        {
            //GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value].DetermineReward(/*GameCanvasComponent._GameInstance._CurArmy*/);
            //DetermineReward(GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]);

            if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._Airfield == true)
            {
                GameCanvasComponent._GameInstance._PlaceAirfield = true;
            }
            else if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._EarlyMove)
                GameCanvasComponent._GameInstance._CurArmy._HasEarlyMove = true;
            else if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._AdditionalMove)
                GameCanvasComponent._GameInstance._CurArmy._HasAdditionalMove = true;
            else if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._AttackDie)
                GameCanvasComponent._GameInstance._CurArmy._HasAttackDie = true;
            else if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._DefenceDie)
                GameCanvasComponent._GameInstance._CurArmy._HasDefenceDie = true;
            else if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._ExtraTroops)
                GameCanvasComponent._GameInstance._CurArmy._HasExtraTroops = true;
            else if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._GuaranteedCard)
                GameCanvasComponent._GameInstance._CurArmy._HasGuaranteedCard = true;

            GameCanvasComponent._GameInstance._CurArmy._Rewards.Add(GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]);

            for (int i = 0; i < ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives.Count; i++)
            {
                if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._Name == ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives[i]._Reward._Name)
                {
                    //ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives.RemoveAt(i);

                    for (int j = 0; j < ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives.Count; j++)
                    {
                        if (j != i)
                            ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives[j]._Inactive = false;
                    }

                    for (int j = 0; j < _Objectives.Length; j++)
                    {
                        if (_Objectives[j]._Objective == ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives[i])
                        {
                            ObjectiveClaimed(j);
                            break;
                        }
                    }

                    ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives.Clear();
                    break;
                }
            }
            
        }

        ClearChildren();

        if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._Card || GameCanvasComponent._GameInstance._CurArmy._HasGuaranteedCard)
        {
            int val = Random.Range(0, 3);

            if (val != 2)
            {
                val = 1;

                GameCanvasComponent._GameInstance._CurArmy._OneStars++;

                for (int i = 0; i < GameCanvasComponent._GameInstance._TurnOrder.Count; i++)
                {
                    if (GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyName == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName)
                    {
                        GameCanvasComponent._GameInstance._TurnOrder[i] = GameCanvasComponent._GameInstance._CurArmy;
                    }
                }
            }
            else
            {
                for (int i = 0; i < GameCanvasComponent._GameInstance._TurnOrder.Count; i++)
                {
                    if (GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyName == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName)
                    {
                        GameCanvasComponent._GameInstance._TurnOrder[i] = GameCanvasComponent._GameInstance._CurArmy;
                    }
                }
            }

            RecievedCard(val);
        }
        else
        {
            this.gameObject.SetActive(false);
            GameCanvasComponent._GameInstance.CmdProgressTurn();
        }

    }

    [ClientRpc]
    void ObjectiveClaimed(int j)
    {
        _Objectives[j]._Claimed.color = GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour;
        _Objectives[j]._Claimed.gameObject.SetActive(true);
    }

    [ClientRpc]
    void ClearChildren()
    {
        for (int i = 0; i < this.transform.childCount;)
        {
            GameObject temp = this.transform.GetChild(i).gameObject;
            temp.transform.parent = null;
            Destroy(temp);
        }
    }

    [ClientRpc]
    void RecievedCard(int val)
    {
        this.gameObject.SetActive(false);
        _StarDisplay.DoCardReveal(val);
    }
}
