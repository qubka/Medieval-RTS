using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class MoneyInfo : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        StopAllCoroutines();
        StartCoroutine(Show());
    }
    
    private IEnumerator Show()
    {
        yield return new WaitForSecondsRealtime(1f);
        
        var builder = new StringBuilder();
        var totalIncome = 0;
        var player = Game.Player;
        
        if (player.troops.Count > 0) {
            var income = player.troopWage;
            var amount = income.ToString("+#;-#;0");
            builder
                .Append(player.leader.surname)
                .Append("'s Party")
                .Append("<size=18>")
                .Append(amount.Length switch {
                    1 => "<pos=90%>",
                    2 => "<pos=85%>",
                    3 => "<pos=80%>",
                    4 => "<pos=75%>",
                    _ => "<pos=70%>"
                })
                .Append("<color=#ff0000ff>")
                .Append(amount)
                .Append("</color>")
                .Append("</size>")
                .AppendLine();

            totalIncome += income;
        }
        
        foreach (var settlement in player.leader.settlements) {
            var income = settlement.Income;
            var amount = income.ToString("+#;-#;0");
            builder
                .Append(settlement.label)
                .Append("'s Tax")
                .Append("<size=18>")
                .Append(amount.Length switch {
                    1 => "<pos=90%>",
                    2 => "<pos=85%>",
                    3 => "<pos=80%>",
                    4 => "<pos=75%>",
                    _ => "<pos=70%>"
                })
                .Append(income > 0 ? "<color=#00ff00ff>" : income < 0 ? "<color=#ff0000ff>" : "<color=#ffff00ff>")
                .Append(amount)
                .Append("</color>")
                .Append("</size>")
                .AppendLine();
            
            totalIncome += income;
        }
        
        foreach (var settlement in player.leader.settlements) {
            if (settlement.garrison.Count <= 0)
                continue; 
            
            var income = settlement.Wage;
            var amount = income.ToString("+#;-#;0");
            builder
                .Append(settlement.label)
                .Append("'s Garrison")
                .Append("<size=18>")
                .Append(amount.Length switch {
                    1 => "<pos=90%>",
                    2 => "<pos=85%>",
                    3 => "<pos=80%>",
                    4 => "<pos=75%>",
                    _ => "<pos=70%>"
                })
                .Append(income > 0 ? "<color=#00ff00ff>" : income < 0 ? "<color=#ff0000ff>" : "<color=#ffff00ff>")
                .Append(amount)
                .Append("</color>")
                .Append("</size>")
                .AppendLine();
            
            totalIncome += income;
        }
        
        var description = builder.ToString();
        
        builder.Clear();

        var total = totalIncome.ToString("+#;-#;0");
        
        builder
            .Append("<size=20>")
            .Append("<u>")
            .Append("Total Income")
            .Append("</u>")
            .Append(total.Length switch {
                1 => "<pos=90%>",
                2 => "<pos=85%>",
                3 => "<pos=80%>",
                4 => "<pos=75%>",
                _ => "<pos=70%>"
            })
            .Append(totalIncome > 0 ? "<color=#00ff00ff>" : totalIncome < 0 ? "<color=#ff0000ff>" : "<color=#ffff00ff>")
            .Append(total)
            .Append("</color>")
            .Append("</size>")
            .AppendLine();

        var caption = builder.ToString();
        
        Manager.fixedPopup.DisplayInfo(caption, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Manager.fixedPopup.HideInfo();
    }
}