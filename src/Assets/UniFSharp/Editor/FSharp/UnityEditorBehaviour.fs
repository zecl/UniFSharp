namespace UniFSharp
open System
open UnityEngine
open UnityEditor

[<InitializeOnLoad>]
type UnityEditorBehaviour private() =
  inherit MonoBehaviour () 

  static let mutable isStarting = true
  static let mutable nextProgressTime = 0. 
  static let mutable nextEventTime = 0.
  static let mutable nextBirthdayTime = 0.
  static let mutable hourFlg = true
  static let mutable isBirthday = false

  [<Literal>]
  static let OneDay = 86425.
  static let ( +++ ) a b = EditorApplication.update <- Delegate.Combine(a, b :> Delegate) :?> EditorApplication.CallbackFunction

  static do
    if (isStarting && EditorApplication.timeSinceStartup < 8. && FSharpBuildToolsWindow.UnityChanOption.startingVoice) then
      FSharpBuildToolsWindow.Initialize()
      UnityChan.playStartingVoice(StartingVoice.Hajimeru)
      isStarting <- false

    // Event
    nextEventTime <- EditorApplication.timeSinceStartup + 15.
    let eventAction = new EditorApplication.CallbackFunction (fun _ ->
      if (FSharpBuildToolsWindow.UnityChanOption.eventVoice) then
        if (nextEventTime < EditorApplication.timeSinceStartup) then
          nextEventTime <- EditorApplication.timeSinceStartup + OneDay
          if (UnityChan.playDayEvent(DateTime.Now)) then
              NotificationWindow.ShowNotificationUnityChan()
              nextBirthdayTime <- EditorApplication.timeSinceStartup + 25.
          isBirthday <- true)
    EditorApplication.update +++ eventAction

    // Birthday
    nextBirthdayTime <- EditorApplication.timeSinceStartup + 25.
    let birthdayAction = new EditorApplication.CallbackFunction (fun _ ->
      if (isBirthday && FSharpBuildToolsWindow.UnityChanOption.eventVoice) then
        if (nextBirthdayTime < EditorApplication.timeSinceStartup) then
          nextBirthdayTime <- EditorApplication.timeSinceStartup + OneDay
          UnityChan.playBirthday DateTime.Now FSharpBuildToolsWindow.UnityChanOption.birthday)
    EditorApplication.update +++ birthdayAction

    // Progress
    UnityEditorBehaviour.SetNextProgressTime(FSharpBuildToolsWindow.UnityChanOption.progressInterval)
    let progressAction = new EditorApplication.CallbackFunction (fun _ ->
      if (FSharpBuildToolsWindow.UnityChanOption.progressVoice) then
        UnityEditorBehaviour.CheckProgress())
    EditorApplication.update +++ progressAction

    // TimeSignal
    let timeSignalAction = new EditorApplication.CallbackFunction (fun _ ->
      if (FSharpBuildToolsWindow.UnityChanOption.timeSignalVoice) then
        UnityEditorBehaviour.CheckHour())
    EditorApplication.update +++ timeSignalAction

  static member SetNextProgressTime(interval:float) = nextProgressTime <- EditorApplication.timeSinceStartup + interval * 60.

  static member SetNextBirthdayTime () =
    isBirthday <- true
    nextBirthdayTime <- EditorApplication.timeSinceStartup + 5.

  static member CheckProgress () = 
    if (nextProgressTime < EditorApplication.timeSinceStartup) then
      UnityEditorBehaviour.SetNextProgressTime(FSharpBuildToolsWindow.UnityChanOption.progressInterval)
      NotificationWindow.ShowNotificationUnityChan()
      UnityChan.playProgressVoice ()

  static member CheckHour () =
    if (hourFlg && DateTime.Now.ToString("HH:mm") = UnityEditorBehaviour.GetNextHourString()) then
      hourFlg <- false
      NotificationWindow.ShowNotificationUnityChan()
      UnityChan.playTimeSignalVoice(DateTime.Now.Hour)
    else if (DateTime.Now.ToString("HH:mm") <> UnityEditorBehaviour.GetNextHourString()) then
      hourFlg <- true

  static member GetNextHourString () = 
    String.Format("{0}:00", DateTime.Now.ToString("HH"))