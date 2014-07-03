namespace DTE
open System
open System.Linq 
open System.Runtime.InteropServices
open System.Runtime.InteropServices.ComTypes
open EnvDTE

module AutomateVisualStudio = 
  let is64BitProcess = (IntPtr.Size = 8)
  [<DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)>]
  extern [<MarshalAs(UnmanagedType.Bool)>] bool IsWow64Process([<In>] IntPtr hProcess, [<Out>] bool& wow64Process)

  [<CompiledName "InternalCheckIsWow64">]
  let internalCheckIsWow64 () = 
    let internalCheckIsWow64 () = 
      if ((Environment.OSVersion.Version.Major = 5 && Environment.OSVersion.Version.Minor >= 1) || Environment.OSVersion.Version.Major >= 6) then
        use p = System.Diagnostics.Process.GetCurrentProcess()
        let mutable retVal = false
        if (not <| IsWow64Process(p.Handle, &retVal)) then
          false
        else
          retVal
      else
        false

    is64BitProcess || internalCheckIsWow64()

  [<CompiledName "Is64BitOperatingSystem">]
  let is64BitOperatingSystem = is64BitProcess || internalCheckIsWow64 ()

  [<CompiledName "GetVisualStudioInstallationPath">]
  let getVisualStudioInstallationPath (version:string) =
    let installationPath = 
      if (is64BitOperatingSystem) then
        Registory.getReg (String.Format(@"SOFTWARE\Wow6432Node\Microsoft\VisualStudio\{0}", version)) "InstallDir"
      else
        Registory.getReg (String.Format(@"SOFTWARE\Microsoft\VisualStudio\{0}", version)) "InstallDir"
    installationPath + "devenv.exe"

  let openExternalScriptEditor vsVersion solutionPath = 
    let p = new System.Diagnostics.Process()
    p.StartInfo.Arguments <- solutionPath
    p.StartInfo.FileName <- getVisualStudioInstallationPath vsVersion
    p.Start()

  [<DllImport("ole32.dll")>]
  extern int CreateBindCtx(uint32 reserved, [<Out>] IBindCtx& ppbc)

  let marshalReleaseComObject(objCom: obj) =
    let i = ref 1
    if (objCom <> null && Marshal.IsComObject(objCom)) then
      while (!i > 0) do
        i := Marshal.ReleaseComObject(objCom)

  let getDTE' (processId:int) (dteVersion:string) =
    let progId = String.Format("!VisualStudio.DTE.{0}:", dteVersion) + processId.ToString()
        
    let mutable bindCtx : IBindCtx = null;
    let mutable rot : IRunningObjectTable= null;
    let mutable enumMonikers :IEnumMoniker = null;
    let mutable runningObject : obj = null
    
    try
      Marshal.ThrowExceptionForHR(CreateBindCtx(0u, &bindCtx))
      bindCtx.GetRunningObjectTable(&rot)
      rot.EnumRunning(&enumMonikers)

      let moniker = Array.create<IMoniker>(1) null
      let numberFetched = IntPtr.Zero
      let cont' = ref true 
      while (enumMonikers.Next(1, moniker, numberFetched) = 0 && !cont') do
        let runningObjectMoniker = moniker.[0]
        let mutable name = null

        try
          if (runningObjectMoniker <> null) then
            runningObjectMoniker.GetDisplayName(bindCtx, null, &name)
        with | :? UnauthorizedAccessException -> () // do nothing

        if (not <| String.IsNullOrEmpty(name) && String.Equals(name, progId, StringComparison.Ordinal)) then
          Marshal.ThrowExceptionForHR(rot.GetObject(runningObjectMoniker, &runningObject))
          cont' := false
    finally
      if (enumMonikers <> null) then
        enumMonikers |> marshalReleaseComObject
      if (rot <> null) then
        rot |> marshalReleaseComObject
      if (bindCtx <> null) then
        bindCtx |> marshalReleaseComObject
    runningObject :?> EnvDTE.DTE

  let tryGetDTE (dteVersion:string) (targetSolutionFullName:string) tryMax =
    let getVisualStudioProcesses () =
      System.Diagnostics.Process.GetProcesses() |> Seq.where(fun x -> try x.ProcessName = "devenv" with | _  ->false)

    try
      let retry = RetryBuilder(tryMax,1.)
      retry {
        return 
          getVisualStudioProcesses() |> Seq.tryPick(fun p ->
            let dte = getDTE' p.Id dteVersion
            if (targetSolutionFullName.ToLower() = dte.Solution.FullName.ToLower()) then
              Some (dte,p)
            else
              None)}
    with | _ -> None

  let showDocument (dte:EnvDTE.DTE) (documentFullName:string) =
      let targetItem = 
        retry{
          let targetItem = dte.Solution.FindProjectItem(documentFullName)
          if (targetItem = null) then 
            return None 
          else
            return Some targetItem }

      match targetItem with
       | None -> ()
       | Some target ->
        if (not <| target.IsOpen(Constants.vsViewKindCode)) then
          target.Open(Constants.vsViewKindCode) |> ignore
          target.Document.Activate()
        else
          target.Document.Activate() 

  let jumpToLine dte documentFullName lineNumber =
    showDocument dte documentFullName
    let selectionDocument = dte.ActiveDocument.Selection :?> EnvDTE.TextSelection
    try
      selectionDocument.GotoLine(lineNumber, true) 
    with | _ -> () 

