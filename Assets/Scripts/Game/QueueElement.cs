using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QueueElement
{
    public string Name;         // prefab name    
    public int  Delay;          // moves before appearing, 0 - try add just one, -1 - continue adding
    public int Slot;            // preffered slot from 1 to 5, <=0 for auto adding (enemy occupies this slot and near slots to the right depending on enemy size)
    public string Parameters;   // specific extra parameters

    public QueueElement(string _name, int _delay, int slot, string _parameters = "")
    {
        Name = _name;
        Delay = _delay;
        Slot = slot;
        Parameters = _parameters;
    }
}
