using UnityEngine;
using UnityEngine.UI;

public class ProgressView : MonoBehaviour
{
    public float ChangeSpeed = 0.025f;
    public float MaxTime = 2.0f;
    public Text AmountText;
    public GameObject AGameObject;
    private int AmountCurrent;
    private int Amount;
    private int MaxAmount;

    public Image Filler;

    void Awake()
    {
        AmountCurrent = 0;
        Amount = 0;
        if (AmountText)
        {
            AmountText.text = "0";
        }
    }

    public void InitPanel(int amount, int maxAmount)
    {
        MaxAmount = maxAmount;
        SetAmountForce(amount);
    }

    public void SetAmountForce(int amount)
    {
        LeanTween.cancel(AGameObject);
        Amount = amount;
        AmountCurrent = amount;
        AmountText.text = amount.ToString();
        Filler.fillAmount = (float)amount / MaxAmount;
    }

    public void SetAmount(int amount)
    {
        LeanTween.cancel(AGameObject);
        Amount = amount;

        float time = ChangeSpeed * Mathf.Abs(Amount - AmountCurrent);
        if (time > MaxTime)
        {
            time = MaxTime;
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
}
