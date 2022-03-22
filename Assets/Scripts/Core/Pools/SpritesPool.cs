using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpritesPool : MonoBehaviour
{
    public List<string> FoldersToScan = new List<string>(); // folder inside Assets/Prefabs
    private Dictionary<string, Sprite> _sprites = new Dictionary<string, Sprite>();
    public List<Sprite> Sprites;

    public void InitPool()
    {
        for (int i = 0; i < Sprites.Count; ++i)
        {
            _sprites.Add(Sprites[i].name, Sprites[i]);
        }
    }
	
	public Sprite GetSprite(string spriteName)
    {
        return _sprites[spriteName];
    }

    public Sprite GetSpriteSafe(string id)
    {
        Sprite res = null;
        if (_sprites.TryGetValue(id, out res))
        {
            return res;
        }
        string path = "art\\" + id;
        res = Resources.Load<Sprite>(path);
        _sprites.Add(id, res);
        return res;
    }
}
