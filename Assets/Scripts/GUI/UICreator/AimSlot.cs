using UnityEngine;
using System.Collections.Generic;

public class AimSlot : MonoBehaviour
{
    private enum EAimSlotState
    {
        Disabled = 0,
        Incompleted = 1,
        Completed = 2
    }

    public int AColor = 0; // hand, eye,...
    private int _level = 1;
    private EAimSlotState _state = EAimSlotState.Disabled;

    public void InitSlot(Vector3Int data)
    {
        gameObject.SetActive(true);
        _level = data.y;
        if (data.z == 1)
        {
            _state = EAimSlotState.Completed;
        } else
        {
            _state = EAimSlotState.Incompleted;
        }
    }

    public void DisableSlot()
    {
        _state = EAimSlotState.Disabled;
        //TODO можливо просто притіняти
        gameObject.SetActive(false);
    }

    public void SaveData(ref List<Vector3Int> res)
    {
        if (_state == EAimSlotState.Disabled)
        {
            return;
        }
        int completed = 0;
        if (_state == EAimSlotState.Completed)
        {
            completed = 1;
        }
        res.Add(new Vector3Int(AColor, _level, completed));
    }

    public bool IsIncompleted()
    {
        return _state == EAimSlotState.Incompleted;
    }

    public bool CheckAim(int alevel)
    {
        if (IsIncompleted() && _level == alevel)
        {
            _state = EAimSlotState.Completed;
            //TODO mark as completed visually
            return true;
        }
        return false;
    }

}