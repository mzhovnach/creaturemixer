using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pools : MonoBehaviour
{
	private static GameObject _object = null;
	private static Pools _instance = null;

    // objects pool
    public List<string>                         PrefabsFolder = new List<string>(); // folder inside Assets/Prefabs
    public List<GameObject>                     Prefabs; // scanned objects
    private Dictionary<string, GameObject>      _prefabs = new Dictionary<string, GameObject>();
    public Dictionary<string, List<GameObject>> _objectsPool = new Dictionary<string, List<GameObject>>();
    // sprites pool
    public List<string>                         SpritesFolder = new List<string>(); // folder inside Assets/Prefabs
    public List<Sprite>                         Sprites; // scanned sprites
    private Dictionary<string, Sprite>          _sprites = new Dictionary<string, Sprite>();

    // Retreive or create the current music emitter
    private void Awake()
	{
		//DontDestroyOnLoad(_object); each scene has their own pool!
		_instance = this;
        InitObjectsPool();
        InitSpritesPool();
    }

    private void OnDestroy()
    {
        _instance = null;
        _objectsPool.Clear();
        _prefabs.Clear();
    }

    public void InitObjectsPool()
    {
        for (int i = 0; i < Prefabs.Count; ++i)
        {
            _prefabs.Add(Prefabs[i].name, Prefabs[i]);
            _objectsPool.Add(Prefabs[i].name, new List<GameObject>());
        }
    }

    public static GameObject CreateAccordingToConsts(string objectName, Transform aparent)
    {
        if (Consts.USE_POOL)
        {
            return GetObjectFromPool(objectName, aparent);
        } else
        {
            return InstantiateObject(objectName, aparent);
        }
    }

    public static GameObject CreateAccordingToConsts(string objectName, Transform aparent, Vector3 pos)
    {
        if (Consts.USE_POOL)
        {
            return GetObjectFromPool(objectName, aparent, pos);
        } else
        {
            return InstantiateObject(objectName, aparent, pos);
        }
    }

    public static GameObject GetObjectFromPool(string objectName, Transform aparent)
    {
        return GetObjectFromPool(objectName, aparent, Vector3.zero);
    }

    public static GameObject GetObjectFromPool(string objectName, Transform aparent, Vector3 pos)
    {
        //Debug.Log(objectName + " ! ");
        List<GameObject> objects = null;
        objects = _instance._objectsPool[objectName];
        for (int i = 0; i < objects.Count; ++i)
        {
            if (!objects[i].activeSelf)
            {
                objects[i].SetActive(true);
                return objects[i];
            }
        }
        GameObject obj = (GameObject)GameObject.Instantiate(_instance._prefabs[objectName], pos, Quaternion.identity, aparent);
        obj.name = objectName;
        //obj.transform.SetParent(aparent, false);
        objects.Add(obj);
        return obj;
    }

    public static GameObject InstantiateObject(string objectName, Transform aparent)
    {
        return InstantiateObject(objectName, aparent, Vector3.zero);
    }

    public static GameObject InstantiateObject(string objectName, Transform aparent, Vector3 pos)
    {   
        GameObject obj = (GameObject)GameObject.Instantiate(_instance._prefabs[objectName], pos, Quaternion.identity, aparent);
        obj.name = objectName;
        //obj.transform.SetParent(aparent, false);
        return obj;
    }

    public static GameObject InstantiateObject(string objectName, Transform aparent, Vector3 pos, float angleZ)
    {
        Quaternion rot = new Quaternion();
        rot.eulerAngles = new Vector3(0, 0, angleZ);
        GameObject obj = (GameObject)GameObject.Instantiate(_instance._prefabs[objectName], pos, rot, aparent);
        obj.name = objectName;
        //obj.transform.SetParent(aparent, false);
        return obj;
    }


    public void InitSpritesPool()
    {
        for (int i = 0; i < Sprites.Count; ++i)
        {
            _sprites.Add(Sprites[i].name, Sprites[i]);
        }
    }

    public static Sprite GetSprite(string spriteName)
    {
        return _instance._sprites[spriteName];
    }

    public static Sprite GetSpriteSafe(string id)
    {
        Sprite res = null;
        if (_instance._sprites.TryGetValue(id, out res))
        {
            return res;
        }
        string path = "art\\" + id;
        res = Resources.Load<Sprite>(path);
        _instance._sprites.Add(id, res);
        return res;
    }

    public static GameObject GetPrefab(string id)
    {
        return _instance._prefabs[id];
    }
}
