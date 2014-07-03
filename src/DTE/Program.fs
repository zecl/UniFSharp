namespace DTE
open System
open System.IO
open AutomateVisualStudio

module Program =
   
  [<EntryPoint>]
  let main argv = 
    let cmds = System.Environment.GetCommandLineArgs()
    if cmds.Length < 4 then 0 else

    let vsVersion = cmds.[1] // "12.0"
    let solutionPath = cmds.[2].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
    let targetDocumetFileName =  cmds.[3].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)

    if String.IsNullOrEmpty(vsVersion) then 0 else
    if File.Exists(solutionPath) |> not then 0 else
    if File.Exists(targetDocumetFileName) |> not then 0 else

    let active dte =
      if (cmds.Length = 4) then
        showDocument dte targetDocumetFileName
      else
        let lineNumber =  cmds.[4]
        if (lineNumber <> null) then
          let num = Int32.Parse(lineNumber)
          jumpToLine dte targetDocumetFileName num

    let dte = tryGetDTE vsVersion solutionPath 2
    match dte with
    | None -> 
      if openExternalScriptEditor vsVersion solutionPath then
        let dte = tryGetDTE vsVersion solutionPath 30
        dte |> Option.iter (fun (dte,p) -> active dte; Microsoft.VisualBasic.Interaction.AppActivate(p.Id))
    | Some (dte,p) -> 
      active dte
      Microsoft.VisualBasic.Interaction.AppActivate(p.Id)
    0