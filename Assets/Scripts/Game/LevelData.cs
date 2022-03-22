//using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine;

[System.Serializable]
public class SSlotData
{
    public int x;
    public int y;
	public EPipeType pt;  // pipes type
	public int p;   // parameter
	public int c;   // color
	
	public SSlotData() 
	{
		x = 0;
		y = 0;
		pt = EPipeType.None;
		p = -1;
		c = 0;
	}

	public SSlotData(int xx, int yy, EPipeType ppt, int pp, int cc)
    {
        x = xx;
        y = yy;
		pt = ppt;
		p = pp;
		c = cc;
	}
}

[System.Serializable]
public class LevelData
{
    public ELevelType       Type;
    public List<long>      	Resources;
	public List<SSlotData>  Slots;
    public List<Vector3Int> Aims; // x - chip type, y - level of chip, z - completed or not(0 or 1)
    [Range(0, 2)] public int CreatureId;
    public List<int>        QueueState; // states of queue with pipes
    public List<int>        Colors;
    public bool             AddNewPipes;
    // statistic
    public float			timePlayed;
    public int              Moves = 0;
    public int              CollectAim = 0;

    public List<QueueElement> EnemiesQueue;
    //TODO  List<EnemyData> Enemies; збереження стану ворогів на полі (життя, мана, ходів до атаки,...) Доробити якщо треба зберігати гру

    public LevelData() 
    {
		Slots = new List<SSlotData>();
		Resources = new List<long>();
        QueueState = new List<int>();
        AddNewPipes = true;

        for (int i = 0; i < Consts.COLORS.Length; ++i)
		{
			Resources.Add(0);
		}
        Aims = new List<Vector3Int>();
        //for (int i = 0; i <= QueuePanel.SIZE; ++i)
        //{
        //    QueueState.Add(GameManager.Instance.BoardData.GetRandomColor());
        //}
        timePlayed = 0;
        Colors = new List<int>();
	}

    public static LevelData ConvertToLevelData(CreatureMixLevelData cmLevelData, int level)
    {
        LevelData res = new LevelData();
        res.Type = cmLevelData.Type;
        res.Moves = cmLevelData.Moves;
        res.CollectAim = cmLevelData.CollectAim;
        res.Colors = new List<int>();
        for (int i = 0; i < cmLevelData.Colors.Count; ++i)
        {
            res.Colors.Add(cmLevelData.Colors[i]);
        }
        res.Slots = new List<SSlotData>();
        for (int i = 0; i < cmLevelData.NeededStates.Count; ++i)
        {
            res.Slots.Add(cmLevelData.NeededStates[i]);
        }
        res.Aims = new List<Vector3Int>();
        res.CreatureId = cmLevelData.CreatureId;
        for (int i = 0; i < cmLevelData.Aims.Count; ++i)
        {
            res.Aims.Add(new Vector3Int(cmLevelData.Aims[i].x, cmLevelData.Aims[i].y, 0));
        }
        if (IsEmptyLevel(res.Slots))
        {
            res.AddNewPipes = true;
            UnityEngine.Random.InitState(level);
            res.Slots.Clear();
            for (int i = 0; i < GameBoard.WIDTH; ++i)
            {
                for (int j = 0; j < GameBoard.HEIGHT; ++j)
                {
                    if (UnityEngine.Random.Range(0, 100) <= 33)
                    {
                        SSlotData sData = new SSlotData();
                        sData.x = i;
                        sData.y = j;
                        sData.pt = EPipeType.Colored;
                        sData.p = 0;
                        sData.c = -1;
                        res.Slots.Add(sData);
                    }
                }
                    
            }
        }
        res.AddNewPipes = cmLevelData.AddNewPipes;
        // enemies
        res.EnemiesQueue = new List<QueueElement>();
        for (int i = 0; i < cmLevelData.Queue.Count; ++i)
        {
            res.EnemiesQueue.Add(cmLevelData.Queue[i]);
        }
        //
        return res;
    }

    public static bool IsEmptyLevel(List<SSlotData> slots)
    {
        if (slots.Count == 0)
        {
            return true;
        }
        for (int i = 0; i < slots.Count; ++i)
        {
            if (slots[i].pt == EPipeType.Colored)
            {
                return false;
            }
        }
        return true;
    }

    public static LevelData GenerateCreatureMixLevel(int level)
    {
        UnityEngine.Random.InitState(level);
        LevelData res = new LevelData();
        res.Type = ELevelType.Battle;
        res.AddNewPipes = true;
        res.Colors = new List<int>();
        res.Moves = 0;
        res.CollectAim = 0;
        // aims
        res.Aims = new List<Vector3Int>();
        res.Aims.Add(new Vector3Int(0, UnityEngine.Random.Range(2, 5), 0));
        res.Aims.Add(new Vector3Int(1, UnityEngine.Random.Range(2, 5), 0));
        res.Aims.Add(new Vector3Int(2, UnityEngine.Random.Range(2, 5), 0));
        res.Aims.Add(new Vector3Int(3, UnityEngine.Random.Range(2, 5), 0));

        res.CreatureId = UnityEngine.Random.Range(0, 3);

        // start pipes
        res.Slots = new List<SSlotData>();
        for (int i = 0; i < GameBoard.WIDTH; ++i)
        {
            for (int j = 0; j < GameBoard.HEIGHT; ++j)
            {
                if (UnityEngine.Random.Range(0, 100) <= 33)
                {
                    SSlotData sData = new SSlotData();
                    sData.x = i;
                    sData.y = j;
                    sData.pt = EPipeType.Colored;
                    sData.p = 0;
                    sData.c = -1;
                    res.Slots.Add(sData);
                }
            }
        }
        // enemies
        res.EnemiesQueue = new List<QueueElement>();
        QueueElement startElement = new QueueElement("enemy_1", 0, 0, "");
        res.EnemiesQueue.Add(startElement);
        for (int i = 0; i < 5; ++i)
        {
            QueueElement element = new QueueElement("enemy_1", i * 2, 0, "");
            res.EnemiesQueue.Add(element);
        }
        //
        return res;
    }

    public static LevelData GenerateEndlessLevel()
    {
        //UnityEngine.Random.InitState(level);
        LevelData res = new LevelData();
        res.Type = ELevelType.Endless;
        res.AddNewPipes = true;
        res.Colors = new List<int>();
        res.Moves = 0;
        res.CollectAim = 0;
        // aims
        res.Aims = new List<Vector3Int>();

        // start pipes
        res.Slots = new List<SSlotData>();
        for (int i = 0; i < GameBoard.WIDTH; ++i)
        {
            for (int j = 0; j < GameBoard.HEIGHT; ++j)
            {
                if (UnityEngine.Random.Range(0, 100) <= 33)
                {
                    SSlotData sData = new SSlotData();
                    sData.x = i;
                    sData.y = j;
                    sData.pt = EPipeType.Colored;
                    sData.p = 0;
                    sData.c = -1;
                    res.Slots.Add(sData);
                }
            }
        }
        // enemies
        res.EnemiesQueue = new List<QueueElement>();
        //
        return res;
    }

    public UISetType GetUiSetType()
    {
        switch (Type)
        {
            case ELevelType.None:
                {
                    Debug.LogError("WRONG LEVEL TYPE");
                    return UISetType.LevelBattle;
                }
            case ELevelType.Battle:
                {
                    return UISetType.LevelBattle;
                }
            case ELevelType.Collect:
                {
                    return UISetType.LevelCollect;
                }
            case ELevelType.Endless:
                {
                    return UISetType.LevelEndless;
                }
            default:
                {
                    return UISetType.LevelBattle;
                }
        }
    }
}