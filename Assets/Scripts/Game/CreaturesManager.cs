using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreaturesManager : MonoBehaviour
{
    [SerializeField] List<CreatureController> _creatures;
    int _currentCreatureId = -1;

    public void Reset()
    {
        foreach (var item in _creatures)
        {
            item.gameObject.SetActive(false);
        }
        _currentCreatureId = -1;
    }

    public void ShowCreature(int creatureId)
    {
        if (_currentCreatureId != creatureId)
        {
            Reset();
            _currentCreatureId = creatureId;
            _creatures[_currentCreatureId].gameObject.SetActive(true);
        }
    }

    public void CustomizeCreature(int bodyPart, int level)
    {
        if (_currentCreatureId != -1)
        {
            _creatures[_currentCreatureId].SetCustomization(bodyPart, level);
        }
    }
}
