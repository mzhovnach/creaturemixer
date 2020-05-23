using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PowerupsPanel : MonoBehaviour
{
    const float         CHANGE_SPEED = 0.025f;
    const float         MAX_TIME = 3.0f;
    public Text         AmountText;
    public GameObject   AGameObject;
    public GameObject   CollectManaEffect;
    private int         AmountCurrent;
    private int         Amount;
    private int         MaxAmount;

    public UIButton     UseButton;
    private ZActionWorker _worker;

    public Image    Filler;

    void Awake()
    {
        _worker = new ZActionWorker();
        AmountCurrent = 0;
        Amount = 0;
        AmountText.text = "0";
    }

    public void InitPanel(int mana, int maxMana)
    {
        MaxAmount = maxMana;
        SetAmountForce(mana);
        UpdateUseButton();
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
        UpdateUseButton();

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

    public bool RemoveMana(int manaToRemove)
    {
        int mana = Amount - manaToRemove;
        mana = Mathf.Max(0, mana);
        mana = Mathf.Min(mana, MaxAmount);
        SetAmount(mana);
        return IsFull();
    }

    public bool AddMana(int manaToAdd)
    {
        int mana = Amount + manaToAdd;
        mana = Mathf.Max(0, mana);
        mana = Mathf.Min(mana, MaxAmount);
        SetAmount(mana);
        return IsFull();
    }

    public bool IsFull()
    {
        return Amount >= MaxAmount;
    }

    public void AddManaForBump(SSlot slot, int distance)
    {
        AddMana(distance); // more distance - more mana
        Vector3 startPos = transform.parent.transform.InverseTransformPoint(slot.transform.position);
        Vector3 endPos = transform.parent.transform.InverseTransformPoint(transform.position);
        GameObject effect = GameObject.Instantiate(CollectManaEffect, Vector3.zero, Quaternion.identity) as GameObject;
        effect.transform.SetParent(transform.parent.transform, false);
        effect.transform.localPosition = startPos;
        List<Vector3> path = GameManager.Instance.GameData.XMLSplineData[String.Format("chip_get_{0}", UnityEngine.Random.Range(1, 4))];

        MoveSplineAction splineMover = new MoveSplineAction(effect, path, startPos, endPos, Consts.ADD_POINTS_EFFECT_TIME);
        _worker.AddParalelAction(splineMover);
        GameObject.Destroy(effect, Consts.ADD_MANA_EFFECT_TIME + 0.1f);
    }

    public void UseButtonOnClick(UnityEngine.Object obj)
    {
        if (GameManager.Instance.Game.GetGameState() == EGameState.PlayersTurn)
        {
            SetAmount(0);
            GameManager.Instance.Game.PowerUp_Reshuffle();
            GameManager.Instance.Game.ALivesPanel.AddLives(10);
        }
    }

    public void UpdateUseButton()
    {
        UseButton.gameObject.SetActive(Amount >= MaxAmount);
    }

    void Update()
    {
        _worker.UpdateActions(Time.deltaTime);
    }
}