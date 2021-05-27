using TMPro;

public class ResourceBar : BarBehavior
{
    public TextMeshProUGUI gold;
    public TextMeshProUGUI influence;

    public override void OnUpdate()
    {
        gold.text = Game.Player.leader.money.ToString();
    }
}