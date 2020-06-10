public class Powerup_AddLives : Powerup
{
    private int _power = 0;

    protected override void ApplyLevel()
    {
        _power = 10 + _powerupLevel * 3;
    }
    
	public override void ApplyPowerup()
	{
        if (Consts.LIVES_PANEL)
        {
            GameManager.Instance.Game.ALivesPanel.AddLives(_power);
        } else
        {
            GameManager.Instance.Game.ACharacters.AddLivesToAll(_power);
        }
        //EventData eventData = new EventData("OnPowerUpUsedEvent");
        //eventData.Data["type"] = GameData.PowerUpType.AddLives;
        //GameManager.Instance.EventManager.CallOnPowerUpUsedEvent(eventData);
    }

    public override bool IsCanApply()
    {
        if (Consts.LIVES_PANEL)
        {
            return GameManager.Instance.Game.ALivesPanel.IsWounded();
        } else
        {
            return GameManager.Instance.Game.ACharacters.IsSomebodyWounded();
        }
    }

    public override EPowerupType GetPowerupType()
    {
        return EPowerupType.AddLives;
    }

    public override int GetColor()
    {
        return 0;
    }
}