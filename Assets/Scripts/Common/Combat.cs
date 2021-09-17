using System.Linq;
using UnityEngine.SceneManagement;

public static class Combat
{
    private static bool enabled;
    
    public static Party[] attackers;
    public static Party[] defenders;

    public static void Init()
    {
        if (!enabled) {
            enabled = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Combat") {
            var isPlayerAttacker = attackers.Any(p => p.leader.IsPlayer);
            attackers.SpawnTroops(isPlayerAttacker ? Team.Self : Team.Enemy);
            defenders.SpawnTroops(isPlayerAttacker ? Team.Enemy : Team.Self);
        }
    }

    private static void SpawnTroops(this Party[] party, Team team)
    {
        var isEnemy = team == Team.Enemy;
        var position = isEnemy ? Manager.global.team2Position : Manager.global.team1Position;
        var rotation = isEnemy ? Manager.global.team2Rotation : Manager.global.team1Rotation;

        var troops = party.SelectMany(p => p.troops).ToList();
        var length = party.Sum(p => p.TroopLength) / 1.5f; // TODO: Be careful here

        var left = position;
        var right = position;
        left.x -= length;
        right.x += length;
            
        var segments = Vector.SplitLineToSegments(left, right, troops.Count);
        for (var i = 0; i < segments.Count; i++) {
            var (start, end) = segments[i];
            var center = (end + start) / 2f; // center between end and pos
            
            Squad.Create(troops[i], team, center, rotation);
        }
    }
}