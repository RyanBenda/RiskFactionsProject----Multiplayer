using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinentComponent : MonoBehaviour
{
    public CountryComponent[] _Countries;

    public ArmiesClass2 _ControllingArmy;

    public int _TroopValue = 4;

    public int _ContinentOrder = -1;
    public void CheckCountries(Color countryColour)
    {
        bool fullcontrol = true;
        for (int i = 0; i < _Countries.Length; i++)
        {
            if (_Countries[i]._CurColour != countryColour)
            {
                fullcontrol = false;
                break;
            }
        }

        if (fullcontrol)
            _ControllingArmy = GameCanvasComponent._GameInstance._CurArmy;
        else
            _ControllingArmy = default;
    }
}
