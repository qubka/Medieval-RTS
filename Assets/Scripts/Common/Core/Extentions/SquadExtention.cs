using System.Collections.Generic;
using System.Linq;

public static class SquadExtention
{
    public static void Run(this List<Squad> list)
    {
        var count = list.Count;
        if (count > 0) {
            if (count == 1) {
                var squad = list[0];
                squad.SetRunning(!squad.isRunning);
            } else {
                var selected = list;
                var status = !selected.All(s => s.isRunning);
                foreach (var squad in selected) {
                    squad.SetRunning(status);
                }
            }
        }
    }

    public static void Stop(this List<Squad> list)
    {
        var count = list.Count;
        if (count > 0) {
            if (count == 1) {
                var squad = list[0];
                squad.ForceStop();
            } else {
                var selected = list;
                foreach (var squad in selected) {
                    squad.ForceStop();
                }
            }
        }
    }

    public static void Hold(this List<Squad> list)
    {
        var count = list.Count;
        if (count > 0) {
            if (count == 1) {
                var squad = list[0];
                squad.SetHolding(!squad.isHolding);
            } else {
                var selected = list;
                var status = !selected.All(s => s.isHolding);
                foreach (var squad in selected) {
                    squad.SetHolding(status);
                }
            }
        }
    }

    public static void Range(this List<Squad> list)
    {
        var count = list.Count;
        if (count > 0) {
            if (count == 1) {
                var squad = list[0];
                squad.SetRange(!squad.isRange);
            } else {
                var selected = list.ToList();
                for (var i = selected.Count - 1; i > -1; i--) {
                    if (!selected[i].hasRange) {
                        selected.RemoveAt(i);
                    }
                }

                var status = !selected.All(s => s.isRange);
                foreach (var squad in selected) {
                    squad.SetRange(status);
                }
            }
        }
    }

    public static void Flee(this List<Squad> list)
    {
        var count = list.Count;
        if (count > 0) {
            if (count == 1) {
                var squad = list[0];
                squad.SetFlee(!squad.isFlee);
            } else {
                var selected = list;
                var status = !selected.All(s => s.isFlee);
                foreach (var squad in selected) {
                    squad.SetFlee(status);
                }
            }
        }
    }
}