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
    public List<long>      	Resources;
	public List<SSlotData>  Slots;
    public List<Vector3Int> Aims; // x - chip type, y - level of chip, z - completed or not(0 or 1)
    [Range(0, 2)] public int CreatureId;
    public int              ReshufflePowerups;
    public int              BreakePowerups;
    public int              ChainPowerups;
    public int              DestroyColorsPowerups;
    public int              SwapPowerups;
    public bool             AddsViewed; // for getting Chain Powerup
    public List<int>        QueueState; // states of queue with pipes
    public List<int>        Colors;
    public bool             AddNewPipes;
    // statistic
    public float			timePlayed;

    public List<QueueElement> EnemiesQueue;
    //TODO  List<EnemyData> Enemies; збереження стану ворогів на полі (життя, мана, ходів до атаки,...) Доробити якщо треба зберігати гру

    public LevelData() 
    {
		Slots = new List<SSlotData>();
		Resources = new List<long>();
        QueueState = new List<int>();
        ReshufflePowerups = 0;
        BreakePowerups = 0;
        ChainPowerups = 0;
        DestroyColorsPowerups = 0;
        SwapPowerups = 0;
        AddsViewed = false;
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
        res.ReshufflePowerups = Consts.POWERUPS_RESHUFFLE_AT_START;
        res.BreakePowerups = Consts.POWERUPS_BREAKE_AT_START;
        res.ChainPowerups = Consts.POWERUPS_CHAIN_AT_START;
        res.DestroyColorsPowerups = Consts.POWERUPS_DESTROY_COLOR_AT_START;
        res.AddsViewed = true;
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
        res.AddNewPipes = true;
        res.ReshufflePowerups = Consts.POWERUPS_RESHUFFLE_AT_START;
        res.BreakePowerups = Consts.POWERUPS_BREAKE_AT_START;
        res.ChainPowerups = Consts.POWERUPS_CHAIN_AT_START;
        res.DestroyColorsPowerups = Consts.POWERUPS_DESTROY_COLOR_AT_START;
        res.AddsViewed = true;
        res.Colors = new List<int>();
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
}