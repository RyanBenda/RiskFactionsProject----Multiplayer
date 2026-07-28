using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardDisplay : MonoBehaviour
{
    public GameObject _ButtonPrefab;
    public ObjectiveDisplay[] _Objectives;

    int _Val = 0;

    public StarsAdditionComponent _StarDisplay;

    private void OnEnable()
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

    public void SelectReward(int value)
    {
        if (!GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._Card)
        {
            GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value].DetermineReward(GameCanvasComponent._GameInstance._CurArmy);

            GameCanvasComponent._GameInstance._CurArmy._Rewards.Add(GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]);

            for (int i = 0; i < ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives.Count; i++)
            {
                if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value] == ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives[i]._Reward)
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
                            _Objectives[j]._Claimed.color = GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour;
                            _Objectives[j]._Claimed.gameObject.SetActive(true);
                            break;
                        }
                    }

                    ObjectiveManager._ObjectiveManagerInstance._InactiveObjectives.Clear();
                    break;
                }
            }
            
        }
        
        for (int i = 0; i < this.transform.childCount;)
        {
            GameObject temp = this.transform.GetChild(i).gameObject;
            temp.transform.parent = null;
            Destroy(temp);
        }

        if (GameCanvasComponent._GameInstance._CurArmy._PossibleRewards[value]._Card || GameCanvasComponent._GameInstance._CurArmy._HasGuaranteedCard)
        {
            int val = Random.Range(0, 3);

            if (val != 2)
            { 
                val = 1;

                GameCanvasComponent._GameInstance._CurArmy._OneStars++;
            }
            else
                GameCanvasComponent._GameInstance._CurArmy._TwoStars++;

            this.gameObject.SetActive(false);
            _StarDisplay.DoCardReveal(val);
        }
        else
        {
            this.gameObject.SetActive(false);
            GameCanvasComponent._GameInstance.CmdProgressTurn();
        }

    }
}
