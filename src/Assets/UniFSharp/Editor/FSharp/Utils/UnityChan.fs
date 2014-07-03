namespace UniFSharp
open System
open System.Collections.Generic
open UnityEngine
open UnityEditor
open AliasNameAttribute

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module UnityChan =
  let voicePath = (FSharpBuildTools.projectRootPath + @"UnityChan\Voice\") |> replaceDirSepFromAltSep
  let rand = new System.Random()
  let toMap source = source |> Seq.map (|KeyValue|) |> Map.ofSeq

  let voices<'TEnum, 'U when 'TEnum : enum<'U>> = 
    let getAudioClip name = Resources.LoadAssetAtPath<AudioClip>(voicePath + name)
    Enum.GetValues(typeof<'TEnum>) |> Seq.cast<'TEnum> |> Seq.map (fun x -> new KeyValuePair<_,_>(x,  x |> toAliasName |> getAudioClip))

  let buildVoices = voices<BuildVoice, int> |> toMap
  let cheerVoices = voices<CheerVoice, int> |> toMap
  let progressVoices = voices<ProgressVoice, int> |> toMap
  let licenseVoices = voices<LicenseVoice, int> |> toMap
  let startingVoices = voices<StartingVoice, int> |> toMap
  let choiceVoices = voices<ChoiceVoice, int> |> toMap
  let timeSignalVoices = voices<TimeSignalVoice, int> |> toMap
  let eventVoices = voices<EventVoice, int> |> toMap

  let playStartingVoice voice =
    startingVoices.[voice] |> AudioUtil.playClip 

  let playBuildVoice voice =
    buildVoices.[voice] |> AudioUtil.playClip 

  let playProgressVoice () =
    let index = rand.Next(0, voices<ProgressVoice, int> |> Seq.length)
    let voice = enum<ProgressVoice> index
    progressVoices.[voice] |> AudioUtil.playClip

  let playLicenseVoice voice =
    licenseVoices.[voice] |> AudioUtil.playClip 

  let playCheerVoice () =
    let index = rand.Next(0, voices<CheerVoice, int> |> Seq.length)
    let voice = enum<CheerVoice> index
    cheerVoices.[voice] |> AudioUtil.playClip 

  let playChoiceVoice voice =
    choiceVoices.[voice] |> AudioUtil.playClip 

  let playTimeSignalVoice hour =
    let voice = enum<TimeSignalVoice> hour
    timeSignalVoices.[voice] |> AudioUtil.playClip

  let playDayEvent targetDay =
    let dayInfo = targetDay |> DayInfoUtil.getDayInfo
    let voiceFiles = voices<EventVoice,int>
    let isEvent key = 
      let todayName = Enum.GetName(typeof<EventVoice>, key)
      dayInfo.name = todayName
    voiceFiles |> Seq.tryFind (fun x -> isEvent x.Key) |> function
    | Some x -> 
      eventVoices.[x.Key] |> AudioUtil.playClip 
      true
    | None -> false

  let playBirthday (targetDay:DateTime) (birthday:string) =
    let d = ref System.DateTime.MinValue
    if (DateTime.TryParse(birthday, d)) then
      if ((!d).ToString("MMdd") = targetDay.ToString("MMdd")) then 
        eventVoices.[EventVoice.誕生日] |> AudioUtil.playClip