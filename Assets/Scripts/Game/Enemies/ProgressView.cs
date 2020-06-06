using UnityEngine;
using UnityEngine.UI;

public class ProgressView : ProgressCounter
{
    public Text AmountText;
    public Image Filler;

    protected override void UpdateView(int amount, float norm)
    {
        AmountText.text = amount.ToString();
        Filler.fillAmount = (float)amount / MaxAmount;
    }
}
