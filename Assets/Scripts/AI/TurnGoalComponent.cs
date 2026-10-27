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
                    if (GameCanvasComponent._GameInstance._CurArmy._HasStarReward)
                    {
                        TurnGoalStruct f = new TurnGoalStruct();
                        f._Goal = _Options[i]._Goal;
                        f._Val = -500;
                        f._Path = new List<CountryComponent>();

                        _Options[i] = f;

                        continue;
                    }


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

                                //Debug.Log(c.name + " " + n.name + " " + t);

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
                else if (_Options[i]._Goal == "Take Control of Africa")
                {
                    _Options[i] = ContinentPath(_Options[i]._Goal, 3);
                }
                else if (_Options[i]._Goal == "Take Control of Asia")
                {
                    _Options[i] = ContinentPath(_Options[i]._Goal, 4);
                }
                else if (_Options[i]._Goal == "Take Control of Europe")
                {
                    _Options[i] = ContinentPath(_Options[i]._Goal, 2);
                }
                else if (_Options[i]._Goal == "Take Control of North America")
                {
                    _Options[i] = ContinentPath(_Options[i]._Goal, 0);
                }
                else if (_Options[i]._Goal == "Take Control of South America")
                {
                    _Options[i] = ContinentPath(_Options[i]._Goal, 1);
                }
                else if (_Options[i]._Goal == "Take Control of Oceania")
                {
                    _Options[i] = ContinentPath(_Options[i]._Goal, 5);
                }
                

            }


            _Options.Sort((a, b) => b._Val.CompareTo(a._Val));
        }
    }

    TurnGoalStruct ContinentPath(string goal, int continent)
    {
        if (BoardComponent._BoardInstance._Continents[continent]._ControllingArmy._Army == GameCanvasComponent._GameInstance._CurArmy._Army)
        {
            TurnGoalStruct goa = new TurnGoalStruct();
            goa._Goal = goal;
            goa._Val = -500;
            goa._Path = null;

            return goa;
        }


        List<CountryComponent> startpos = new List<CountryComponent>();
        List<BestStartStruct> bsl = new List<BestStartStruct>();

        int mostTroops = 0;
        foreach (CountryComponent b in BoardComponent._BoardInstance._Continents[continent]._Countries)
        {
            if (b._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour && b._TroopsCount > mostTroops)
                mostTroops = b._TroopsCount;
        }

        foreach (CountryComponent c in GameCanvasComponent._GameInstance._CurArmy._ControlledCountries) // AI should be handled by Host so this should be okay to do
        {
            if (c._TroopsCount >= mostTroops && c._Continent != BoardComponent._BoardInstance._Continents[continent]) // Probably need to do a troops facing check as well
            {
                foreach (CountryComponent b in BoardComponent._BoardInstance._Continents[continent]._Countries) //Finding the best path from one of owned countries to a border country within the continent
                {
                    if (b._IsBorderCountry && c._CurColour != b._CurColour)
                    {
                        List<CountryComponent> path = DijkstrasSearch(c, b, false);

                        if (path.Count > 0)
                        {
                            int opTroops = 0;
                            for (int j = 1; j < path.Count - 1; j++)
                            {
                                opTroops += path[j]._TroopsCount;
                            }

                            if (mostTroops > c._TroopsCount - opTroops && mostTroops > 0)
                                continue;

                            BestStartStruct s = new BestStartStruct();
                            s._StartCountry = c;
                            s._EndCountry = b;
                            s._Dist = path.Count - 1;
                            s._TroopCount = path[0]._TroopsCount;
                            s._Val = ((float)s._TroopCount / 2) - s._Dist;

                            s._Path = path;

                            if (c._IsBorderCountry)
                                s._Val -= 0.01f;

                            for (int j = 1; j < path.Count - 1; j++)
                            {
                                s._Val -= (float)path[j]._TroopsCount * 0.35f;
                            }

                            float temp1 = 0;
                            foreach (CountryComponent c3 in b._NeighbouringCountries)
                            {
                                if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                    temp1++;
                            }

                            if (temp1 == 1)
                                s._Val += 2;
                            else if (temp1 != 0)
                                s._Val += 1 / temp1;


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
        }

        if (bsl.Count == 0) //If no paths that means the best start is within the continent itself
        {
            CountryComponent ctemp = null;
            CountryComponent ctemp2 = null;
            foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[continent]._Countries)
            {
                if (c._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                {
                    CountryComponent ctemp3 = null;
                    foreach (CountryComponent c2 in c._NeighbouringCountries)
                    {
                        if (c2._Continent == BoardComponent._BoardInstance._Continents[continent] && c2._CurColour != c._CurColour) // check to make sure the country has a neighbour it can attack to progress through
                        {
                            if (ctemp3 == null)
                                ctemp3 = c2;
                            else
                            {
                                int temp1 = 0;
                                foreach (CountryComponent c3 in c2._NeighbouringCountries)
                                {
                                    if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                        temp1++;
                                }

                                int temp2 = 0;
                                foreach (CountryComponent c3 in ctemp3._NeighbouringCountries)
                                {
                                    if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                        temp2++;
                                }

                                if (temp1 != 0 && temp1 < temp2)
                                    ctemp3 = c2;
                            }

                            /*else if (ctemp3 != null && c2._IsBorderCountry && !ctemp3._IsBorderCountry)
                                ctemp3 = c2;
                            else if (ctemp3 != null && c2._IsBorderCountry == ctemp3._IsBorderCountry)
                            {
                                int temp1 = 0;
                                foreach (CountryComponent c3 in c2._NeighbouringCountries)
                                {
                                    if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                        temp1++;
                                }

                                int temp2 = 0;
                                foreach (CountryComponent c3 in ctemp3._NeighbouringCountries)
                                {
                                    if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                        temp2++;
                                }

                                if (temp1 != 0 && temp1 < temp2)
                                    ctemp3 = c2;
                            }*/
                            //break;
                        }
                    }

                    if (ctemp == null && ctemp3 != null/* || ctemp2 != null && ctemp3 != null && !ctemp2._IsBorderCountry && ctemp3._IsBorderCountry*/)
                    {
                        ctemp = c;
                        ctemp2 = ctemp3;
                    }
                    else if (ctemp3 != null && (float)c._TroopsCount >= (float)ctemp._TroopsCount * 1.5)
                    {
                        ctemp = c;
                        ctemp2 = ctemp3;
                    }
                    else if (ctemp3 != null)
                    {
                        int temp1 = 0;
                        foreach (CountryComponent c3 in ctemp3._NeighbouringCountries)
                        {
                            if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                temp1++;
                        }

                        int temp2 = 0;
                        foreach (CountryComponent c3 in ctemp2._NeighbouringCountries)
                        {
                            if (c3._CurColour != c._CurColour && c3._Continent == BoardComponent._BoardInstance._Continents[continent])
                                temp2++;
                        }

                        if (temp1 != 0 && temp1 < temp2 || temp1 == temp2 && c._TroopsCount > ctemp._TroopsCount)
                        {
                            ctemp = c;
                            ctemp2 = ctemp3;
                        }
                    }
                    
                    /*else if (ctemp3 != null && c._TroopsCount > ctemp._TroopsCount|| ctemp3 != null && c._TroopsCount == ctemp._TroopsCount && ctemp._IsBorderCountry)
                    {

                        if (ctemp2._IsBorderCountry && !ctemp3._IsBorderCountry)
                        {
                            continue;
                        }
                        else
                        {
                            ctemp = c;
                            ctemp2 = ctemp3;
                        }

                    }*/
                }
            }

            BestStartStruct s = new BestStartStruct();
            s._StartCountry = ctemp;
            s._EndCountry = ctemp2;
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

        List<CountryComponent> finalPath = bsl[0]._Path;

        List<CountryComponent> continentCountries = new List<CountryComponent>();
        foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[continent]._Countries) // find all remaining unowned countries in the continent that don't have a path to it
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
        for (int j = 0; j < tempPath.Count; j++) // from the country reached from the best starting point, search neighbouring countries and their neighbouring countries etc until finding all other remaining countries or until there is no more paths to check
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
                    List<CountryComponent> t1 = new List<CountryComponent>();
                    foreach(CountryComponent c2 in t)
                        t1.Add(c2);
                    t1.Add(c);
                    tempPath.Add(t1);
                }
            }
        }

        if (foundAll) // if all remaining countries are found add them to the final path
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

            foreach (CountryComponent c in tempPath[0]) // add the best progress up till now to the final path
            {
                finalPath.Add(c);
            }

            List<CountryComponent> remainingCountries = new List<CountryComponent>();
            List<CountryComponent> ownedCountries = new List<CountryComponent>();
            foreach (CountryComponent c in BoardComponent._BoardInstance._Continents[continent]._Countries) // get another list of the remaining countries now and get another list of any owned country with an attackable neighbour in the continent
            {
                if (!finalPath.Contains(c) && c._CurColour != finalPath[0]._CurColour)
                {
                    remainingCountries.Add(c);
                }
                else if (c._CurColour == finalPath[0]._CurColour)
                {
                    foreach (CountryComponent c2 in c._NeighbouringCountries)
                    {
                        if (c2._Continent == BoardComponent._BoardInstance._Continents[continent] && c2._CurColour != finalPath[0]._CurColour && !finalPath.Contains(c2))
                        {
                            ownedCountries.Add(c);
                            break;
                        }
                    }
                }
            }

            ownedCountries.Sort((a, b) => a._HasCity.CompareTo(b._HasCity));

            ownedCountries.Sort((a, b) => b._TroopsCount.CompareTo(a._TroopsCount));

            ownedCountries.Sort((a, b) => finalPath.Contains(a).CompareTo(finalPath.Contains(b))); // order the list so all the countries that have a city start at the back of the list, sort by troop count meaning the countries without cities are checked first, then order that by putting the owned countries that aren't already in the path ahead of ones that are to maxamise troop use

            int ocIndex = 0;
            bool progress = false;
            bool failed = false;
            while (remainingCountries.Count > 0) //Finding paths from the owned countries list until all remaining countries are found adding the best paths to the final path or until we loop all owned countries once without any new additions to the final path in which case we break out and label it impossible
            {
                List<List<CountryComponent>> tempPath2 = new List<List<CountryComponent>>();
                List<CountryComponent> temp2 = new List<CountryComponent>();
                temp2.Add(ownedCountries[ocIndex]);
                tempPath2.Add(temp2);

                bool newCountry = false;
                for (int j = 0; j < tempPath2.Count; j++)
                {
                    List<CountryComponent> t = tempPath2[j];

                    if (t.Count == remainingCountries.Count + 1)
                    {
                        List<CountryComponent> t1 = t;
                        tempPath2.Clear();
                        tempPath2.Add(t1);
                        remainingCountries.Clear();
                        progress = true;
                        newCountry = true;
                        break;
                    }

                    foreach (CountryComponent c in t[t.Count - 1]._NeighbouringCountries)
                    {
                        if (c._Continent == BoardComponent._BoardInstance._Continents[continent] && c._CurColour != finalPath[0]._CurColour && !t.Contains(c) && !finalPath.Contains(c))
                        {
                            List<CountryComponent> t1 = new List<CountryComponent>();
                            foreach (CountryComponent c2 in t)
                                t1.Add(c2);
                            t1.Add(c);
                            tempPath2.Add(t1);
                        }
                    }
                }

                tempPath2.Sort((a, b) => b.Count.CompareTo(a.Count));

                foreach (CountryComponent c in tempPath2[0])
                {
                    if (remainingCountries.Contains(c))
                    {
                        newCountry = true;
                        break;
                    }
                }

                if (newCountry)
                {
                    progress = true;
                    foreach (CountryComponent c in tempPath2[0])
                    {
                        finalPath.Add(c);
                        if (remainingCountries.Contains(c))
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
                goa._Goal = goal;
                goa._Val = -500;
                goa._Path = finalPath;

                return goa;
                //continue;
            }
        }

        TurnGoalStruct go = new TurnGoalStruct();
        go._Goal = goal;
        go._Path = finalPath;

        List<Vector3> Troops = new List<Vector3>();


        CountryComponent lastStart = null;
        for (int j = 0; j < go._Path.Count;)
        {
            if (go._Path[j]._CurColour == GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour) // each time the final path has to restart at an owned country calculate the amount of enemies that country has to fight on its part of the path
            {
                Vector3 tempVec = new Vector3();
                tempVec.x = go._Path[j]._TroopsCount;
                tempVec.y = 0;

                if (go._Path[j] != lastStart)
                {
                    lastStart = go._Path[j];
                }
                else
                    tempVec.y += Troops[Troops.Count - 1].y;

                j++;
                while (j < go._Path.Count && go._Path[j]._CurColour != GameCanvasComponent._GameInstance._CurArmy._Army._ArmyColour)
                {
                    if (go._Path[j]._OccupyingArmy._HasDefenceDie) // Is Occupying Army updated properly now that this is multiplayer? should check, also is taking airfields into account worth doing? probably
                        tempVec.y += go._Path[j]._TroopsCount * 1.5f;
                    else
                        tempVec.y += go._Path[j]._TroopsCount;
                    j++;
                }

                Troops.Add(tempVec);
            }
        }

        float oppTroops = 0;
        foreach (Vector3 v in Troops) // if the amount of troops in owned country is less than or equal to 5 times the enemy troops it will have to face add the amount of enemy troops it will face to that float
        {
            if (v.x <= v.y * 5) // having more than 5 times the number of troops is assumed as a "certain victory" and thus ignoring it for the following
                oppTroops += v.y;
        }

        for (int j = 0; j < Troops.Count; j++)
        {
            if (Troops[j].x <= Troops[j].y * 5)
                Troops[j] = new Vector3(Troops[j].x, Troops[j].y, (Troops[j].y / oppTroops)); // calculate the weight of troops a path section has to deal with
        }

        Troops.Sort((a, b) => b.z.CompareTo(a.z)); // sort by weight, anything thats a certain victory will have a weight of zero because its assumed to be certain to win and thus needs no assistance

        float potTroops = BoardComponent._BoardInstance._NewTroops;

        int stars = 0;
        if (GameCanvasComponent._GameInstance._CurrentState == TurnStates.CalculateTroops)
            stars = GameCanvasComponent._GameInstance._CurArmy._OneStars + (2 * GameCanvasComponent._GameInstance._CurArmy._TwoStars);

        switch (stars)
        {
            case 2:
                potTroops += 2;
                break;
            case 3:
                potTroops += 4;
                break;
            case 4:
                potTroops += 7;
                break;
            case 5:
                potTroops += 10;
                break;
            case 6:
                potTroops += 13;
                break;
            case 7:
                potTroops += 17;
                break;
            case 8:
                potTroops += 21;
                break;
            case 9:
                potTroops += 25;
                break;
            case 10:
                potTroops += 30;
                break;
            default:
                break;
        }

        float maxpotTroops = potTroops;
        for (int j = 0; j < Troops.Count; j++) // divide up the potenial troops based on the weight of enemies
        {
            float t = Mathf.CeilToInt(maxpotTroops * Troops[j].z);
            while (t > potTroops)
                t--;

            potTroops -= t;

            Troops[j] = new Vector3(Troops[j].x + t, Troops[j].y, Troops[j].z);

            if (potTroops == 0)
                break;
        }

        go._Val = 0;
        foreach (Vector3 v in Troops) // calculate final value by adding together the value of each path section
        {
            float t = 0;

            if (GameCanvasComponent._GameInstance._CurArmy._HasAttackDie)
                t = v.x - (v.y * 1.5f);
            else
                t = v.x - (v.y * 2f);

            if (t <= 0)
                t -= 8;

            go._Val += t; // x being troop count + potenial troop count of the owned country at the start of the section, y being the number of opposing troops on the path (multiplied by 2 from personaly belief of having double the troops you intent to fight at a minimum)
        }

        go._Val = go._Val / Troops.Count;

        return go;
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