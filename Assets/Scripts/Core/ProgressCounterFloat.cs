using UnityEngine;

public class ProgressCounterFloat : MonoBehaviour
{
    public float        ChangeSpeed = 0.025f; // if <= 0 - immidiatly
    public float        MaxTime = 2.0f;
    public GameObject   AGameObject;
    protected float     AmountCurrent;
    protected float     Amount;
    protected float     MaxAmount;

    //public Transform    ViewTransform;

    void Awake()
    {
        AmountCurrent = 0;
        Amount = 0;
        UpdateView(0, 0);
    }

    public void InitCounter(int amount, int maxAmount)
    {
        MaxAmount = maxAmount;
        SetAmountForce(amount);
    }

    protected virtual void UpdateView(float amount, float norm)
    {
        //ViewTransform.SendMessage(name + "ValueChanged", this, SendMessageOptions.RequireReceiver);
        //AmountText.text = amount.ToString();
        //Filler.fillAmount = (float)amount / MaxAmount;
    }

    public float GetAmount()
    {
        return Amount;
    }

    public float GetMaxAmount()
    {
        return MaxAmount;
    }

    public void SetAmountForce(float amount)
    {
        LeanTween.cancel(AGameObject);
        Amount = amount;
        AmountCurrent = amount;
        UpdateView(amount, amount / MaxAmount);
    }

    public void SetAmount(float amount)
    {
        amount = Mathf.Max(0, amount);
        amount = Mathf.Min(amount, MaxAmount);

        if (ChangeSpeed <= 0)
        {
            // force
            Amount = amount;
            AmountCurrent = amount;
            UpdateView(amount, amount / MaxAmount);
            return;
        }

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
                            UpdateView(AmountCurrent, val / MaxAmount);
                        }
                    }
                );
    }

    public bool IsFull()
    {
        return Amount >= MaxAmount;
    }

    public bool IsEmpty()
    {
        return Amount <= 0;
    }
}
