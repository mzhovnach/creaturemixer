using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QueueElement
{
    public string Name;         // prefab name    
    public int Delay;           // moves before appearing
    public string Parameters;   // specific extra parameters

    public QueueElement(string _name, int _delay, string _parameters = "")
    {
        Name = _name;
        Delay = _delay;
        Parameters = _parameters;
    }
}
