using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class StarTradeComponent : NetworkBehaviour
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

        //if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
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

        if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
            GameCanvasComponent._GameInstance._StarsDisplay.text = "Stars: " + stars.ToString();
    }

    private void Update()
    {
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.CalculateTroops && Input.GetKeyDown(KeyCode.Escape))
        {
            GameCanvasComponent._GameInstance.ProgressTurn();
        }
    }

    [Command(requiresAuthority = false)]
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

        for (int i = 0; i < GameCanvasComponent._GameInstance._TurnOrder.Count; i++)
        {
            if (GameCanvasComponent._GameInstance._TurnOrder[i]._Army._ArmyName == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName)
            {
                GameCanvasComponent._GameInstance._TurnOrder[i] = GameCanvasComponent._GameInstance._CurArmy;
            }
        }

        BoardComponent._BoardInstance._NewTroops += newTroops;

        BoardComponent._BoardInstance._NewTroopsCount.text = BoardComponent._BoardInstance._NewTroops.ToString();

        UpdateCardText(GameCanvasComponent._GameInstance._CurArmy._OneStars, GameCanvasComponent._GameInstance._CurArmy._TwoStars);
        GameCanvasComponent._GameInstance.CmdProgressTurn();
    }

    [ClientRpc]

    void UpdateCardText(int oneStar, int twoStar)
    {

        if (GameCanvasComponent._GameInstance._LocalPlayer._Army._ArmyName == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyName)
        {
            GameCanvasComponent._GameInstance._CurArmy._OneStars = oneStar;
            GameCanvasComponent._GameInstance._CurArmy._OneStars = twoStar;

            int stars = twoStar * 2;
            stars += oneStar;
            GameCanvasComponent._GameInstance._StarsDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;
            GameCanvasComponent._GameInstance._StarsDisplay.text = "Stars: " + stars.ToString();
        }
        else
        {
            int cards = twoStar;
            cards += oneStar;
            GameCanvasComponent._GameInstance._StarsDisplay.color = GameCanvasComponent._GameInstance._CurArmy._TextColour;
            GameCanvasComponent._GameInstance._StarsDisplay.text = "Cards: " + cards.ToString();
        }
    }
}
