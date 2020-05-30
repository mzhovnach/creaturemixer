//using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;

public enum EPowerupType
{
    None = -1,
    AddLives = 0,
    Reshaffle = 1,
    DestroyPiece = 2,
    EnemyAttack = 3
}

[System.Serializable]
public class PowerupData
{
    public EPowerupType     Type;
    public int              Level;

    public PowerupData() 
    {
		Type = EPowerupType.None;
        Level = 0;
	}
	
	public PowerupData(EPowerupType atype, int alevel) 
    {
		Type = atype;
        Level = alevel;
	}
}