using UnityEngine;

public class ProgressCounter : MonoBehaviour
{
    public float ChangeSpeed = 0.025f;
    public float MaxTime = 2.0f;
    public GameObject AGameObject;
    protected int AmountCurrent;
    protected int Amount;
    protected int MaxAmount;

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

    protected virtual void UpdateView(int amount, float norm)
    {
        //ViewTransform.SendMessage(name + "ValueChanged", this, SendMessageOptions.RequireReceiver);
        //AmountText.text = amount.ToString();
        //Filler.fillAmount = (float)amount / MaxAmount;
    }

    public int GetAmount()
    {
        return Amount;
    }

    public int GetMaxAmount()
    {
        return MaxAmount;
    }

    public void SetAmountForce(int amount)
    {
        amount = Mathf.Max(0, amount);
        amount = Mathf.Min(amount, MaxAmount);
        LeanTween.cancel(AGameObject);
        Amount = amount;
        AmountCurrent = amount;
        UpdateView(amount, (float)amount / MaxAmount);
    }

    public void SetAmount(int amount, float delay = 0)
    {
        amount = Mathf.Max(0, amount);
        amount = Mathf.Min(amount, MaxAmount);

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
            .setDelay(delay)
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

    public bool RemoveAmount(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("RemoveAmount needs positive value");
        }
        int newAmount = Amount - amount;
        newAmount = Mathf.Min(newAmount, MaxAmount);
        SetAmount(newAmount);
        return Amount == 0;
    }

    public bool AddAmount(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("AddAmount needs positive value");
        }
        int newAmount = Amount + amount;
        newAmount = Mathf.Min(newAmount, MaxAmount);
        SetAmount(newAmount);
        return Amount == MaxAmount;
    }

    public bool RemoveAmountForce(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("RemoveAmount needs positive value");
        }
        int newAmount = Amount - amount;
        newAmount = Mathf.Min(newAmount, MaxAmount);
        SetAmountForce(newAmount);
        return Amount == 0;
    }

    public bool AddAmountForce(int amount)
    {
        if (amount < 0)
        {
            Debug.LogError("AddAmount needs positive value");
        }
        int newAmount = Amount + amount;
        newAmount = Mathf.Min(newAmount, MaxAmount);
        SetAmountForce(newAmount);
        return Amount == MaxAmount;
    }
}
