namespace UniFSharp
open System

type DayType = 
  /// 平日
  | Weekday
  /// 休日 
  | Holiday
  /// 振休
  | SubstituteHoliday
  /// 特別な日
  | Specialday
  /// 祝日
  | Syukujitsu

type DayInfo = { day : DateTime; dayType: DayType; name : string }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module DayInfoUtil =
    let getEventName key = Enum.GetName(typeof<EventVoice>, key)

    /// 祝日法施行日
    let startSyukujitsu = new DateTime(1948, 7, 20)
    /// 振替休日制度の開始日
    let startSubstituteHoliday = new DateTime(1973, 07, 12)

    let private january (dayInfo:DayInfo) = 
      if (dayInfo.day.Month = 1 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        let week = dayInfo.day.DayOfWeek

        match day with
        | 1 ->
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``元日`` |> getEventName }
        | _ ->
          if (year >= 2000) then
            if ((((day - 1) / 7) |> int = 1) && (week = DayOfWeek.Monday)) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``成人の日`` |> getEventName }
            else
              dayInfo
          else
            if (day = 15) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``成人の日`` |> getEventName }
            else
              dayInfo

    let private february (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 2 |> not) then dayInfo
      else
      let year = dayInfo.day.Year
      let day = dayInfo.day.Day
       
      match year,day with
      | _,3 -> 
        // TODO : 節分はいまのところ毎年2月3日だが、これは1985年から2024年ごろまでに限ったことであり、常にそうではない。
        { dayInfo with dayType = DayType.Specialday; name = EventVoice.``節分`` |> getEventName }
      | _,11 -> 
          if (year >= 1967) then
            { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``建国記念の日`` |> getEventName }
          else
            dayInfo
      | _,14 -> 
        { dayInfo with dayType = DayType.Specialday; name = EventVoice.``バレンタインデー`` |> getEventName }
      | 1989,24 -> 
        { dayInfo with dayType = DayType.Syukujitsu; name = "昭和天皇の大喪の礼"}
      | _,_ -> dayInfo

    let private march (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 3 |> not) then dayInfo
      else
      let day = dayInfo.day.Day

      match day with
      | 3 -> 
        { dayInfo with dayType = DayType.Specialday; name = EventVoice.``ひな祭り`` |> getEventName }
      | 14 ->
        { dayInfo with dayType = DayType.Specialday; name = EventVoice.``ホワイトデー`` |> getEventName }
      |  _ ->
        let isSyunbun (dayInfo:DayInfo) = 
          if (dayInfo.day.Month = 3 |> not) then false
          else
            let year = dayInfo.day.Year
            let day = dayInfo.day.Day
            if (year <= 1947) then false
            else if (year <= 1979) then
              day = int (20.8357 + (0.242194 * float(year - 1980)) - (float(year - 1983) / 4.))
            else if (year <= 2099) then
              day = int (20.8431 + (0.242194 * float(year - 1980)) - (float(year - 1980) / 4.))
            else if (year <= 2150) then
              day = int (21.851 + (0.242194 * float(year - 1980)) - (float(year - 1980) / 4.))
            else false

        if (dayInfo |> isSyunbun) then
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``春分の日`` |> getEventName }
        else
          dayInfo

    let private april (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 4 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day

        match day with
        | 1 -> 
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``トーコちゃんの誕生日`` |> getEventName }
        | 21 -> 
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``ミサキチの誕生日`` |> getEventName }
        | 22 -> 
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``茅野茉莉恵の誕生日`` |> getEventName }
        | 29 -> 
          if (year >= 2007) then
            { dayInfo with dayType = DayType.Syukujitsu; name = "昭和の日" }
          else if (year >= 1989) then
            { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``みどりの日`` |> getEventName }
          else
            { dayInfo with dayType = DayType.Syukujitsu; name = "天皇誕生日"}
        | _ ->
          if ((year = 1959) && (day = 10)) then
            { dayInfo with dayType = DayType.Syukujitsu; name = "皇太子明仁親王の結婚の儀"}
          else
            dayInfo

    let private may (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 5 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        let week = dayInfo.day.DayOfWeek

        match day with
        | 3 ->
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``憲法記念日`` |> getEventName }
        | 4 ->
          if (year >= 2007) then
            { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``みどりの日`` |> getEventName }
          else if (year >= 1986) then
            // 5/4 が日曜日は只の日曜､5/4が月曜日は憲法記念日の振替休日(1986～2006)
            if (week > DayOfWeek.Monday) then
              { dayInfo with dayType = DayType.Holiday; name = "国民の休日"}
            else
              dayInfo
          else
            dayInfo
        | 5 ->
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``こどもの日`` |> getEventName }
        | 6 ->
          // [5/3,5/4が日曜]ケースのみ、ここで判定
          if ((year >= 2007) && ((week = DayOfWeek.Tuesday) || (week = DayOfWeek.Wednesday))) then
            { dayInfo with dayType = DayType.SubstituteHoliday; name = "振替休日"}
          else
            dayInfo
        | _ -> 
          dayInfo

    let private june (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 6 |> not)  then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        match day with
        | 2 -> 
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``大鳥こはくのママの誕生日`` |> getEventName }
        | _ ->
          if ((year = 1993) && (day = 9)) then
            { dayInfo with dayType = DayType.Syukujitsu; name = "皇太子徳仁親王の結婚の儀"}
          else
            dayInfo

    let private july (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 7 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        let week = dayInfo.day.DayOfWeek

        match day with
        | 7 ->
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``七夕`` |> getEventName }
        | _ ->
          if (year >= 2003) then
            if ((int((day - 1) / 7) = 2) && (week = DayOfWeek.Monday)) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``海の日`` |> getEventName }
            else
              dayInfo
          else if (year >= 1996) then
            if (day = 20) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``海の日`` |> getEventName }
            else
              dayInfo
          else
            dayInfo

    let private august (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 8 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        match day with
        | 7 ->
          { dayInfo with dayType = DayType.Specialday; name = "月遅れ七夕"}
        | 11 ->
          if (year >= 2016) then
            { dayInfo with dayType = DayType.Syukujitsu; name = "山の日"}
          else
            dayInfo
        | 13 -> 
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``大鳥こはくの誕生日`` |> getEventName }
        | _ ->
          dayInfo

    let private september (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 9 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        let week = dayInfo.day.DayOfWeek

        let isSyubun (dayInfo:DayInfo) = 
          if (dayInfo.day.Month = 9 |> not) then false
          else
            if (year <= 1947) then false
            else if (year <= 1979) then
              day = int (23.2588 + (0.242194 * float(year - 1980)) - (float(year - 1983) / 4.))
            else if (year <= 2099) then
              day = int (23.2488 + (0.242194 * float(year - 1980)) - (float(year - 1980) / 4.))
            else if (year <= 2150) then
              day = int (24.2488 + (0.242194 * float(year - 1980)) - (float(year - 1980) / 4.))
            else false 

        match dayInfo |> isSyubun with
        | true ->
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``秋分の日`` |> getEventName }
        | false ->
          if (year >= 2003) then
            if ((int ((day - 1) / 7) = 2) && (week = DayOfWeek.Monday)) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``敬老の日`` |> getEventName }
            else if (week = DayOfWeek.Tuesday) then
              let yesterdayInfo = { dayInfo with day = dayInfo.day.AddDays(-1.); dayType = DayType.Weekday}
              if (yesterdayInfo |> isSyubun) then
                { dayInfo with dayType = DayType.Holiday; name = "国民の休日"}
              else
                dayInfo
            else
              dayInfo
          else if (year >= 1966) then
            if (day = 15) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``敬老の日`` |> getEventName }
            else
              dayInfo
          else
            dayInfo

    let private october (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 10 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        let week = dayInfo.day.DayOfWeek
        match day with
        | 8 ->
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``大鳥こはくのパパの誕生日`` |> getEventName }
        | _ ->
          if (year >= 2000) then
            if ((int ((day - 1) / 7) = 1) && (week = DayOfWeek.Monday)) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``体育の日`` |> getEventName }
            else
              dayInfo
          else if (year >= 1966) then
            if (day = 10) then
              { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``体育の日`` |> getEventName }
            else
              dayInfo
          else
            dayInfo

    let private november (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 11 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        match day with
        | 3 ->
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``文化の日`` |> getEventName }
        | 23 ->
          { dayInfo with dayType = DayType.Syukujitsu; name = EventVoice.``勤労感謝の日`` |> getEventName }
        | _ ->
          if ((year = 1990) && (day = 12)) then
            { dayInfo with dayType = DayType.Syukujitsu; name = "即位礼正殿の儀"}
          else
            dayInfo

    let private december (dayInfo:DayInfo) =
      if (dayInfo.day.Month = 12 |> not) then dayInfo
      else
        let year = dayInfo.day.Year
        let day = dayInfo.day.Day
        match day with
        | 23 ->
          if (year >= 1989) then
            { dayInfo with dayType = DayType.Syukujitsu; name = "天皇誕生日"}
          else
            dayInfo
        | 24 ->
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``クリスマス・イブ`` |> getEventName }
        | 25 ->
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``クリスマス`` |> getEventName }
        | 31 ->
          { dayInfo with dayType = DayType.Specialday; name = EventVoice.``大晦日`` |> getEventName }
        | _ ->
          dayInfo

    let  getDayInfo (targetDay:DateTime) =
      let rec getDayInfo' flg (targetDay:DateTime)  =

        let month = targetDay.Month
        let dayInfo = { day = DateTime.Today; dayType = DayType.Weekday; name = ""} //new DayInfo(day = targetDay)

        // 祝日法施行日以前
        if (targetDay < startSyukujitsu) then dayInfo
        else
          let dayInfo = 
            match month with
            | 1  -> dayInfo |> january
            | 2  -> dayInfo |> february
            | 3  -> dayInfo |> march
            | 4  -> dayInfo |> april
            | 5  -> dayInfo |> may
            | 6  -> dayInfo |> june
            | 7  -> dayInfo |> july
            | 8  -> dayInfo |> august
            | 9  -> dayInfo |> september
            | 10 -> dayInfo |> october
            | 11 -> dayInfo |> november
            | 12 -> dayInfo |> december
            | _  -> new Exception("Value of the month is invalid.") |> raise

          let substituteHoliday (dayInfo:DayInfo) =
            let week = dayInfo.day.DayOfWeek
            if ((dayInfo.dayType = DayType.Weekday || dayInfo.dayType = DayType.Holiday) && (week = DayOfWeek.Monday)) then
              if (flg && dayInfo.day >= startSubstituteHoliday) then
                let yesterday : DayInfo = dayInfo.day.AddDays(-1.)|> (getDayInfo' false)
                if (yesterday.dayType = DayType.Syukujitsu) then
                  { dayInfo with dayType = DayType.SubstituteHoliday; name = String.Format("振替休日({0})", yesterday.name)}
                else
                  dayInfo
              else
                dayInfo
            else
              dayInfo

          dayInfo |> substituteHoliday
      getDayInfo' true targetDay