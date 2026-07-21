using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CapitalPlacement
{
    public CountryComponent Country;
    public float val;
}

public class CapitalPlacementComponent : MonoBehaviour
{

    //public ArmiesClass _Army;


    public bool _test = false;

    //[System.Serializable]
    public List<CapitalPlacement> _Options = new List<CapitalPlacement>();


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (_test)
        {
            _test = false;
            _Options.Clear();
            foreach (CountryComponent c in GameCanvasComponent._GameInstance._CurArmy._ControlledCountries)
            {
                CapitalPlacement o = new CapitalPlacement();
                o.Country = c;
                o.val = 5;

                foreach (CountryComponent n in o.Country._NeighbouringCountries)
                {
                    if (n._CurColour == c._CurColour)
                    {
                        o.val -= 0.5f;

                        o.val += (float)n._TroopsCount * 0.1f;
                    }
                    else if (n._CurColour != c._CurColour)
                    {
                        o.val -= 1f;

                        o.val -= (float)n._TroopsCount * 0.1f;
                    }

                    if (n._IsCapital)
                        o.val -= 0.5f;
                }

                if (o.Country._IsBorderCountry)
                    o.val -= 0.5f;

                o.val += 0.2f * o.Country._TroopsCount;
                
                float temp = 0;
                float temp2 = 0;
                foreach (CountryComponent ac in c._Continent._Countries)
                {
                    if (ac._CurColour == c._CurColour)
                        temp++;
                    else
                        temp2++;
                }
                o.val += (temp / c._Continent._Countries.Length) * 4;
                o.val -= temp2 * 0.1f;
                
                _Options.Add(o);
            }

            _Options.Sort((a, b) => b.val.CompareTo(a.val));
        }
    }
}
