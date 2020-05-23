using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LivesPanel : MonoBehaviour
{
    const float         CHANGE_SPEED = 0.025f;
    const float         MAX_TIME = 3.0f;
    public Text         AmountText;
    public GameObject   AGameObject;
    private int         AmountCurrent;
    private int         Amount;
    private int         MaxAmount;

    public Image    Filler;

    void Awake()
    {
        AmountCurrent = 0;
        Amount = 0;
        AmountText.text = "0";
    }

    public void InitPanel(int lives, int maxLives)
    {
        MaxAmount = maxLives;
        SetAmountForce(lives);
    }

    void SetAmountForce(int amount)
    {
        LeanTween.cancel(AGameObject);
        Amount = amount;
        AmountCurrent = amount;
        AmountText.text = amount.ToString();
        Filler.fillAmount = (float)amount / MaxAmount;
    }

    void SetAmount(int amount)
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
                    (float val) =>
                    {
                        int ival = (int)val;
                        if (ival != AmountCurrent)
                        {
                            AmountCurrent = ival;
                            AmountText.text = ival.ToString();
                            Filler.fillAmount = val / MaxAmount;
                        }
                    }
                );
    }

    public bool RemoveLives(int livesToRemove)
    {
        int lives = Amount - livesToRemove;
        lives = Mathf.Max(0, lives);
        lives = Mathf.Min(lives, MaxAmount);
        SetAmount(lives);
        return IsDead();
    }

    public bool AddLives(int livesToAdd)
    {
        int lives = Amount + livesToAdd;
        lives = Mathf.Max(0, lives);
        lives = Mathf.Min(lives, MaxAmount);
        SetAmount(lives);
        return IsDead();
    }

    public bool IsDead()
    {
        return Amount <= 0;
    }
	
	public bool AttackByEnemy(Enemy enemy)
	{
		return RemoveLives(enemy.GetDamage());
	}
}