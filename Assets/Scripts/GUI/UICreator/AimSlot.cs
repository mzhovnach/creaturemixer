using UnityEngine;
using System.Collections.Generic;
using System;

public class AimSlot : MonoBehaviour
{
    public int AColor = 0; // hand, eye,...
    private int _level = 1;
    private int _completed = 0; // 0 or 1

    public void InitSlot(Vector3Int data)
    {
        gameObject.SetActive(true);
        _level = data.y;
        _completed = data.z;
    }

    public void SaveData(ref List<Vector3Int> res)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        res.Add(new Vector3Int(AColor, _level, _completed));
    }

    public bool IsCompleted()
    {
        return _completed == 1;
    }

}