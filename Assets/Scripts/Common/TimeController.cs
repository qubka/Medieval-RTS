using System;
using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;

public class TimeController : SingletonObject<TimeController>
{
    [SerializeField] private TextMeshProUGUI timeLabel;
    [SerializeField] private Vector3Int startDate = new Vector3Int(1080, 1, 1);
    [HideInInspector] public DateTime dateTime;
    [HideInInspector] public string dateStamp;
    [HideInInspector] public int prevDay;
    
    public static DateTime Now => Instance.dateTime;
    public static string Date => Instance.dateStamp;
    
    protected override void Awake()
    {
        base.Awake();
        
        dateTime = new DateTime(startDate.x, startDate.y, startDate.z, 12, 0, 0);
        dateStamp = DateStamp();
        prevDay = startDate.z;
        
        timeLabel.text = dateStamp + DayTime();
    }
    
    private IEnumerator Start()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Load(TimeSave save) {

        dateTime = new DateTime(save.ticks);
        dateStamp = DateStamp();
        prevDay = save.prevDay;
        
        timeLabel.text = dateStamp + DayTime();
    }

    private void OnUpdate()
    {
        dateTime = dateTime.AddMinutes(1);
        
        var day = dateTime.Day;
        if (day != prevDay) {
            var events = Events.Instance;
            events.OnDailyTickEvent();
            if (dateTime.DayOfWeek == DayOfWeek.Monday) {
                events.OnWeeklyTickEvent();
            }
            dateStamp = DateStamp();
        }
        prevDay = day;
        
        timeLabel.text = dateStamp + DayTime();
    }
    
    private string DateStamp() => dateTime.ToString("d MMMM, yyyy", CultureInfo.InvariantCulture);
    private string DayTime() => Environment.NewLine + StringExtention.GetPrettyName(dateTime.GetTimeOfDay());
}

[Serializable]
public class TimeSave {

    public long ticks;
    public int prevDay;

    public TimeSave(TimeController tc)
    {
        ticks = tc.dateTime.Ticks;
        prevDay = tc.prevDay;
    }
}