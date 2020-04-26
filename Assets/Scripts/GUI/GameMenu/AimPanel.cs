using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class AimPanel : MonoBehaviour 
{
	public GameObject AGameObject;
    public List<AimSlot> Slots;
    public GameObject FlyEffectPrefab;

    void Awake()
	{

    }
	
	void OnDestroy()
	{

    }

    public void InitPanel(LevelData levelData)
    {
        for (int i = 0; i < Slots.Count; ++i)
        {
            Slots[i].DisableSlot();
        }
        for (int i = 0; i < levelData.Aims.Count; ++i)
        {
            Slots[levelData.Aims[i].x].InitSlot(levelData.Aims[i]);
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
                SPipe pipe = slot.TakePipe();
                StartCoroutine(FlyingEffectCoroutine(pipe));
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

    protected IEnumerator FlyingEffectCoroutine(SPipe pipe)
    {
        int pipeColor = pipe.AColor;
        pipe.PlayHideAnimation();
        Vector3 fromPos = pipe.transform.position;
        fromPos.z = 0;
        Vector3 toPos = Slots[pipeColor].transform.position;
        toPos.z = 0;
        int amount = 3;
        for (long i = 0; i < amount; ++i)
        {
            Vector3[] pathPoints = new Vector3[5];
            // from
            Vector3 p1 = fromPos;
            pathPoints[1] = p1;
            // to
            Vector3 p3 = toPos;
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
            pathPoints[2] = p2;
            //
            LTSpline spline = new LTSpline(pathPoints);
            //
            GameObject trailEffect = GameObject.Instantiate(FlyEffectPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            trailEffect.transform.SetParent(transform.parent.transform, false);
            trailEffect.transform.position = fromPos;
            trailEffect.transform.localScale = new Vector3(0, 0, 1);
            LeanTween.scale(trailEffect, Vector3.one, Consts.ADD_POINTS_EFFECT_TIME / 2.0f)
                .setEase(LeanTweenType.easeInOutSine)
                .setLoopPingPong()
                .setLoopCount(2);
            LeanTween.value(trailEffect, 0.0f, 1.0f, Consts.ADD_POINTS_EFFECT_TIME)
                .setOnUpdate((float norm)=>
                {
                    spline.place(trailEffect.transform, norm);
                })
                .setOnComplete(() =>
                {
                    Destroy(trailEffect, Consts.ADD_POINTS_EFFECT_TIME + 0.2f);
                });
            yield return new WaitForSeconds(0.15f);
        }

    }
}