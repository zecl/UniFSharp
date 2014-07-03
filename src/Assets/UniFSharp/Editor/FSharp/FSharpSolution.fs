namespace UniFSharp
open System
open System.IO 
open System.Text 
open System.Text.RegularExpressions 
open System.Xml.Linq
open System.Runtime.InteropServices
open UnityEngine

type SolutionType =
  | CSharp
  | FSharp

[<AbstractClass; Sealed>]
type FSharpSolution private () =

  static let createSolutionFile (pathName:string) (resourceFile:string) (normalProjectGuid:string) (editorProjectGuid:string) =
    use sr = new StreamReader(resourceFile + FSharpBuildTools.txtExtension, new UTF8Encoding(false))
    use sw = File.CreateText(pathName)
    let fileName = Path.GetFileNameWithoutExtension(pathName)
    let slnGuidValue = Guid.NewGuid()
    let sln = Regex.Replace(sr.ReadToEnd(), "#SolutionGuid#", slnGuidValue |> string)
              |> fun sln -> Regex.Replace(sln, "#NormalProjectGuid#", normalProjectGuid)
              |> fun sln -> Regex.Replace(sln, "#EditorProjectGuid#", editorProjectGuid)
    sw.Write(sln)
    sw.Flush()
    sw.Close()
    fileName

  static let createVisualStudioSolutionFile () = 
    let vsSolutionFile = FSharpProject.getProjectRootPath() + FSharpBuildTools.vsSolutionFileName
    if (not <| File.Exists(vsSolutionFile)) then
      let normalProjectGuid = ProjectFileType.VisualStudioNormal |> getProjectGuid
      let editorProjectGuid = ProjectFileType.VisualStudioEditor |> getProjectGuid

      let vsSolutionPath = FSharpProject.getFSharpProjectPath(FSharpBuildTools.vsSolutionFileName)
      let templatePath = FSharpBuildTools.templatePath + FSharpBuildToolsWindow.FSharpOption.vsVersion.ToString() |> appendDirSep
      createSolutionFile vsSolutionPath (templatePath + FSharpBuildTools.vsSolutionFileName) normalProjectGuid editorProjectGuid
      |> ignore

  static let createMonoDevelopSolutionFile () = 
    let monoSolutionFile = FSharpProject.getProjectRootPath() + FSharpBuildTools.monoSolutionFileName
    if (not <| File.Exists(monoSolutionFile)) then
      let normalProjectGuid = ProjectFileType.MonoDevelopNormal |> getProjectGuid
      let editorProjectGuid = ProjectFileType.MonoDevelopEditor |> getProjectGuid
      let monoSolutionPath = FSharpProject.getFSharpProjectPath(FSharpBuildTools.monoSolutionFileName)
      let templatePath = FSharpBuildTools.templatePath + FSharpBuildToolsWindow.FSharpOption.monoVersion.ToString() |> appendDirSep
      createSolutionFile monoSolutionPath (templatePath + FSharpBuildTools.monoSolutionFileName) normalProjectGuid editorProjectGuid
      |> ignore

  static member CreateSolutionFile () =
    createVisualStudioSolutionFile ()
    createMonoDevelopSolutionFile ()

  static member OpenExternalVisualStudio (solutionType: SolutionType, fileName:string, [<Optional;DefaultParameterValue(null)>]?lineNumber:int) = 
    let vsVersion = FSharpBuildToolsWindow.FSharpOption.vsVersion |> toAliasName
    let solutionFilePath = 
      match solutionType with
      | SolutionType.CSharp -> 
        let projectName = FSharpProject.getUnityProjectName ()
        FSharpProject.getFSharpProjectPath(projectName + "-csharp.sln")
      | SolutionType.FSharp -> FSharpProject.getFSharpProjectPath(FSharpBuildTools.vsSolutionFileName)

    if (File.Exists(solutionFilePath)) then
      let args = 
        match lineNumber with
        | None -> vsVersion + " " + solutionFilePath + " " + fileName
        | Some x -> vsVersion + " " + solutionFilePath + " " + fileName + " " + string x

      let p = new System.Diagnostics.Process()
      p.StartInfo.Arguments <- args
      let dtePath = getAbsolutePath Application.dataPath (FSharpBuildTools.projectRootPath + @"Assembly\")
      p.StartInfo.FileName <- Path.Combine(dtePath, FSharpBuildTools.AutomateVisualStudioName)
      p.Start()
    else false

  static member OpenExternalMonoDevelop () = 
    let vsVersion = FSharpBuildToolsWindow.FSharpOption.vsVersion |> toAliasName
    FSharpProject.openExternalScriptEditor(Func<_>(fun () -> FSharpBuildTools.monoSolutionFileName), Func<_>(fun () -> FSharpProject.getMonoDevelopBuildInPath()))
