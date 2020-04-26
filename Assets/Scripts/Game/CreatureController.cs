using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureController : MonoBehaviour
{
    [SerializeField] List<Transform> _hands;
    [SerializeField] List<Transform> _legs;
    [SerializeField] List<Transform> _eyes;
    [SerializeField] List<Transform> _horns;

    void SetCustomizationLevel(List<Transform> items, int level)
    {
        if (level > items.Count || items.Count == 0) return;
        //disable all
        foreach (Transform item in items)
        {
            item.gameObject.SetActive(false);
        }
        for (int i = 0; i < level; i++)
        {
            items[i].gameObject.SetActive(true);
        }
    }

    public void SetCustomization(int bodyPart, int level)
    {
        SetCustomizationLevel(GetPart(bodyPart), level);
    }

    List<Transform> GetPart(int bodyPart)
    {
        switch (bodyPart)
        {
            case 0:
                return _legs;
            case 1:
                return _eyes;
            case 2:
                return _hands;
            case 3:
                return _horns;
            default:
                return _hands;
        }
    }
}
