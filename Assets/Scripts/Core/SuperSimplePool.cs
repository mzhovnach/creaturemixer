using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperSimplePool : MonoBehaviour
{
    public string FolderToScan = ""; // folder inside Assets/Prefabs
    private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
    public List<GameObject> Prefabs;
    public Dictionary<string, List<GameObject>> _pool = new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        for (int i = 0; i < Prefabs.Count; ++i)
        {
            _prefabs.Add(Prefabs[i].name, Prefabs[i]);
            _pool.Add(Prefabs[i].name, new List<GameObject>());
        }
    }
	
	public GameObject GetObjectFromPool(string objectName, Transform aparent)
    {
        return GetObjectFromPool(objectName, aparent, Vector3.zero);
    }

    public GameObject GetObjectFromPool(string objectName, Transform aparent, Vector3 pos)
    {
        Debug.Log(objectName + " ! ");
        List<GameObject> objects = null;
        objects = _pool[objectName];
        for (int i = 0; i < objects.Count; ++i)
        {
            if (!objects[i].activeSelf)
            {
                objects[i].SetActive(true);
                return objects[i];
            }
        }
        GameObject obj = (GameObject)GameObject.Instantiate(_prefabs[objectName], pos, Quaternion.identity, aparent);
        obj.name = objectName;
        //obj.transform.SetParent(aparent, false);
        objects.Add(obj);
        return obj;
    }
	
	public GameObject InstantiateObject(string objectName, Transform aparent)
	{
        return InstantiateObject(objectName, aparent, Vector3.zero);
	}
	
	public GameObject InstantiateObject(string objectName, Transform aparent, Vector3 pos)
	{
		GameObject obj = (GameObject)GameObject.Instantiate(_prefabs[objectName], pos, Quaternion.identity, aparent);
        obj.name = objectName;
        //obj.transform.SetParent(aparent, false);
        return obj;
	}
}
