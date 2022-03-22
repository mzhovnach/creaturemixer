using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ScorePanel : ProgressCounter
{
    public GameObject       CollectEffect;
    public Text             AmountText;
    private ZActionWorker   _worker;

    [SerializeField]
    Text                    _bestText;

    void Awake()
    {
        _worker = new ZActionWorker();
        UpdateBestText();
    }

    public void InitPanel()
    {
        InitCounter(0, int.MaxValue);
        UpdateBestText();
    }

    public void Collect(SSlot slot, int pipeColor, int toCollect)
    {
        if (IsFull())
        {
            return;
        }
        SetAmount(GetAmount() + toCollect);
        Vector3 startPos = transform.parent.transform.InverseTransformPoint(slot.transform.position);
        Vector3 endPos = transform.parent.transform.parent.InverseTransformPoint(transform.position);
        GameObject effect = GameObject.Instantiate(CollectEffect, Vector3.zero, Quaternion.identity) as GameObject;
        effect.transform.SetParent(transform.parent.transform, false);
        effect.transform.localPosition = startPos;
        List<Vector3> path = GameManager.Instance.GameData.XMLSplineData[String.Format("chip_get_{0}", UnityEngine.Random.Range(1, 4))];
        MoveSplineAction splineMover = new MoveSplineAction(effect, path, startPos, endPos, Consts.ADD_POINTS_EFFECT_TIME);
        _worker.AddParalelAction(splineMover);
        GameObject.Destroy(effect, Consts.ADD_MANA_EFFECT_TIME + 0.1f);
    }

    void Update()
    {
        _worker.UpdateActions(Time.deltaTime);
    }

    protected override void UpdateView(int amount, float norm)
    {
        AmountText.text = amount.ToString();
    }

    public void UpdateBestText()
    {
        _bestText.text = "Best : " + GameManager.Instance.Player.BestScore.ToString();
    }
}