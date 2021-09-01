using System.Collections.Generic;

public static class PartyExtention
{
    public static Party FindStrongest(this List<Party> parties)
    {
        Party strongest = null;
        var power = 0;
                
        foreach (var party in parties) {
            var strength = party.TroopStrength;
            if (strength > power) {
                strongest = party;
                power = strength;
            }
        }
                
        return strongest;
    }
}