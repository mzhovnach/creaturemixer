using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Powerup : MonoBehaviour
{
    public int                  MaxMana = 20;
    protected int				_powerupLevel = 0;  // можливо якось відображати прокачку паверапа

    public void InitPowerup(int level)
    {
		_powerupLevel = level;
        ApplyLevel();
    }

    protected virtual void ApplyLevel()
    {
        // todo get damage, get other powerups specific parameters, maybe change max mana
        //MaxAmount = GameManager.Instance.GameData.GetPowerupMaxMana(PowerupType, _powerupLevel);
    }
    
	public virtual void ApplyPowerup()
	{

	}

    public virtual void ApplyPowerup(SSlot slot)
    {

    }

    public virtual bool IsCanApply()
    {
        // перевірка чи паверап буде примінений, чи щось змінить
        return true;
    }

    public virtual bool IsCanApply(SSlot slot)
    {
        return true;
    }

    public virtual int GetMaxMana()
    {
        //TODO return max mana according to level
        return MaxMana;
    }

    public virtual EPowerupType GetPowerupType()
    {
        return EPowerupType.None;
    }

    public virtual bool IsSelectable()
    {
        return false; // якщо ні, то заюзується одразу при кліку на кнопку, а якщо так, то ще очікує додаткової дії (наприклад вибору монстра чи фішки)
    }

    public virtual int GetColor()
    {
        return -1; // color of mana, -1 is any, maybe needed 1-3 colors!!!!
    }
}