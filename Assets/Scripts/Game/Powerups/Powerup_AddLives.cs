public class Powerup_AddLives : Powerup
{
    private int _power = 0;

    protected override void ApplyLevel()
    {
        _power = 10 + _powerupLevel * 3;
    }
    
	public override void ApplyPowerup()
	{
        GameManager.Instance.Game.ALivesPanel.AddLives(_power);
        //EventData eventData = new EventData("OnPowerUpUsedEvent");
        //eventData.Data["type"] = GameData.PowerUpType.AddLives;
        //GameManager.Instance.EventManager.CallOnPowerUpUsedEvent(eventData);
    }

    public override bool IsCanApply()
    {
        return GameManager.Instance.Game.ALivesPanel.IsWounded();
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