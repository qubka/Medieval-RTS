using TMPro;

public class ResourceBar : BarBehavior
{
    public TMP_Text gold;
    public TMP_Text influence;

    public override void OnUpdate()
    {
        gold.text = Game.Player.leader.money.ToString();
    }
}