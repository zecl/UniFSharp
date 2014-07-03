namespace UniFSharp
open System
open UnityEditor
open UnityEngine

type CustomSections =
  | [<AliasName("F#")>] FSharp = 0
  | [<AliasName("UnityChan")>] UnityChan = 1
  | [<AliasName("About")>] About = 2

type public FSharpBuildToolsWindow () =
  inherit EditorWindow ()
  [<DefaultValue>]val mutable section : CustomSections
  [<DefaultValue>]val mutable preferencesSectionStyle : GUIStyle
  static let (!!) s = GUIStyle.op_Implicit s

  static let mutable unityChanOption : UnityChanOption = null
  static member UnityChanOption 
    with get () = 
      if (unityChanOption = null) then
        unityChanOption <- unityChanOption |> SerializerUtil.load
      unityChanOption

  static let mutable fsharpOption : FSharpOption = null
  static member FSharpOption 
    with get () = 
      if (fsharpOption = null) then
        fsharpOption <- fsharpOption |> SerializerUtil.load
      fsharpOption
 
  static let setWindowSize (window:EditorWindow) = 
    let width = EditorPrefs.GetFloat("UnityEditor.PreferencesWindoww", 500.f)
    let height = EditorPrefs.GetFloat("UnityEditor.PreferencesWindowh", 400.f)
    window.minSize <- new Vector2(width, height)
    window.maxSize <- window.minSize

  static member Initialize () =
    fsharpOption <- FSharpBuildToolsWindow.FSharpOption |> SerializerUtil.load 
    unityChanOption <- FSharpBuildToolsWindow.UnityChanOption |> SerializerUtil.load 

  static let openWindow () = 
    let window = UnityEditor.EditorWindow.GetWindow<FSharpBuildToolsWindow>(true, "UniFSharp" + " - F# Build Tools for Unity")
    window.section <- CustomSections.FSharp 
    FSharpBuildToolsWindow.Initialize ()
    let preferencesSectionStyle = !!"PreferencesSection"
    window.preferencesSectionStyle <- preferencesSectionStyle
    setWindowSize window
    window

  [<MenuItem("UniFSharp" + "/Option %&O", false, 50)>]
  static member ShowWindow() = 
    openWindow () |> ignore
    UnityChan.playChoiceVoice ChoiceVoice.Dore
  
  let ( !++ ) x = EditorGUI.indentLevel <- EditorGUI.indentLevel + x
  let ( !-- ) x = EditorGUI.indentLevel <- EditorGUI.indentLevel - x

  let horizontalBlock begin' = 
    begin'()
    disposable { GUILayout.EndHorizontal() }

  let verticalBlock begin' = 
    begin'()
    disposable { GUILayout.EndVertical() }

  member this.OnGUI () =
    try
      use hb = horizontalBlock (fun _ -> GUILayout.BeginHorizontal())
      this.DrawSectionBox()
      let innerBlock () = 
        use vb = verticalBlock (fun _ -> GUILayout.BeginVertical())
        this.DrawSections()
      innerBlock()
    with | e  -> Debug.LogException(e)

  member this.DrawSectionBox () = 
    use vb = verticalBlock (fun _ -> GUILayout.BeginVertical(GUILayout.Width(120.f)))
    GUILayout.Space(30.f)
    EditorGUIUtility.LookLikeControls(180.f, 0.f)
    GUI.DrawTexture(new Rect(0.f, 0.f, float32 Screen.width * 0.24f, float32 Screen.height), FSharpBuildToolsWindow.BackgroundTexture)
    let preferencesSectionBox = !!"PreferencesSectionBox"
    GUI.Label(new Rect(0.f, 0.f, float32 Screen.width * 0.24f, float32 Screen.height), "", preferencesSectionBox)
    this.preferencesSectionStyle.normal.textColor <- if EditorGUIUtility.isProSkin then new Color(0.7f, 0.7f, 0.7f, 1.f) else Color.black
    let names = Enum.GetValues(typeof<CustomSections>) |> Seq.cast<int> |> Seq.map (fun x -> x |> toAliasName'<CustomSections,int>) |> Seq.toArray 
    let s = GUILayout.SelectionGrid(this.section |> int, names, 1, this.preferencesSectionStyle)
    this.section <- enum<CustomSections> s
    this.preferencesSectionStyle.onNormal.background <- FSharpBuildToolsWindow.OnNomalTexture

  member this.DrawSections () = 
    let unityChanTexture = Resources.LoadAssetAtPath(FSharpBuildTools.unityChanRootPath + @"Texture\unity-chan-background.png", typeof<Texture2D>) :?> Texture
    match this.section with
    | CustomSections.FSharp -> 
      EditorGUI.DrawPreviewTexture(new Rect(130.f, 0.f, 446.f, 693.f), unityChanTexture)
      this.DrawFSharpSection ()
    | CustomSections.UnityChan -> 
      EditorGUI.DrawPreviewTexture(new Rect(130.f, 0.f, 446.f, 693.f), unityChanTexture)
      this.DrawUnityChanSection ()
    | CustomSections.About -> 
      this.DrawAboutSection ()
    | _ -> ()

  member this.EnumPopupAsAliasName<'TEnum when 'TEnum : enum<int>> (label:string, selectedValue:'TEnum) =
    let displayOptions = Enum.GetValues(typeof<'TEnum>) |> Seq.cast<'TEnum> |> Seq.map(fun x -> new GUIContent(x |> toAliasName, x |> toAliasName)) |> Seq.toArray
    let optionValues = Enum.GetValues(typeof<'TEnum>) |> Seq.cast<'TEnum> |> Seq.map(fun x -> box x :?> int) |> Seq.toArray
    let content = new GUIContent(label)
    let selection = EditorGUILayout.IntPopup(content, box selectedValue :?> int, displayOptions, optionValues) //|> enum<'TEnum>
    Enum.ToObject(typeof<'TEnum>, selection) :?> 'TEnum

  let drawBoxBar width = GUILayout.Box("", GUILayout.Width(width - 130.f), GUILayout.Height(1.f))
  member this.DrawFSharpSection () =
    let option = FSharpBuildToolsWindow.FSharpOption 
    let before = option.foldoutIDE
    option.foldoutIDE <- EditorGUILayout.Foldout(option.foldoutIDE, "Target IDE")
    !++1
    if (before <> option.foldoutIDE) then
      option |> SerializerUtil.save
    if (option.foldoutIDE) then
      let before = option.monoVersion
      option.monoVersion <- this.EnumPopupAsAliasName<MonoDevelopVersion>("MonoDevelop", option.monoVersion)
      if (before <> option.monoVersion) then
        option |> SerializerUtil.save

      let before = option.vsVersion
      option.vsVersion <- this.EnumPopupAsAliasName<VsVersion>("Visual Studio", option.vsVersion)
      if (before <> option.vsVersion) then
        option |> SerializerUtil.save
    
    !--1
    drawBoxBar this.position.width

    let before = option.foldoutFsprojDetail
    option.foldoutFsprojDetail <- EditorGUILayout.Foldout(option.foldoutFsprojDetail, "F# Project Detail")
    !++1
    if (before <> option.foldoutFsprojDetail) then
      option |> SerializerUtil.save
    if (option.foldoutFsprojDetail) then
      let before = option.netFramework
      option.netFramework <- this.EnumPopupAsAliasName<NetFramework>(".NET Framework", option.netFramework)
      if (before <> option.netFramework) then
        option |> SerializerUtil.save

      let before = option.fsharpCoreVersion
      option.fsharpCoreVersion <- this.EnumPopupAsAliasName<FSharpCoreVersion>("FSharp.Core", option.fsharpCoreVersion)
      if (before <> option.fsharpCoreVersion) then
        option |> SerializerUtil.save

      let before = option.msBuildVersion
      option.msBuildVersion <- this.EnumPopupAsAliasName<MsBuildVersion>("MSBuild", option.msBuildVersion)
      if (before <> option.msBuildVersion) then
        option |> SerializerUtil.save

      let before = option.assemblyName
      option.assemblyName <- EditorGUILayout.TextField("AssemblyName", option.assemblyName)
      if (before <> option.assemblyName) then
        option |> SerializerUtil.save

      let before = option.rootNameSpace
      option.rootNameSpace <- EditorGUILayout.TextField("RootNamespace", option.rootNameSpace)
      if (before <> option.rootNameSpace) then
        option |> SerializerUtil.save

      let before = option.assemblyNameEditor
      option.assemblyNameEditor <- EditorGUILayout.TextField("AssemblyName(Editor)", option.assemblyNameEditor)
      if (before <> option.assemblyNameEditor) then
        option |> SerializerUtil.save

      let before = option.rootNameSpaceEditor
      option.rootNameSpaceEditor <- EditorGUILayout.TextField("RootNamespace(Editor)", option.rootNameSpaceEditor)
      if (before <> option.rootNameSpaceEditor) then
        option |> SerializerUtil.save

    !--1
    drawBoxBar this.position.width

    let before = option.foldoutOther
    option.foldoutOther <- EditorGUILayout.Foldout(option.foldoutOther, "Other Settings")
    !++1
    if (before <> option.foldoutOther) then
        option |> SerializerUtil.save
    if (option.foldoutOther) then
      let before = option.buildLogConsoleOutput
      option.buildLogConsoleOutput <- EditorGUILayout.Toggle("Build Log Output", option.buildLogConsoleOutput)
      if (before <> option.buildLogConsoleOutput) then
        option |> SerializerUtil.save

      let before = option.assemblySearch 
      option.assemblySearch <- this.EnumPopupAsAliasName<AssemblySearch>("Assembly Search", option.assemblySearch)
      if (before <> option.assemblySearch) then
        option |> SerializerUtil.save
    !--1

  member this.DrawUnityChanSection () =
    let option = FSharpBuildToolsWindow.UnityChanOption

    let before = option.startingVoice
    option.startingVoice <- EditorGUILayout.Toggle("Starting Voice", option.startingVoice)
    if (before <> option.startingVoice) then
      option |> SerializerUtil.save

    let before = option.buildVoice
    option.buildVoice <- EditorGUILayout.Toggle("Build Voice", option.buildVoice)
    if (before <> option.buildVoice) then
      option |> SerializerUtil.save

    let before = option.progressVoice
    option.progressVoice <- EditorGUILayout.Toggle("Progress Voice", option.progressVoice)
    if (before <> option.progressVoice) then
      option |> SerializerUtil.save

    if (option.progressVoice) then
      let before = option.progressInterval
      option.progressInterval <- float <| EditorGUILayout.IntSlider("Progress Interval(minute)", option.progressInterval |> int, 1, 24 * 60)
      if (before <> option.progressInterval) then
        option |> SerializerUtil.save
    else
      GUILayout.BeginVertical(!!"")
      GUILayout.Space(16.f)
      GUILayout.EndVertical()

    let before = option.timeSignalVoice
    option.timeSignalVoice <- EditorGUILayout.Toggle("TimeSignal Voice", option.timeSignalVoice)
    if (before <> option.timeSignalVoice) then
      option |> SerializerUtil.save

    let before = option.eventVoice
    option.eventVoice <- EditorGUILayout.Toggle("Event Voice", option.eventVoice)
    if (before <> option.eventVoice) then
      option |> SerializerUtil.save

    let before = option.birthday
    option.birthday <- EditorGUILayout.TextField("Birthday", option.birthday)
    let birthday = ref DateTime.MinValue 
    if (DateTime.TryParse(option.birthday, birthday)) then
      if (before <> option.birthday) then
        option |> SerializerUtil.save
    else
      option.birthday <- before

    GUILayout.BeginVertical()
    GUILayout.Space(233.f)
    GUILayout.EndVertical()
    let texture = Resources.LoadAssetAtPath(FSharpBuildTools.unityChanRootPath + @"Texture\speaker.png", typeof<Texture2D>) :?> Texture
    if (GUILayout.Button(new GUIContent(texture))) then
      NotificationWindow.ShowNotificationUnityChan()
      UnityChan.playCheerVoice()

  member this.DrawAboutSection () =
    let mutable rect = new Rect(200.f, 60.f, 235.5f, 205.5f)
    let assmblyVersionInfo = System.Diagnostics.FileVersionInfo.GetVersionInfo(System.Reflection.Assembly.GetExecutingAssembly().Location)

    let drawLogoAndVersion rect =     
      use hb = horizontalBlock (fun _ -> GUILayout.BeginHorizontal(!!""))
      let lightSilhouetteTexture = Resources.LoadAssetAtPath(FSharpBuildTools.unityChanRootPath + @"Texture\imageLicenseLogo.png", typeof<Texture2D>) :?> Texture
      EditorGUI.DrawPreviewTexture(rect, lightSilhouetteTexture)
      //GUILayout.Space(70.f)
      GUILayout.Label(FSharpBuildTools.ToolName + " Version " + assmblyVersionInfo.FileVersion)
    drawLogoAndVersion rect

    let drawCopyright () = 
      use hb = horizontalBlock (fun _ -> GUILayout.BeginHorizontal(!!""))
      //GUILayout.Space(65.f)
      let inner () = 
        use vb = verticalBlock (fun _ -> GUILayout.BeginVertical(!!""))
        GUILayout.Label(assmblyVersionInfo.LegalCopyright)
      inner ()
    drawCopyright()

    GUILayout.Space(223.f)

    let drawLicense () =
      let en = @"These Asset are licensed under the ""Unity-Chan"" License Terms and Conditions. You are allowed to use these Asset only if you follow the Character Use Guidelines set by Unity Technologies Japan G.K., for the usage of its characters."
      //let ja = @"このアセットは、『ユニティちゃんライセンス』で提供されています。このアセットをご利用される場合は、『キャラクター利用のガイドライン』も併せてご確認ください。"
      EditorGUILayout.HelpBox(en, MessageType.Info, true)
    drawLicense ()

    rect.width <- 260.f 
    rect.height <- 15.f 
    rect.x <- rect.x - 70.f 
    rect.y <- rect.y + 273.f 

    // License & Guideline links
    let drawLinks rect =
      let mutable rect = rect
      use hb = horizontalBlock (fun _ -> GUILayout.BeginHorizontal())
      let labelSkin = new GUIStyle(GUI.skin.label)
      labelSkin.normal.textColor <- Color.blue
      let url1 = @"http://unity-chan.com/download/license.html"
      let urlc1 = new GUIContent(url1, "UnityChan License")
      if (GUI.Button(rect,urlc1, labelSkin)) then
        Application.OpenURL(url1)
      EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link)
      rect.y <- rect.y + 20.f 
      rect.width <- rect.width + 10.f 

      let url2 = @"http://unity-chan.com/download/guideline.html"
      let urlc2 = new GUIContent(url2, "UnityChan Guideline")
      if (GUI.Button(rect,urlc2, labelSkin)) then
          Application.OpenURL(url2)
      EditorGUIUtility.AddCursorRect (rect, MouseCursor.Link)
    drawLinks rect

    GUILayout.Space(40.f)

    let drawButton () = 
      let texture = Resources.LoadAssetAtPath(FSharpBuildTools.unityChanRootPath + @"Texture\speaker.png", typeof<Texture2D>) :?> Texture2D
      if (GUILayout.Button(new GUIContent(texture))) then
        NotificationWindow.ShowNotificationUnityChan()
        UnityChan.playLicenseVoice(LicenseVoice.LicenseContents)
    drawButton ()

  static let mutable onNomalTexture : Texture2D = null
  static let mutable skyBlue = new Color (0.35f , 0.7f , 0.9f)
  static member OnNomalTexture 
    with get () =
      if (onNomalTexture = null) then
        onNomalTexture <- new Texture2D(1, 1)
        onNomalTexture.SetPixel(0, 0, skyBlue)
        onNomalTexture.Apply()
      onNomalTexture
    
  static let mutable backgroundTexture : Texture2D = null
  static member BackgroundTexture 
    with get () =
      if (backgroundTexture = null) then
        backgroundTexture <- new Texture2D(1, 1)
        backgroundTexture.SetPixel(0, 0, skyBlue)
        backgroundTexture.Apply()
      backgroundTexture