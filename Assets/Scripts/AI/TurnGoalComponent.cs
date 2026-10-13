using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct TurnGoalStruct
{
    public string _Goal;
    public float _Val;
    public List<CountryComponent> _Path;
}

struct BestStartStruct
{
    public CountryComponent _StartCountry;
    public CountryComponent _EndCountry;
    public int _Dist;
    public int _TroopCount;
    public float _Val;

    public List<CountryComponent> _Path;
}

public class TurnGoalComponent : MonoBehaviour
{
    public bool _test = false;

    public List<TurnGoalStruct> _Options = new List<TurnGoalStruct>();

    // Update is called once per frame
    void Update()
    {
        if (_test)
        {
            _test = false;

            for (int i = 0; i < _Options.Count; i++)
            {
                if (_Options[i]._Goal == "Get a Star")
                {
                    float v = 0;
                    CountryComponent a = null;
                    CountryComponent d = null;

                    foreach (CountryComponent c in GameCanvasComponent._GameInstance._CurArmy._ControlledCountries)
                    {
                        foreach (CountryComponent n in c._NeighbouringCountries)
                        {
                            if (n._CurColour != c._CurColour)
                            {
                                float t = c._TroopsCount; // + BoardComponent._BoardInstance._NewTroops
                                t -= n._TroopsCount * 2;

                                if (n._HasCity)
                                {
                                    t += 0.1f;
                                }

                                /*if (n._IsCapital)
                                {
                                    t += 3;
                                }*/

                                if (n._HasAirfieldEffect)
                                    t -= 0.5f;

                                for (int j = 0; j < GameCanvasComponent._GameInstance._TurnOrder.Count; j++)
                                {
                                    if (GameCanvasComponent._GameInstance._TurnOrder[j]._Army._ArmyName == n._OccupyingArmy._Army._ArmyName)
                                    {
                                        foreach (RewardScriptableObject r in GameCanvasComponent._GameInstance._TurnOrder[j]._Rewards)
                                        {
                                            if (r._DefenceDie)
                                                t -= 0.5f;
                                        }
                                        break;
                                    }
                                }

                                if (c._IsBorderCountry)
                                    t -= 1;

                                float p = 0;
                                for (int j = 0; j < n._Continent._Countries.Length; j++)
                                {
                                    if (n._Continent._Countries[j]._CurColour == c._CurColour)
                                        p++;
                                }

                                t += (p / n._Continent._Countries.Length);

                                Debug.Log(c.name + " " + n.name + " " + t);

                                if (t > v || v == 0)
                                {
                                    v = t;
                                    a = c;
                                    d = n;
                                }
                            }
                        }
                    }

                    TurnGoalStruct g = new TurnGoalStruct();
                    g._Goal = _Options[i]._Goal;
                    g._Val = v;
                    g._Path = new List<CountryComponent>();
                    g._Path.Add(a);
                    g._Path.Add(d);

                    _Options[i] = g;
                }

                if (_Options[i]._Goal == "Take Control of Oceania")
                {
                    List<CountryComponent> startpos = new List<CountryComponent>();
                    List<BestStartStruct> bsl = new List<BestStartStruct>();

                    foreach (CountryComponent c in GameCanvasComponent._GameInstance._CurArmy._ControlledCountries) // AI should be handled by Host so this should be okay to do
                    {
                        foreach (CountryComponent b in BoardComponent._BoardInstance._Continents[5]._Countries)
                        {
                            if (b._IsBorderCountry)
                            {
                                List<CountryComponent> path = DijkstrasSearch(c, b, false);

                                if (path.Count > 0)
                                {
                                    BestStartStruct s = new BestStartStruct();
                                    s._StartCountry = c;
                                    s._EndCountry = b;
                                    s._Dist = path.Count - 1;
                                    s._TroopCount = path[0]._TroopsCount;
                                    s._Val = ((float)s._TroopCount / 2) - s._Dist;

                                    s._Path = path;

                                    if (c._IsBorderCountry)
                                        s._Val -= 0.01f;

                                    for (int j = 1; j < path.Count; j++)
                                    {
                                        s._Val -= (float)path[j]._TroopsCount * 0.35f;
                                    }


                                    bool foundCountry = false;
                                    for (int k = 0; k < bsl.Count; k++)
                                    {
                                        if (bsl[k]._StartCountry == s._StartCountry)
                                        {
                                            foundCountry = true;

                                            if (bsl[k]._Val < s._Val)
                                                bsl[k] = s;

                                            break;
                                        }
                                    }

                                    if (!foundCountry)
                                        bsl.Add(s);
                                }
                            }
                        }
                    }

                    if (bsl.Count == 0)
                    {
                        CountryComponent ctemp = null;
                        CountryComponent ctemp2 = null;
                        foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[5]._Countries)
                        {
                            if (c._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                            {
                                CountryComponent ctemp3 = null;
                                foreach (CountryComponent c2 in c._NeighbouringCountries)
                                {
                                    if (c2._Continent == BoardComponent._BoardInstance._Continents[5] && c2._CurColour != c._CurColour)
                                        ctemp3 = c2;
                                }

                                if (ctemp == null && ctemp3 != null)
                                {
                                    ctemp = c;
                                    ctemp2 = ctemp3;
                                }
                                else if (ctemp3 != null && c._TroopsCount > ctemp._TroopsCount)
                                {
                                    ctemp = c;
                                    ctemp2 = ctemp3;
                                }
                            }
                        }

                        BestStartStruct s = new BestStartStruct();
                        s._StartCountry = ctemp;
                        s._EndCountry = ctemp;
                        s._Dist = 1;
                        s._TroopCount = ctemp._TroopsCount;
                        s._Val = ((float)s._TroopCount / 2) - s._Dist;

                        List<CountryComponent> list = new List<CountryComponent>();
                        list.Add(ctemp);
                        list.Add(ctemp2);
                        s._Path = list;

                        bsl.Add(s);
                    }

                    bsl.Sort((a, b) => b._Val.CompareTo(a._Val));

                    List<CountryComponent> finalPath = bsl[0]._Path;//DijkstrasSearch(bsl[0]._StartCountry, bsl[0]._EndCountry, false);

                    List<CountryComponent> continentCountries = new List<CountryComponent>();
                    foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[5]._Countries)
                    {
                        if (!finalPath.Contains(c) && c._CurColour != finalPath[0]._CurColour)
                        {
                            continentCountries.Add(c);
                        }
                    }

                    List<List<CountryComponent>> tempPath = new List<List<CountryComponent>>();
                    List<CountryComponent> temp = new List<CountryComponent>();
                    temp.Add(finalPath[finalPath.Count - 1]);
                    tempPath.Add(temp);
                    bool foundAll = false;
                    for (int j = 0; j < tempPath.Count; j++)
                    {
                        List<CountryComponent> t = tempPath[j];

                        if (t.Count == continentCountries.Count + 1)
                        {
                            List<CountryComponent> t1 = t;
                            t1.RemoveAt(0); // Possibly should check if end of final path is the same as 0 before removing it but 99% confident it will always be in this scenario
                            tempPath.Clear();
                            tempPath.Add(t1);
                            foundAll = true;
                            break;
                        }

                        foreach (CountryComponent c in t[t.Count - 1]._NeighbouringCountries)
                        {
                            if (c._Continent == finalPath[finalPath.Count - 1]._Continent && c._CurColour != finalPath[0]._CurColour && !t.Contains(c))
                            {
                                List<CountryComponent> t1 = t;
                                t1.Add(c);
                                tempPath.Add(t1);
                            }
                        }
                    }



                    //List<CountryComponent> finalPath = DijkstrasSearch(bsl[0]._StartCountry, bsl[0]._EndCountry, false);

                    if (foundAll)
                    {
                        foreach (CountryComponent c in tempPath[0])
                        {
                            finalPath.Add(c);
                        }
                    }
                    else
                    {
                        tempPath.Sort((a, b) => b.Count.CompareTo(a.Count));

                        tempPath[0].RemoveAt(0); // Possibly should check if end of final path is the same as 0 before removing it but 99% confident it will always be in this scenario

                        foreach (CountryComponent c in tempPath[0])
                        {
                            finalPath.Add(c);
                        }

                        List<CountryComponent> remainingCountries = new List<CountryComponent>();
                        List<CountryComponent> ownedCountries = new List<CountryComponent>();
                        foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[5]._Countries)
                        {
                            if (!finalPath.Contains(c) && c._CurColour != finalPath[0]._CurColour)
                            {
                                remainingCountries.Add(c);
                            }
                            else if (c._CurColour == finalPath[0]._CurColour)
                            {
                                foreach (CountryComponent c2 in c._NeighbouringCountries)
                                {
                                    if (c2._Continent == BoardComponent._BoardInstance._Continents[5] && c2._CurColour != finalPath[0]._CurColour && !finalPath.Contains(c2))
                                        ownedCountries.Add(c);
                                }
                            }
                        }

                        ownedCountries.Sort((a, b) => b._TroopsCount.CompareTo(a._TroopsCount));
                        int ocIndex = 0;
                        bool progress = false;
                        bool failed = false;
                        while (remainingCountries.Count > 0)
                        {
                            List<List<CountryComponent>> tempPath2 = new List<List<CountryComponent>>();
                            List<CountryComponent> temp2 = new List<CountryComponent>();
                            temp2.Add(ownedCountries[ocIndex]);
                            tempPath2.Add(temp2);
                            //bool foundAll2 = false;
                            for (int j = 0; j < tempPath2.Count; j++)
                            {
                                List<CountryComponent> t = tempPath2[j];

                                if (t.Count == remainingCountries.Count + 1)
                                {
                                    List<CountryComponent> t1 = t;
                                    tempPath2.Clear();
                                    tempPath2.Add(t1);
                                    remainingCountries.Clear();
                                    break;
                                }

                                foreach (CountryComponent c in t[t.Count - 1]._NeighbouringCountries)
                                {
                                    if (c._Continent == BoardComponent._BoardInstance._Continents[5] && c._CurColour != finalPath[0]._CurColour && !t.Contains(c) && !finalPath.Contains(c))
                                    {
                                        List<CountryComponent> t1 = t;
                                        t1.Add(c);
                                        tempPath2.Add(t1);
                                    }
                                }
                            }

                            tempPath2.Sort((a, b) => b.Count.CompareTo(a.Count));

                            foreach (CountryComponent c in tempPath2[0])
                            {
                                finalPath.Add(c);
                                if (remainingCountries.Contains(c))
                                {
                                    progress = true;
                                    remainingCountries.Remove(c);
                                }
                            }

                            ocIndex++;

                            if (ocIndex == ownedCountries.Count && progress)
                            {
                                ocIndex = 0;
                                progress = false;
                            }
                            else if (ocIndex == ownedCountries.Count)
                            {
                                failed = true;
                                break;
                            }
                        }

                        if (failed)
                        {
                            TurnGoalStruct goa = new TurnGoalStruct();
                            goa._Goal = _Options[i]._Goal;
                            goa._Val = -500;
                            goa._Path = finalPath;

                            _Options[i] = goa;

                            continue;
                        }
                    }

                    TurnGoalStruct go = new TurnGoalStruct();
                    go._Goal = _Options[i]._Goal;
                    go._Val = 5;
                    go._Path = finalPath;

                    _Options[i] = go;
                        
                        /*else
                        {
                            TurnGoalStruct g = new TurnGoalStruct();
                            g._Goal = _Options[i]._Goal;
                            g._Val = -100;
                            g._Path = new List<CountryComponent>();

                            _Options[i] = g;
                        }*/
                }
            }


            _Options.Sort((a, b) => b._Val.CompareTo(a._Val));
        }
    }

    List<CountryComponent> DijkstrasSearch(CountryComponent start, CountryComponent end, bool dist)
    {
        List<CountryComponent> pathList = new List<CountryComponent>();

        if (start == end)
            return pathList;

        start._Gscore = 0;
        start._Previous = null;
        end._Previous = null;

        List<CountryComponent> openList = new List<CountryComponent>();
        List<CountryComponent> closedList = new List<CountryComponent>();

        CountryComponent curCountry;

        openList.Add(start);

        while(openList.Count > 0)
        {
            openList.Sort((a, b) => b._Gscore.CompareTo(a._Gscore));

            curCountry = openList[openList.Count - 1];

            if (curCountry == end)
                break;
            openList.RemoveAt(openList.Count - 1);
            closedList.Add(curCountry);

            foreach (CountryComponent c in curCountry._NeighbouringCountries)
            {
                if (c._CurColour != start._CurColour)
                {
                    bool temp = closedList.Contains(c);
                    if (!temp)
                    {
                        int gScore = curCountry._Gscore + DijkstrasCostCalc(c, dist); // + c cost()

                        if (!openList.Contains(c))
                        {
                            c._Gscore = gScore;
                            c._Previous = curCountry;
                            openList.Add(c);
                        }
                        else if (gScore < c._Gscore)
                        {
                            c._Gscore = gScore;
                            c._Previous = curCountry;
                        }
                    }
                }
            }
        }

        curCountry = end;

        if (curCountry._Previous == null)
            return pathList;

        while (curCountry._Previous != null)
        {
            pathList.Add(curCountry);
            curCountry = curCountry._Previous;
        }

        pathList.Reverse();

        pathList.Insert(0, start);

        return pathList;
    }

    int DijkstrasCostCalc(CountryComponent cur, bool dist)
    {
        int cost = 1;
        if (!dist)
        {
            cost = cur._TroopsCount;

            if (cur._HasAirfieldEffect)
                cost += 2;

            for (int j = 0; j < GameCanvasComponent._GameInstance._TurnOrder.Count; j++)
            {
                if (GameCanvasComponent._GameInstance._TurnOrder[j]._Army._ArmyName == cur._OccupyingArmy._Army._ArmyName)
                {
                    foreach (RewardScriptableObject r in GameCanvasComponent._GameInstance._TurnOrder[j]._Rewards)
                    {
                        if (r._DefenceDie)
                            cost += 2;
                    }
                    break;
                }
            }

            if (!cur._HasCity)
                cost += 1;
        }

        return cost;
    }

    BestStartStruct FindClosestCountry(CountryComponent start)
    {
        BestStartStruct countryComponent = new BestStartStruct();

        List<CountryComponent> countriesToCheck = new List<CountryComponent>();
        List<CountryComponent> countriesChecked = new List<CountryComponent>();

        //List<BestStartStruct> foundCountries

        countriesToCheck.Add(start);

        while (countriesToCheck.Count > 0)
        {
            int countryCount = countriesToCheck.Count;
            for (int i = 0; i < countryCount; i++)
            {
                foreach (CountryComponent c in countriesToCheck[i]._NeighbouringCountries)
                {
                    if (!c._BeenAdded)
                    {
                        c._BeenAdded = true;
                        countriesToCheck.Add(c);
                    } 
                }

                countriesToCheck[i]._BeenChecked = true;
            }

            for (int i = 0; i < countriesToCheck.Count; i++)
            {
                if (countriesToCheck[i]._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                {


                    foreach (CountryComponent c in countriesToCheck)
                    {
                        countriesChecked.Add(c);
                    }
                    //countriesToCheck.Clear();
                }
                else if (countriesToCheck[i]._BeenChecked)
                {
                    countriesChecked.Add(countriesToCheck[i]);
                    countriesToCheck.Remove(countriesToCheck[i]);
                    i--;
                }
            }
        }

        countriesToCheck.Clear();

        foreach (CountryComponent c in countriesChecked)
        {
            c._BeenChecked = false;
            c._BeenAdded = false;
        }

        return countryComponent;
    }
}

// Old Code saving just in case
/* List<CountryComponent> startpos = new List<CountryComponent>();
                    List<BestStartStruct> bsl = new List<BestStartStruct>();

                    // Check if any of the countries in continent are correct colour
                    foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[5]._Countries)
                    {
                        if (c._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                            startpos.Add(c);
                    }

                    if (startpos.Count < BoardComponent._BoardInstance._Continents[5]._Countries.Length)
                    {

                        // if only 1 use that

                        // if multiple use one with most troops unless the one with most troops doesn't have a neighbour to attack
                        if (startpos.Count > 0)
                        {
                            startpos.Sort((a, b) => b._TroopsCount.CompareTo(a._TroopsCount));

                            for (int j = 0; j < startpos.Count; j++)
                            {
                                bool NearbyEnemy = false;
                                foreach (CountryComponent c2 in startpos[j]._NeighbouringCountries)
                                {
                                    if (c2._CurColour != startpos[j]._CurColour && c2._Continent == BoardComponent._BoardInstance._Continents[5])
                                    {
                                        BestStartStruct s = new BestStartStruct();
                                        s._StartCountry = startpos[j];
                                        s._EndCountry = c2;
                                        s._Val = startpos[j]._TroopsCount;
                                        if (startpos[j]._IsBorderCountry)
                                            s._Val -= 0.01f;
                                        bsl.Add(s);

                                        NearbyEnemy = true;
                                        break;
                                    }
                                }

                                if (!NearbyEnemy)
                                {
                                    startpos.RemoveAt(j);
                                    j--;
                                }
                            }

                            bsl.Sort((a, b) => b._Val.CompareTo(a._Val));
                        }
                        else // if none search 
                        {

                            foreach (CountryComponent c in GameCanvasComponent._GameInstance._CurArmy._ControlledCountries) // AI should be handled by Host so this should be okay to do
                            {
                                foreach (CountryComponent b in BoardComponent._BoardInstance._Continents[5]._Countries)
                                {
                                    if (b._IsBorderCountry)
                                    {
                                        List<CountryComponent> path = DijkstrasSearch(c, b, true);

                                        if (path.Count > 0)
                                        {
                                            BestStartStruct s = new BestStartStruct();
                                            s._StartCountry = c;
                                            s._EndCountry = b;
                                            s._Dist = path.Count - 1;
                                            s._TroopCount = path[0]._TroopsCount;
                                            s._Val = ((float)s._TroopCount / 3) - s._Dist;

                                            bool foundCountry = false;
                                            for (int k = 0; k < bsl.Count; k++)
                                            {
                                                if (bsl[k]._StartCountry == s._StartCountry)
                                                {
                                                    foundCountry = true;

                                                    if (bsl[k]._Val < s._Val)
                                                        bsl[k] = s;

                                                    break;
                                                }
                                            }

                                            if (!foundCountry)
                                                bsl.Add(s);
                                        }
                                    }
                                }
                            }

                            bsl.Sort((a, b) => b._Val.CompareTo(a._Val));
                        }


                        List<CountryComponent> finalPath = DijkstrasSearch(bsl[0]._StartCountry, bsl[0]._EndCountry, false);

                        List<CountryComponent> continentCountries = new List<CountryComponent>();
                        foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[5]._Countries)
                        {
                            if (!finalPath.Contains(c) && c._CurColour != finalPath[0]._CurColour)
                            {
                                continentCountries.Add(c);
                            }
                        }

                        List<List<CountryComponent>> tempPath = new List<List<CountryComponent>>();
                        List<CountryComponent> temp = new List<CountryComponent>();
                        temp.Add(finalPath[finalPath.Count - 1]);
                        tempPath.Add(temp);
                        bool foundAll = false;
                        for (int j = 0; j < tempPath.Count; j++)
                        {
                            List<CountryComponent> t = tempPath[j];

                            if (t.Count == continentCountries.Count + 1)
                            {
                                List<CountryComponent> t1 = t;
                                t1.RemoveAt(0); // Possibly should check if end of final path is the same as 0 before removing it but 99% confident it will always be in this scenario
                                tempPath.Clear();
                                tempPath.Add(t1);
                                foundAll = true;
                                break;
                            }

                            foreach (CountryComponent c in t[t.Count - 1]._NeighbouringCountries)
                            {
                                if (c._Continent == finalPath[finalPath.Count - 1]._Continent && c._CurColour != finalPath[0]._CurColour && !t.Contains(c))
                                {
                                    List<CountryComponent> t1 = t;
                                    t1.Add(c);
                                    tempPath.Add(t1);
                                }
                            }
                        }



                        //List<CountryComponent> finalPath = DijkstrasSearch(bsl[0]._StartCountry, bsl[0]._EndCountry, false);

                        if (foundAll)
                        {
                            foreach (CountryComponent c in tempPath[0])
                            {
                                finalPath.Add(c);
                            }
                        }
                        else
                        {
                            tempPath.Sort((a, b) => b.Count.CompareTo(a.Count));

                            tempPath[0].RemoveAt(0); // Possibly should check if end of final path is the same as 0 before removing it but 99% confident it will always be in this scenario

                            foreach (CountryComponent c in tempPath[0])
                            {
                                finalPath.Add(c);
                            }

                            List<CountryComponent> remainingCountries = new List<CountryComponent>();
                            List<CountryComponent> ownedCountries = new List<CountryComponent>();
                            foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[5]._Countries)
                            {
                                if (!finalPath.Contains(c) && c._CurColour != finalPath[0]._CurColour)
                                {
                                    remainingCountries.Add(c);
                                }
                                else if (c._CurColour == finalPath[0]._CurColour)
                                {
                                    foreach (CountryComponent c2 in c._NeighbouringCountries)
                                    {
                                        if (c2._Continent == BoardComponent._BoardInstance._Continents[5] && c2._CurColour != finalPath[0]._CurColour && !finalPath.Contains(c2))
                                            ownedCountries.Add(c);
                                    }
                                }
                            }

                            ownedCountries.Sort((a, b) => b._TroopsCount.CompareTo(a._TroopsCount));
                            int ocIndex = 0;
                            bool progress = false;
                            bool failed = false;
                            while (remainingCountries.Count > 0)
                            {
                                List<List<CountryComponent>> tempPath2 = new List<List<CountryComponent>>();
                                List<CountryComponent> temp2 = new List<CountryComponent>();
                                temp2.Add(ownedCountries[ocIndex]);
                                tempPath2.Add(temp2);
                                //bool foundAll2 = false;
                                for (int j = 0; j < tempPath2.Count; j++)
                                {
                                    List<CountryComponent> t = tempPath2[j];

                                    if (t.Count == remainingCountries.Count + 1)
                                    {
                                        List<CountryComponent> t1 = t;
                                        tempPath2.Clear();
                                        tempPath2.Add(t1);
                                        remainingCountries.Clear();
                                        break;
                                    }

                                    foreach (CountryComponent c in t[t.Count - 1]._NeighbouringCountries)
                                    {
                                        if (c._Continent == BoardComponent._BoardInstance._Continents[5] && c._CurColour != finalPath[0]._CurColour && !t.Contains(c) && !finalPath.Contains(c))
                                        {
                                            List<CountryComponent> t1 = t;
                                            t1.Add(c);
                                            tempPath2.Add(t1);
                                        }
                                    }
                                }

                                tempPath2.Sort((a, b) => b.Count.CompareTo(a.Count));

                                foreach (CountryComponent c in tempPath2[0])
                                {
                                    finalPath.Add(c);
                                    if (remainingCountries.Contains(c))
                                    {
                                        progress = true;
                                        remainingCountries.Remove(c);
                                    }
                                }

                                ocIndex++;

                                if (ocIndex == ownedCountries.Count && progress)
                                {
                                    ocIndex = 0;
                                    progress = false;
                                }
                                else if (ocIndex == ownedCountries.Count)
                                {
                                    failed = true;
                                    break;
                                }
                            }

                            if (failed)
                            {
                                TurnGoalStruct goa = new TurnGoalStruct();
                                goa._Goal = _Options[i]._Goal;
                                goa._Val = -500;
                                goa._Path = finalPath;

                                _Options[i] = goa;

                                continue;
                            }
                        }

                        TurnGoalStruct go = new TurnGoalStruct();
                        go._Goal = _Options[i]._Goal;
                        go._Val = 5;
                        go._Path = finalPath;

                        _Options[i] = go;
                    }
                    else
                    {
                        TurnGoalStruct g = new TurnGoalStruct();
                        g._Goal = _Options[i]._Goal;
                        g._Val = -100;
                        g._Path = new List<CountryComponent>();

                        _Options[i] = g;
                    }
                }
            }*/