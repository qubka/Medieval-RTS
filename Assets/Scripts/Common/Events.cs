using System.Collections.Generic;
using UnityEngine.Events;

public sealed class Events : SingletonObject<Events>
{
    private readonly UnityEvent _hourlyTickEvent = new UnityEvent();
    private readonly UnityEvent _dailyTickEvent = new UnityEvent();
    private readonly UnityEvent _weeklyTickEvent = new UnityEvent();
    /*private readonly UnityEvent<Hero> _messengerSent = new UnityEvent<Hero>();
    private readonly UnityEvent<Kingdom> _peaceProposalSent = new UnityEvent<Kingdom>();
    private readonly UnityEvent<Town> _fiefGranted = new UnityEvent<Town>();
    private readonly UnityEvent<Settlement> _playerSettlementTaken = new UnityEvent<Settlement>();
    private readonly UnityEvent<WarDeclaredEvent> _warDeclared = new UnityEvent<WarDeclaredEvent>();
    private readonly UnityEvent<Kingdom> _kingdomBannerChanged = new UnityEvent<Kingdom>();*/
    //private readonly UnityEvent<AllianceEvent> _allianceFormed = new UnityEvent<AllianceEvent>();
    //private readonly UnityEvent<AllianceEvent> _allianceBroken = new UnityEvent<AllianceEvent>();
    //private readonly UnityEvent<WarExhaustionEvent> _warExhaustionAdded = new UnityEvent<WarExhaustionEvent>();

    private List<object> _listeners;
    
    private void Start()
    {
        _listeners = new List<object> {
            _hourlyTickEvent,
            _dailyTickEvent,
            _weeklyTickEvent,
            /*_allianceBroken,
            _allianceFormed,
            _fiefGranted,
            _messengerSent,
            _peaceProposalSent,
            _playerSettlementTaken,
            _warDeclared,
            _kingdomBannerChanged,
            _warExhaustionAdded*/
        };
    }

    public static UnityEvent HourlyTickEvent => Instance._hourlyTickEvent;
    public static UnityEvent DailyTickEvent => Instance._dailyTickEvent;
    public static UnityEvent WeeklyTickEvent => Instance._weeklyTickEvent;
    //public static UnityEvent<AllianceEvent> AllianceFormed => Instance._allianceFormed;
    //public static UnityEvent<AllianceEvent> AllianceBroken => Instance._allianceBroken;
    /* public static UnityEvent<Hero> MessengerSent => Instance._messengerSent;
     public static UnityEvent<Kingdom> PeaceProposalSent => Instance._peaceProposalSent;
     public static UnityEvent<Town> FiefGranted => Instance._fiefGranted;
     public static UnityEvent<Settlement> PlayerSettlementTaken => Instance._playerSettlementTaken;
     public static UnityEvent<WarDeclaredEvent> WarDeclared => Instance._warDeclared;
     public static UnityEvent<Kingdom> KingdomBannerChanged => Instance._kingdomBannerChanged;
     public static UnityEvent<WarExhaustionEvent> WarExhaustionAdded => Instance._warExhaustionAdded;
     
     public void OnMessengerSent(Hero hero) => Instance._messengerSent.Invoke(hero);
     public void OnPeaceProposalSent(Kingdom kingdom) => Instance._peaceProposalSent.Invoke(kingdom);
     public void OnFiefGranted(Town fief) => Instance._fiefGranted.Invoke(fief);
     public void OnPlayerSettlementTaken(Settlement currentSettlement) => Instance._playerSettlementTaken.Invoke(currentSettlement);
     public void OnWarDeclared(WarDeclaredEvent warDeclaredEvent) => Instance._warDeclared.Invoke(warDeclaredEvent);
     public void OnKingdomBannerChanged(Kingdom kingdom) => Instance._kingdomBannerChanged.Invoke(kingdom);*/
    //public void OnAllianceFormed(AllianceEvent allianceEvent) => Instance._allianceFormed.Invoke(allianceEvent);
    //public void OnAllianceBroken(AllianceEvent allianceEvent) => Instance._allianceBroken.Invoke(allianceEvent);
    //public void OnWarExhaustionAdded(WarExhaustionEvent warExhaustionEvent) => Instance._warExhaustionAdded.Invoke(warExhaustionEvent);
    
    /*WarDeclared
   MakePeace
   ClanChangedKingdom
   OnSettlementOwnerChangedEvent*/
    
    public void OnHourlyTickEvent() => Instance._hourlyTickEvent.Invoke();
    public void OnDailyTickEvent() => Instance._dailyTickEvent.Invoke();
    public void OnWeeklyTickEvent() => Instance._weeklyTickEvent.Invoke();
    
    public static void RemoveListeners(object o) => Instance.RemoveListenersInternal(o);

    private void RemoveListenersInternal(object obj)
    {
        //foreach (dynamic listener in _listeners)
         //   listener.ClearListeners(obj);
    }
}
