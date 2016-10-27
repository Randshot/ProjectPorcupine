using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Random = UnityEngine.Random;

public class Goals
{
    public static string FillHunger = "FillHunger";
    public static string FillMind = "FillMind";
    public static string FillOther = "FillOther";
}

public class Brain : MonoBehaviour,IBrain
{
    public int Hunger = 100;
    public int Mind = 100;
    public void Init()
    {
        _goalsWeight.Add(Goals.FillHunger, 0);
        _goalsWeight.Add(Goals.FillMind, 0);
        _goalsWeight.Add(Goals.FillOther, 0);
    }

    private float _costTime = 0;
	public void Tick(IGoap goap, float deltaTime)
    {
        _costTime += deltaTime;
        if (_costTime >= 1)
        {
            Hunger -= 2;
            Mind -= 2;
            _costTime = 0;
        }
    }

    public void Release()
    {

    }

    readonly Dictionary<string, int> _goalsWeight = new Dictionary<string, int>();

    readonly Dictionary<string, bool> _sortedTags = new Dictionary<string, bool>();
    public Dictionary<string, bool> NextGoal()
    {
        _goalsWeight[Goals.FillHunger] = GetHungerWeight();
        _goalsWeight[Goals.FillMind] = GetMindWeight();
        _goalsWeight[Goals.FillOther] = GetOtherWeight();

        var items = from pair in _goalsWeight
                    orderby pair.Value descending
                    select pair;

        _sortedTags.Clear();
        foreach (KeyValuePair<string, int> pair in items)
        {
            _sortedTags.Add(pair.Key,true); 
        }
        return _sortedTags;
    }

    private int GetOtherWeight()
    {
        return Normal;
    }

    private int GetMindWeight()
    {
        if (Mind < 30)
            return Height;
        else if (Mind < 60)
            return Normal;
        else
            return Low;
    }

    private int Low = 1;
    private int Normal = 2;
    private int Height = 3;
    private int GetHungerWeight()
    {
        if (Hunger < 30)
            return Height;
        else if (Hunger < 60)
            return Normal;
        else
            return Low;
    }
}
