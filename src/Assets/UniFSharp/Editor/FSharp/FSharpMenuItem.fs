namespace UniFSharp
open System
open System.IO
open System.Text.RegularExpressions 
open System.Reflection
open System.Diagnostics
open UnityEngine
open UnityEditor

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FSharpMenuItem =

  let executeMSBuild (projectfiletype:ProjectFileType) (isdebug: bool) (action:int -> unit) = 
    let vsFSharpPrjectPath = FSharpProject.getFSharpProjectFilePath(projectfiletype)
    if (not <| File.Exists(vsFSharpPrjectPath)) then
      FSharpProject.createFSharpProjectFile (projectfiletype) |> ignore

    let outputAssemblyPath = FSharpProject.getOutputAssemblyPath(projectfiletype)
    let msBuildVersion = FSharpBuildToolsWindow.FSharpOption.msBuildVersion
    if (not <| FSharpBuildToolsWindow.FSharpOption.buildLogConsoleOutput) then
      let handler = DataReceivedEventHandler(fun o e -> ())
      Async.StartWithContinuations(
           async {
              return MSBuild.execute (msBuildVersion |> toAliasName) vsFSharpPrjectPath outputAssemblyPath  isdebug handler handler
              },
           (fun r -> action r),
           (fun ex -> Debug.LogException(ex)),
           (fun _ -> ()))
    else
      let outputHandler = DataReceivedEventHandler(fun o e -> if (e <> null && not <| System.String.IsNullOrEmpty(e.Data)) then Debug.Log(e.Data))
      let errorHandler = DataReceivedEventHandler(fun o e -> if (e <> null && not <| System.String.IsNullOrEmpty(e.Data)) then Debug.LogError(e.Data))
      Async.StartWithContinuations(
           async {
              return MSBuild.execute (msBuildVersion |> toAliasName) vsFSharpPrjectPath outputAssemblyPath isdebug outputHandler errorHandler
              },
           (fun r -> action r),
           (fun ex -> Debug.LogException(ex)),
           (fun _ -> ()))

  let action = fun r -> 
    if (r = 0) then
      NotificationWindow.ShowNotificationCompile()
      if (FSharpBuildToolsWindow.UnityChanOption.buildVoice) then
        UnityChan.playBuildVoice(BuildVoice.BuildComplete)
    else
      NotificationWindow.ShowNotificationFailed()
      if (FSharpBuildToolsWindow.UnityChanOption.buildVoice) then
        UnityChan.playBuildVoice(BuildVoice.BuildError)
    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate)

  [<MenuItem(FSharpBuildTools.ToolName + "/Rebuild(Debug)", false, 10)>]
  let rebuildDebug () = 
    executeMSBuild (ProjectFileType.VisualStudioNormal) true action

  [<MenuItem(FSharpBuildTools.ToolName + "/Rebuild(Release)", false, 11)>]
  let rebuildRelease () = 
    executeMSBuild (ProjectFileType.VisualStudioNormal) false action

  [<MenuItem(FSharpBuildTools.ToolName + "/", false, 20)>]
  let separator() = ()

  [<MenuItem(FSharpBuildTools.ToolName + "/Editor Rebuild(Debug)", false, 30)>]
  let editorRebuildDebug () = 
    executeMSBuild (ProjectFileType.VisualStudioEditor) true action

  [<MenuItem(FSharpBuildTools.ToolName + "/Editor Rebuild(Release)", false, 31)>]
  let editorRebuildRelease () = 
    executeMSBuild (ProjectFileType.VisualStudioEditor) false action

  type JumpInfo = { AssetPath:string; Extension:string; Line:int}
  let getActiveLog () =
    try
      let assembly = Assembly.Load("UnityEditor.dll")
      let typeOfConsoleWindow = assembly.GetType("UnityEditor.ConsoleWindow")
      let instanceOfConsole = typeOfConsoleWindow.GetField("ms_ConsoleWindow", BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Static).GetValue(null)
      let activeLog = typeOfConsoleWindow.GetField("m_ActiveText", BindingFlags.Public ||| BindingFlags.NonPublic ||| BindingFlags.Instance).GetValue(instanceOfConsole) |> string
      activeLog
    with 
    | :? TargetException as err ->
        Debug.LogError(err.Message)
        "please open the console. (window>console)"
    | _  as err-> err.Message

  let getJumpInfo (activeLog:string) =
    let pattern = @"(?<=\(at )(?<assetPath>[\S!\.]*?)(?=(?<extension>\.cs|\.fs?):(?<line>\d*?)\))"
    let re = new Regex(pattern, RegexOptions.IgnoreCase ||| RegexOptions.Singleline)
    re.Matches(activeLog) |> Seq.cast<Match>
    |> Seq.tryPick(fun m ->let assetPath = m.Groups.["assetPath"].Value.Trim()
                           let extension = m.Groups.["extension"].Value.Trim()
                           let line = System.Int32.Parse(m.Groups.["line"].Value.Trim())
                           Some { AssetPath = assetPath + extension; Extension = extension; Line = line})

  [<MenuItem("UniFSharp/ActiveLog JumpToLine %J",false, 80)>]
  let jumpToLine () = 
    let activeLog = getActiveLog()
    if (String.IsNullOrEmpty(activeLog)) then
      EditorUtility.DisplayDialog("ActiveLog JumpToLine", "Please select a log from ConsoleWindow.", "OK") |> ignore
    else
      let jumpInfo = getJumpInfo(activeLog)
      match jumpInfo with
      | None -> ()
      | Some jumpInfo ->
        let basePath = FSharpProject.getProjectRootPath()
        let fileName = getAbsolutePath basePath jumpInfo.AssetPath
        match jumpInfo.Extension with
        | FSharpBuildTools.fsExtension ->
          FSharpSolution.OpenExternalVisualStudio(SolutionType.FSharp, fileName, jumpInfo.Line) |> ignore 
        | ".cs" ->
          FSharpSolution.OpenExternalVisualStudio(SolutionType.CSharp, fileName, jumpInfo.Line) |> ignore
        | _ -> ()

  [<MenuItem("Assets/Create/F# Script/NewBehaviourScript",false, 70)>]
  let createNewBehaviourScript () = FSharpScriptCreateAsset.CreateFSharpScript "NewBehaviourScript.fs"

  [<MenuItem("Assets/Create/F# Script/NewModule", false, 71)>]
  let createNewModule () = FSharpScriptCreateAsset.CreateFSharpScript "NewModule.fs"

  [<MenuItem("Assets/Create/F# Script/", false, 80)>]
  let createSeparator () = ()

  [<MenuItem("Assets/Create/F# Script/NewTabWindow", false, 91)>]
  let createNewTabEditorWindow () = FSharpScriptCreateAsset.CreateFSharpScript "NewTabWindow.fs"

  [<MenuItem("Assets/Create/F# Script/", false, 100)>]
  let createSeparator2 () = ()

  [<MenuItem("Assets/Create/F# Script/more...", false, 101)>]
  let more () = MoreFSharpScriptWindow.ShowWindow()
