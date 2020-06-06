using System.Collections.Generic;
using UnityEngine;
using System;

public class PowerupsPanel : MonoBehaviour
{
    public GameObject   	    AGameObject;
	public Transform   		    Container;
    public GameObject   	    CollectManaEffect; //TODO colorize effect or different prefabs
    private ZActionWorker 	    _worker;
	private List<PowerupButton>	_buttons = new List<PowerupButton>();
    public List<Transform>      Positions;

    private PowerupButton 	    _selectedButton = null;

    void Awake()
    {
        _worker = new ZActionWorker();
    }

    public void InitPanel(List<PowerupData> powerupsData)
    {
		ClearPowerups();
		for (int i = 0; i < powerupsData.Count; ++i)
		{
			AddPowerupButton(powerupsData[i]);
		}
    }
	
	public void ClearPowerups()
	{
		_selectedButton = null;
		for (int i = 0; i < _buttons.Count; ++i)
		{
            _buttons[i].gameObject.SetActive(false);
		}
        _buttons.Clear();

    }
	
	private void AddPowerupButton(PowerupData powerupData)
	{
		GameObject obj = GameManager.Instance.Game.GetPool().InstantiateObject("Powerup_" + powerupData.Type.ToString(), Container);
		PowerupButton powerupButton = obj.GetComponent<PowerupButton>();
        _buttons.Add(powerupButton);
		obj.transform.localPosition = Positions[_buttons.Count - 1].localPosition;
		powerupButton.InitPowerup(this, powerupData);
	}

    public int AddManaForBump(SSlot slot, SPipe pipe, int mana, int color)
    {
		for (int i = 0; i < _buttons.Count; ++i)
		{
			int addedMana = _buttons[i].AddMana(mana, color);
			if (addedMana > 0)
			{
				mana -= addedMana;
				Vector3 startPos = transform.parent.transform.InverseTransformPoint(slot.transform.position);
				Vector3 endPos = transform.parent.transform.parent.InverseTransformPoint(_buttons[i].transform.position);
				GameObject effect = GameObject.Instantiate(CollectManaEffect, Vector3.zero, Quaternion.identity) as GameObject;
				effect.transform.SetParent(transform.parent.transform, false);
				effect.transform.localPosition = startPos;
				List<Vector3> path = GameManager.Instance.GameData.XMLSplineData[String.Format("chip_get_{0}", UnityEngine.Random.Range(1, 4))];
				MoveSplineAction splineMover = new MoveSplineAction(effect, path, startPos, endPos, Consts.ADD_POINTS_EFFECT_TIME);
				_worker.AddParalelAction(splineMover);
				GameObject.Destroy(effect, Consts.ADD_MANA_EFFECT_TIME + 0.1f);
			}
			if (mana <= 0)
			{
				break;
			}
		}
        return mana;
    }

    void Update()
    {
        _worker.UpdateActions(Time.deltaTime);
    }
	
	public void UseButtonOnClick(PowerupButton powerupButton)
    {
        if (GameManager.Instance.Game.GetGameState() == EGameState.PlayersTurn)
        {
			if (_selectedButton)
			{
				if (_selectedButton == powerupButton)
				{
                    powerupButton.Unselect();
                    _selectedButton = null;
                    return;
				} else
				{
                    _selectedButton.Unselect();
                    _selectedButton = null;
                }
			}
			
			if (!powerupButton.IsSelectable())
			{
                powerupButton.TryApplyPowerup();
			} else
			{
                powerupButton.Select();
                _selectedButton = powerupButton;
			}
        }
    }
	
	public EPowerupType GetSelectedPowerupType()
	{
		if (_selectedButton)
		{
			return _selectedButton.GetPowerupType();
		} else
		{
			return EPowerupType.None;
		}
	}

    public bool IsCanApply()
    {
        for (int i = 0; i < _buttons.Count; ++i)
        {
            if (_buttons[i].IsFull() && _buttons[i].IsCanApply())
            {
                return true;
            }
        }
        return false;
    }

    public bool OnSlotTouched(SSlot slot)
    {
        if (GameManager.Instance.Game.GetGameState() != EGameState.PlayersTurn)
        {
            return false;
        }
        if (_selectedButton)
        {
            bool applied = _selectedButton.TryApplyPowerup(slot);
            if (applied)
            {
                _selectedButton.Unselect();
                _selectedButton = null;
            }
            return true; // can't swipe if powerup selected
        } else
        {
            return false;
        }
    }
}