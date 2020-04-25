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
            Slots[i].gameObject.SetActive(false);
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
}