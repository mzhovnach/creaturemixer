using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class AimPanel : MonoBehaviour 
{
	public GameObject AGameObject;
    public List<AimSlot> Slots;
    public GameObject FlyEffectPrefab;
    [SerializeField] CreaturesManager _creaturesManager;

    void Awake()
	{

    }
	
	void OnDestroy()
	{

    }

    public void InitPanel(LevelData levelData)
    {
        _creaturesManager.Reset();
        for (int i = 0; i < Slots.Count; ++i)
        {
            Slots[i].DisableSlot();
        }

        _creaturesManager.ShowCreature(levelData.CreatureId);
        for (int i = 0; i < levelData.Aims.Count; ++i)
        {
            Slots[levelData.Aims[i].x].InitSlot(levelData.Aims[i]);
            if (levelData.Aims[i].z == 0)
            {
                _creaturesManager.CustomizeCreature(levelData.Aims[i].x, 0);
            }
            else
            {
                _creaturesManager.CustomizeCreature(levelData.Aims[i].x, levelData.Aims[i].y);
            }
        }
    }

    public List<Vector3Int> GetDataToSave()
    {
        List<Vector3Int> res = new List<Vector3Int>();
        for (int i = 0; i < Slots.Count; ++i)
        {
            if (Slots[i].gameObject.activeSelf)
            {
                Slots[i].SaveData(ref res);
            }
        }
        return res;
    }

    public bool CheckSlot(SSlot slot)
    {
        if (!slot || !slot.Pipe || slot.Pipe.PipeType != EPipeType.Colored)
        {
            return false;
        }
        int pipeColor = slot.Pipe.AColor;
        if (pipeColor < Slots.Count)
        {
            if (Slots[pipeColor].CheckAim(slot.Pipe.Param))
            {
                _creaturesManager.CustomizeCreature(pipeColor, Slots[pipeColor].GetLevel());
                SPipe pipe = slot.TakePipe();
                FlyingEffect(pipe);
                return true;
            }
        }
        return false;
    }

    public bool IsAllAimsCompleted()
    {
        for (int i = 0; i < Slots.Count; ++i)
        {
            if (Slots[i].IsIncompleted())
            {
                return false;
            }
        }
        return true;
    }

    protected void FlyingEffect(SPipe pipe)
    {
        Time.timeScale = 0.1f;
        int pipeColor = pipe.AColor;
        //pipe.PlayHideAnimation();
        Vector3 fromPos = pipe.transform.position;
        Vector3 toPos = Slots[pipeColor].transform.position;

        Vector3[] pathPoints = new Vector3[5];
        // from
        Vector3 p1 = fromPos;
        p1.z = -2;
        pathPoints[1] = p1;
        pipe.transform.position = p1;
        // to
        Vector3 p3 = toPos;
        p3.z = -2;
        pathPoints[3] = p3;
        // first liverage
        Vector3 p0 = p1;
        p0.x += UnityEngine.Random.Range(-4.0f, 4.0f);
        p0.y += UnityEngine.Random.Range(-2.0f, -4.0f);
        pathPoints[0] = p0;
        // second liverage
        Vector3 p4 = p3;
        p4.x += UnityEngine.Random.Range(-4.0f, 4.0f);
        p4.y += UnityEngine.Random.Range(2.0f, 4.0f);
        pathPoints[4] = p4;
        // middle point
        Vector3 p2 = (p1 + p3) / 2.0f;
        p2.x += UnityEngine.Random.Range(-3.0f, 3.0f);
        p2.z = -5;
        pathPoints[2] = p2;
        //
        LTSpline spline = new LTSpline(pathPoints);
        //
        //GameObject trailEffect = GameObject.Instantiate(FlyEffectPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        //trailEffect.transform.SetParent(pipe.transform, false);
        //trailEffect.transform.localPosition = Vector3.zero;
        ////trailEffect.transform.position = fromPos;
        ////trailEffect.transform.localScale = new Vector3(0, 0, 1);
        ////LeanTween.scale(trailEffect, Vector3.one, Consts.ADD_POINTS_EFFECT_TIME / 2.0f)
        ////    .setEase(LeanTweenType.easeInOutSine)
        ////    .setLoopPingPong(1)
        LeanTween.value(pipe.gameObject, 0.0f, 1.0f, Consts.ADD_POINTS_EFFECT_TIME)
            .setOnUpdate((float norm)=>
            {
                spline.place(pipe.transform, norm);
            })
            .setOnComplete(() =>
            {
                //Destroy(trailEffect, Consts.ADD_POINTS_EFFECT_TIME + 0.2f);
                pipe.transform.localScale = Vector3.one;
                pipe.PlayHideAnimation();
            });
    }
}