using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour
{
    public int value = -1;
    public RewardDisplay _Parent;
    public GameObject[] _Icons;
    public Button _Button;

    private void OnEnable()
    {
        if (_Button == null)
            _Button = GetComponent<Button>();

        if (GameCanvasComponent._GameInstance._LocalPlayer._IsTurn)
            _Button.interactable = true;
        else
            _Button.interactable = false;

    }
    public void SelectReward()
    {
        _Parent.SelectReward(value);
    }
}
