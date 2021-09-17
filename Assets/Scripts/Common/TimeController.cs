using System;
using System.Collections;
using System.Globalization;
using BehaviorDesigner.Runtime;
using TMPro;
using UnityEngine;

public class TimeController : SingletonObject<TimeController>
{
    [SerializeField] private TimeBar timeBar;
    [SerializeField] private TMP_Text timeLabel;
    [SerializeField] private Vector3Int startDate = new Vector3Int(1080, 1, 1);
    [SerializeField] private bool paused;
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
        
        Label();
    }
    
    private void Start()
    {
        if (!paused) {
            StartCoroutine(Tick());
        }
    }

    private IEnumerator Tick()
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
        
        Label();
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

        Label();
    }
    
    private string DateStamp() => dateTime.ToString("d MMMM, yyyy", CultureInfo.InvariantCulture);
    private string DayTime() => Environment.NewLine + StringExtention.GetPrettyName(dateTime.GetTimeOfDay());

    public void Lock()
    {
        timeBar.Stop();
        timeBar.isLocked = true;
    }
    
    public void Unlock()
    {
        timeBar.isLocked = false;
        timeBar.Normal();
    }

    private void Label()
    {
        if (timeLabel) {
            timeLabel.text = dateStamp + DayTime();
        }
    }
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