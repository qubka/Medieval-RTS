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
    
    protected override void Awake()
    {
        base.Awake();
        
        dateTime = new DateTime(startDate.x, startDate.y, startDate.z, 12, 0, 0);
        dateStamp = ToString();
        prevDay = startDate.z;
        
        timeLabel.text = dateStamp + StringExtention.GetPrettyName(dateTime.GetTimeOfDay());
    }
    
    private IEnumerator Start()
    {
        while (true) {
            OnUpdate();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Load(TimeSave save) {

        dateTime = save.dateTime;
        dateStamp = ToString();
        prevDay = save.prevDay;
        
        timeLabel.text = dateStamp + StringExtention.GetPrettyName(dateTime.GetTimeOfDay());
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
            dateStamp = ToString();
        }
        prevDay = day;
        
        timeLabel.text = dateStamp + StringExtention.GetPrettyName(dateTime.GetTimeOfDay());
    }
    
    public override string ToString() => dateTime.ToString("d MMMM, yyyy", CultureInfo.InvariantCulture) + Environment.NewLine;
}

[Serializable]
public class TimeSave {

    public DateTime dateTime;
    public int prevDay;

    public TimeSave(TimeController tc)
    {
        dateTime = tc.dateTime;
        prevDay = tc.prevDay;
    }
}