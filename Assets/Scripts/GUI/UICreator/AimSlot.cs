using UnityEngine;
using UnityEngine.UI;
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
    public GameObject CheckMark;
    public Text LevelText; 
    private int _level = 1;
    private EAimSlotState _state = EAimSlotState.Disabled;

    public void InitSlot(Vector3Int data)
    {
        gameObject.SetActive(true);
        LevelText.gameObject.SetActive(true);
        _level = data.y;
        LevelText.text = _level.ToString();
        if (data.z == 1)
        {
            SetCompleted();
        } else
        {
            SetIncompleted();
        }
    }

    public void DisableSlot()
    {
        _state = EAimSlotState.Disabled;
        CheckMark.SetActive(false);
        //TODO можливо просто притіняти
        gameObject.SetActive(false);
        LevelText.gameObject.SetActive(false);
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
        if (IsIncompleted() && _level == alevel + 1) //бо параметр з 0, фішка з одиничкою має level 0
        {
            SetCompleted();
            return true;
        }
        return false;
    }

    private void SetCompleted()
    {
        _state = EAimSlotState.Completed;
        CheckMark.SetActive(true);
    }

    private void SetIncompleted()
    {
        _state = EAimSlotState.Incompleted;
        CheckMark.SetActive(false);
    }

}