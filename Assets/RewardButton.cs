using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardButton : MonoBehaviour
{
    public int value = -1;
    public RewardDisplay _Parent;
    public GameObject[] _Icons;

    public void SelectReward()
    {
        _Parent.SelectReward(value);
    }
}
