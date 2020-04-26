using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class AimPanel : MonoBehaviour 
{
	public GameObject AGameObject;
    public List<AimSlot> Slots;

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
                //Vector3[] pathPoints = new Vector3[4];
                //Vector3 p0 = slot.Pipe.transform.position;
                //p0.z = 0;
                //pathPoints[0] = p0;
                //Vector3 p1 = Slots[pipeColor].transform.position;
                //p0.z = 0;
                //pathPoints[0] = p0;
                //LTSpline spline = new LTSpline(pathPoints);
                SPipe pipe = slot.TakePipe();
                pipe.PlayHideAnimation();
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
}