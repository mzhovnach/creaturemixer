using System.Collections.Generic;
using UnityEngine;

public class Powerup_Reshaffle : Powerup
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
    
	public override void ApplyPowerup()
	{
        GameBoard board = GameManager.Instance.Game;
        board.SetGameState(EGameState.PlayerUsedPowerup, "PowerUp_Reshuffle");
        List<SSlot> slots = new List<SSlot>();
        for (int i = 0; i < GameBoard.WIDTH; ++i)
        {
            for (int j = 0; j < GameBoard.HEIGHT; ++j)
            {
                SSlot slot = board.Slots[i, j];
                SPipe pipe = slot.Pipe;
                if (pipe != null)
                {
                    slots.Add(slot);
                }
            }
        }
        if (slots.Count > 0)
        {
            //int boosterPower = GameManager.Instance.Player.BoosterLevel * Consts.PU__POWER_PER_LEVEL_RESHUFFLE;
            //boosterPower = Mathf.Min(slots.Count, boosterPower);
            int boosterPower = slots.Count;
            // take pipes from slots
            slots = Helpers.ShuffleList(slots);
            List<SPipe> pipes = new List<SPipe>();
            for (int i = 0; i < boosterPower; ++i)
            {
                pipes.Add(slots[i].TakePipe());
            }
            // find free slots
            List<SSlot> freeSlots = board.GetEmptySSlots();
            // randomly move pipes to slots
            float maxTime = 0;
            for (int i = 0; i < boosterPower; ++i)
            {
                // add to new slot
                SPipe pipe = pipes[i];
                int randI = UnityEngine.Random.Range(0, freeSlots.Count);
                SSlot slot = freeSlots[randI];

                if (slot.X == pipe.X && slot.Y == pipe.Y && freeSlots.Count > 1)
                {
                    // we must change slot
                    --i;
                    continue;
                }

                freeSlots.RemoveAt(randI);
                // find distance
                float dx = pipe.X - slot.X;
                float dy = pipe.Y - slot.Y;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                // find move time, check max time
                float moveTime = distance * Consts.PU__RESHUFFLE_TIME_PER_SLOT;
                if (moveTime > maxTime)
                {
                    maxTime = moveTime;
                }
                //
                slot.SetPipe(pipe, false);
                // move upper
                Vector3 oldPos = pipe.transform.position;
                oldPos.z = GameBoard.PipeZ - 1.0f;
                pipe.transform.position = oldPos;
                // fly to new slot
                GameObject pipeObj = pipe.gameObject;
                LeanTween.cancel(pipeObj);
                Vector3 newPos = slot.transform.position;
                newPos.z = GameBoard.PipeZ;
                LeanTween.move(pipeObj, newPos, moveTime)
                    //.setDelay(i * 0.01f)
                    .setEase(LeanTweenType.easeInOutSine);
            }
            MusicManager.playSound("reshuffle");
            pipes.Clear();
            freeSlots.Clear();
            //
            EventData eventData = new EventData("OnPowerUpUsedEvent");
            eventData.Data["type"] = GameData.PowerUpType.Reshuffle;
            GameManager.Instance.EventManager.CallOnPowerUpUsedEvent(eventData);
            //
            LeanTween.delayedCall(maxTime, () =>
            {
                if (!board.CheckIfOutOfMoves())
                {
                    board.SetGameState(EGameState.PlayersTurn, "Reshuffle powerup completed");
                }
            });
            //Invoke("CheckIfOutOfMoves", maxTime);
        }
        else
        {
            //TODO wrong sound, nothing happens
            board.SetGameState(EGameState.PlayersTurn, "Reshuffle wrong");
        }
	}
		
    protected override bool IsCanApply()
    {
        GameBoard board = GameManager.Instance.Game;
        List<SSlot> slots = new List<SSlot>();
        for (int i = 0; i < GameBoard.WIDTH; ++i)
        {
            for (int j = 0; j < GameBoard.HEIGHT; ++j)
            {
                SSlot slot = board.Slots[i, j];
                SPipe pipe = slot.Pipe;
                if (pipe != null)
                {
                    slots.Add(slot);
                }
            }
        }
        return slots.Count > 0;
    }

    public override EPowerupType GetPowerupType()
    {
        return EPowerupType.Reshaffle;
    }

    public override int GetColor()
    {
        return 1;
    }
	
	public override int GetMaxMana()
    {
        return _maxMana;
    }
}