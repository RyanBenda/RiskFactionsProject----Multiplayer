using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StarTradeComponent : MonoBehaviour
{
    public TextMeshProUGUI _StarCount;

    public Button[] _TradeOptions;
    public TextMeshProUGUI[] _TradeOptionsText;

    void OnEnable()
    {
        int stars = 0;
        bool hasOddCard = false;
        for (int i = 0; i < GameCanvasComponent._GameInstance._CurArmy._OneStars; i++)
        {
            stars++;
            hasOddCard = true;
        }

        for (int i = 0; i < GameCanvasComponent._GameInstance._CurArmy._TwoStars; i++)
        {
            stars += 2;
        }

        _StarCount.text = "Star Count: "+ stars;

        bool temp = false;
        for (int i = 0; i < _TradeOptions.Length; i++)
        {
            if (stars >= i + 2)
            {
                if (!temp || hasOddCard)
                {
                    _TradeOptions[i].enabled = true;
                    _TradeOptionsText[i].color = Color.white;
                }
                else
                {
                    _TradeOptions[i].enabled = false;
                    _TradeOptionsText[i].color = Color.black;
                }
            }
            else
            {
                _TradeOptions[i].enabled = false;
                _TradeOptionsText[i].color = Color.black;
            }

            temp = !temp;
        }
    }

    private void OnDisable()
    {
        ObjectiveManager._ObjectiveManagerInstance.ObjectiveCheck();
        int stars = GameCanvasComponent._GameInstance._CurArmy._TwoStars * 2;
        stars += GameCanvasComponent._GameInstance._CurArmy._OneStars;

        GameCanvasComponent._GameInstance._StarsDisplay.text = "Stars: " + stars.ToString();
    }

    private void Update()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.CalculateTroops && Input.GetKeyDown(KeyCode.Escape))
        {
            GameCanvasComponent._GameInstance.ProgressTurn();
        }
    }

    public void TradeIn(int value)
    {

        int newTroops = 0;
        switch (value)
        {
            case 2:
                newTroops = 2;
                break;
            case 3:
                newTroops = 4;
                break;
            case 4:
                newTroops = 7;
                break;
            case 5:
                newTroops = 10;
                break;
            case 6:
                newTroops = 13;
                break;
            case 7:
                newTroops = 17;
                break;
            case 8:
                newTroops = 21;
                break;
            case 9:
                newTroops = 25;
                break;
            case 10:
                newTroops = 30;
                break;
        }


        while (value > 1 && GameCanvasComponent._GameInstance._CurArmy._TwoStars > 0)
        {
            GameCanvasComponent._GameInstance._CurArmy._TwoStars--;
            value -= 2;
        }

        while (value > 0)
        {
            GameCanvasComponent._GameInstance._CurArmy._OneStars--;
            value -= 1;
        }

        BoardComponent._BoardInstance._NewTroops += newTroops;

        BoardComponent._BoardInstance._NewTroopsCount.text = BoardComponent._BoardInstance._NewTroops.ToString();

        GameCanvasComponent._GameInstance.ProgressTurn();
    }
}
