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
        GameObject obj = (GameObject)GameObject.Instantiate(_prefabs[objectName], Vector3.zero, Quaternion.identity);
        obj.name = objectName;
        obj.transform.SetParent(aparent, false);
        objects.Add(obj);
        return obj;
    }
}
