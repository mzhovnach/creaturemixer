using System.Collections.Generic;
using UnityEngine;

public class Powerup_DestroyPiece : Powerup
{
    private int _maxMana = 0;

    protected override void ApplyLevel()
    {
        _maxMana = MaxMana;
        _maxMana -= _powerupLevel * 1;
        if (_maxMana < 1)
        {
            _maxMana = 1;
        }
    }
    
	public override void ApplyPowerup(SSlot slot)
	{
        GameBoard board = GameManager.Instance.Game;
        board.SetGameState(EGameState.PlayerUsedPowerup, "PowerUp_DestroyPiece");
        board.BreakePipeInSlot(slot, (slot.Pipe as Pipe_Colored).GetExplodeEffectPrefab());
        //EventData eventData = new EventData("OnPowerUpUsedEvent");
        //eventData.Data["type"] = GameData.PowerUpType.Breake;
        //GameManager.Instance.EventManager.CallOnPowerUpUsedEvent(eventData);
        // if no pipes left - add new pipe on board without move counting
        if (board.GetMovablePipesCount() == 0)
        {
            board.OnTurnWasMade(false, true);
        } else
        {
            board.SetGameState(EGameState.PlayersTurn, "DestroyPiece powerup completed");
        }
    }

    public override bool IsCanApply(SSlot slot)
    {
        if (!slot || !slot.Pipe)
        {
            return false;
        }
        EPipeType pipeType = slot.Pipe.PipeType;
        if (pipeType == EPipeType.Colored) // || pipeType == EPipeType.Blocker) //TODO upgrade to breake blockers
        {
            return true;
        }
        return false;
    }

    public override EPowerupType GetPowerupType()
    {
        return EPowerupType.DestroyPiece;
    }

    public override int GetColor()
    {
        return 2;
    }
	
	public override int GetMaxMana()
    {
        return _maxMana;
    }

    public override bool IsSelectable()
    {
        return true;
    }
}