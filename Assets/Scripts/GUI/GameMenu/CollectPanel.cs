using System.Collections.Generic;
using UnityEngine;
using System;

public class CollectPanel : ProgressView
{
    public GameObject       CollectEffect;
    private ZActionWorker   _worker;

    void Awake()
    {
        _worker = new ZActionWorker();
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
}