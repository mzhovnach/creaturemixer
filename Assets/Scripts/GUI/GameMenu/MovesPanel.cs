using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

public class MovesPanel : MonoBehaviour 
{
	const float CHANGE_SPEED = 0.025f;
	const float MAX_TIME = 3.0f;

	public Text AmountText;
	public GameObject AGameObject;
	private long AmountCurrent;
	private long Amount;

	void Awake()
	{
		AmountCurrent = 0;
		Amount = 0;
		AmountText.text = "0";
	}

    public void InitPanel(int moves)
    {
        SetAmountForce(moves);
    }
		
	public void OnTurnWasMade()
	{		
		long amount = Amount - 1;
        SetAmountForce(amount);
	}

	public void SetAmountForce(long amount)
	{
		LeanTween.cancel(AGameObject);
		Amount = amount;
		AmountCurrent = amount;
		AmountText.text = amount.ToString();
	}

	void SetAmount(long amount)
	{
		LeanTween.cancel(AGameObject);
		Amount = amount;

		float time = CHANGE_SPEED * Mathf.Abs(Amount - AmountCurrent);
		if (time > MAX_TIME)
		{
			time = MAX_TIME;
		}
		LeanTween.value(AGameObject, (float)AmountCurrent, (float)Amount, time)
		//.setEase(LeanTweenType.easeInOutSine)
		//	.setDelay(UIConsts.SHOW_DELAY_TIME)
			.setOnUpdate
			(
				(float val)=>
				{
					int ival = (int)val;
					if (ival != AmountCurrent)
					{
						AmountCurrent = ival;
						AmountText.text = ival.ToString();
					}
				}
			);
	}

    public long GetMovesLeft()
    {
        return Amount;
    }
}