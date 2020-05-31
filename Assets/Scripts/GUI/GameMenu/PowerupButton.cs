using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//[RequireComponent(typeof(Powerup))]
public class PowerupButton : MonoBehaviour
{
    const float         		CHANGE_SPEED = 0.025f;
    const float         		MAX_TIME = 3.0f;
    public Text         		AmountText;
    public GameObject   		AGameObject;
    public GameObject           SelectionObject;

    public Image                BackImage;
    public Image    			Filler;
	public UIButton     		UseButton;
	public Image 				Icon;
	//public List<Sprite>			LevelsSprites;

    protected Powerup           _powerup;
    protected int         		_amountCurrent;
    protected int         		_amount;
    protected int         		_maxAmount;
	protected bool 				_selected = false;
	protected PowerupsPanel 	_panel = null;

    void Awake()
    {
        _powerup = GetComponent<Powerup>();
        _amountCurrent = 0;
        _amount = 0;
        AmountText.text = "0";

        Color acolor = Consts.GET_COLOR_BY_ID(_powerup.GetColor());
        Filler.color = acolor;
    }

    public void InitPowerup(PowerupsPanel panel, PowerupData data)
    {
		_panel = panel;
        _powerup.InitPowerup(data.Level);
        _maxAmount = _powerup.GetMaxMana();
        SetAmountForce(0);
		UpdatePowerupView(data.Level);
        UpdateUseButton();
		UnselectForce();
    }

    void SetAmountForce(int amount)
    {
        LeanTween.cancel(AGameObject);
        _amount = amount;
        _amountCurrent = amount;
        AmountText.text = amount.ToString();
        Filler.fillAmount = (float)amount / _maxAmount;
    }

    void SetAmount(int amount)
    {
        LeanTween.cancel(AGameObject);
        _amount = amount;
        UpdateUseButton();

        float time = CHANGE_SPEED * Mathf.Abs(_amount - _amountCurrent);
        if (time > MAX_TIME)
        {
            time = MAX_TIME;
        }
        LeanTween.value(AGameObject, (float)_amountCurrent, (float)_amount, time)
            //.setEase(LeanTweenType.easeInOutSine)
            //	.setDelay(UIConsts.SHOW_DELAY_TIME)
            .setOnUpdate((float val) =>
                {
                    int ival = (int)val;
                    if (ival != _amountCurrent)
                    {
                        _amountCurrent = ival;
                        AmountText.text = ival.ToString();
                        Filler.fillAmount = val / _maxAmount;
                    }
                })
            .setOnComplete(()=>
                {
                    Filler.fillAmount = ((float)_amount) / _maxAmount;
                });
    }

    public bool RemoveMana(int manaToRemove)
    {
        int mana = _amount - manaToRemove;
        mana = Mathf.Max(0, mana);
        mana = Mathf.Min(mana, _maxAmount);
        SetAmount(mana);
        return IsFull();
    }

    public bool AddMana(int manaToAdd)
    {
        int mana = _amount + manaToAdd;
        mana = Mathf.Max(0, mana);
        mana = Mathf.Min(mana, _maxAmount);
        SetAmount(mana);
        return IsFull();
    }
	
	public int AddMana(int manaToAdd, int color)
    {
		if (IsFull())
		{
			return 0;
		}
		if (color < 0 || color == _powerup.GetColor())
		{
			int manaToAddReal = Mathf.Min(_maxAmount - _amount, manaToAdd);
			int mana = _amount + manaToAddReal;
			mana = Mathf.Max(0, mana);
			mana = Mathf.Min(mana, _maxAmount);
			SetAmount(mana);
			return manaToAdd - manaToAddReal;
		}
        return 0;
    }

    public bool IsFull()
    {
        return _amount >= _maxAmount;
    }
	
	public int GetNeededAmount()
	{
		return _maxAmount - _amount;
	}

    public void UpdateUseButton()
    {
        UseButton.gameObject.SetActive(_amount >= _maxAmount);
    }
	
	private void UpdatePowerupView(int level)
	{
		//Icon.sprite = LevelsSprites[level];
	}
	
	public bool IsSelected()
	{
		return _selected;
	}
	
	public void Select()
	{
		_selected = true;
		SelectionObject.SetActive(true);
	}
	
	public void UnselectForce()
	{
		_selected = false;
		SelectionObject.SetActive(false);
	}
	
	public void Unselect()
	{
		_selected = false;
		SelectionObject.SetActive(false);
	}
	
	public bool TryApplyPowerup()
	{
        if (!IsCanApply())
        {
            return false;
        }
        SetAmount(0);
		_powerup.ApplyPowerup();
        return true;
	}

    public bool TryApplyPowerup(SSlot slot)
    {
        if (!IsCanApply(slot))
        {
            return false;
        }
        SetAmount(0);
        _powerup.ApplyPowerup(slot);
        return true;
    }

    public bool IsCanApply()
    {
        return _powerup.IsCanApply();
    }

    public bool IsCanApply(SSlot slot)
    {
        return _powerup.IsCanApply(slot);
    }

    public bool IsSelectable()
    {
        return _powerup.IsSelectable();
    }

    public EPowerupType GetPowerupType()
    {
        return _powerup.GetPowerupType();
    }

    public void UseButtonOnClick(UnityEngine.Object obj)
    {
        _panel.UseButtonOnClick(this);
    }
}