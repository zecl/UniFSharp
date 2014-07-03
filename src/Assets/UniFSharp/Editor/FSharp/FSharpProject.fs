namespace UniFSharp
open System
open System.IO
open System.Xml 
open System.Xml.Linq 
open System.Text 
open System.Text.RegularExpressions 
open UnityEngine
open UnityEditorInternal 


[<AutoOpen>]
module FSharpProject =
  [<Literal>]
  let private xmlSourceFileDataEnter = "    <Compile Include=\""
  [<Literal>]
  let private xlmSourceFileDataEnding = "\" />\n"

  [<CompiledName "OpenFileNameSelector">]
  let openFileNameSelector () = 
    let editor = InternalEditorUtility.GetExternalScriptEditor()
    if (editor.Contains("Visual Studio")) then
      FSharpBuildTools.vsSolutionFileName
    else
      FSharpBuildTools.monoSolutionFileName

  [<CompiledName "GetMonoDevelopBuildInPath">]
  let getMonoDevelopBuildInPath () =
    let unityDirectory = UnityEditor.EditorApplication.applicationContentsPath |> (getUpDirectory 2)
    Path.Combine(unityDirectory, @"MonoDevelop\bin\MonoDevelop.exe") |> replaceDirSepFromAltSep

  [<CompiledName "EditorPathSelector">]
  let editorPathSelector () = 
    let editor = InternalEditorUtility.GetExternalScriptEditor()
    if (editor.Contains("Visual Studio")) then
      editor
    else
      getMonoDevelopBuildInPath()

  [<CompiledName "GetProjectRootPath">]
  let getProjectRootPath () = Application.dataPath |> (getUpDirectory 1) |> replaceDirSepFromAltSep |> appendDirSep 
  [<CompiledName "GetFSharpProjectFilePath">]
  let getFSharpProjectPath fsprojFileName = getProjectRootPath () + fsprojFileName
  [<CompiledName "GetNormalOutputAssemblyPath">]
  let getNormalOutputAssemblyPath () = (Application.dataPath + @"\Assembly-FSharp\") |> replaceDirSepFromAltSep
  [<CompiledName "GetEditorOutputAssemblyPath">]
  let getEditorOutputAssemblyPath () = (Application.dataPath + @"\Assembly-FSharp-Editor\") |> replaceDirSepFromAltSep

  let surround s = string Path.DirectorySeparatorChar + s + string Path.DirectorySeparatorChar
  [<CompiledName "GetJudgmentRelativePath">]
  let getJudgmentRelativePath pathName = 
    let basePath = getProjectRootPath()
    getRelativePath basePath (getAbsolutePath basePath pathName) |> getDirectoryName |> surround

  let containsEditorFolder pathName = 
    let directory = pathName |> getJudgmentRelativePath
    (directory |> replaceDirSepFromAltSep).Contains(surround "Editor")

  [<CompiledName "TemplateAssemblyName">]
  let templateAssemblyName pathName =
    if (containsEditorFolder pathName) then
      FSharpBuildToolsWindow.FSharpOption.assemblyNameEditor
    else
      FSharpBuildToolsWindow.FSharpOption.assemblyName

  [<CompiledName "TemplateRootNamespace">]
  let templateRootNamespace pathName =
    if (containsEditorFolder pathName) then
      FSharpBuildToolsWindow.FSharpOption.rootNameSpaceEditor
    else
      FSharpBuildToolsWindow.FSharpOption.rootNameSpace

  [<CompiledName "GetFSharpProjectFileName">]
  let getFSharpProjectFileName projectFileType =
    match (projectFileType:ProjectFileType) with
    | VisualStudioNormal -> FSharpBuildTools.vsNormalFsprojFileName
    | VisualStudioEditor -> FSharpBuildTools.vsEditorFsprojFileName
    | MonoDevelopNormal -> FSharpBuildTools.monoNormalFsprojFileName 
    | MonoDevelopEditor -> FSharpBuildTools.monoEditorFsprojFileName 

  [<CompiledName "GetFSharpProjectFilePath">]
  let getFSharpProjectFilePath projectFileType =
    let fsharpPrjectFileName = getFSharpProjectFileName projectFileType
    getFSharpProjectPath fsharpPrjectFileName

  [<CompiledName "GetOutputAssemblyPath">]
  let getOutputAssemblyPath projectFileType = 
    match (projectFileType:ProjectFileType) with
    | VisualStudioNormal -> getNormalOutputAssemblyPath()
    | VisualStudioEditor -> getEditorOutputAssemblyPath()
    | _ -> new System.Exception("erro") |> raise

  [<CompiledName "GetFSharpProjectTemplateFilePath">]
  let getFSharpProjectTemplateFilePath projectFileType = 
    let version = 
      match (projectFileType:ProjectFileType) with
      | VisualStudioNormal -> FSharpBuildToolsWindow.FSharpOption.vsVersion |> string
      | VisualStudioEditor -> FSharpBuildToolsWindow.FSharpOption.vsVersion |> string
      | MonoDevelopNormal -> FSharpBuildToolsWindow.FSharpOption.monoVersion |> string
      | MonoDevelopEditor -> FSharpBuildToolsWindow.FSharpOption.monoVersion |> string
    FSharpBuildTools.templatePath + version |> replaceDirSepFromAltSep |> appendDirSep

  [<CompiledName "GetAllFSharpScriptAssets">]
  let getAllFSharpScriptAssets (projectFileType:ProjectFileType) =
    let basePath = getProjectRootPath()
    match projectFileType with
    | VisualStudioNormal 
    | MonoDevelopNormal ->
      Directory.GetFiles(Application.dataPath, FSharpBuildTools.fsExtensionWildcard, SearchOption.AllDirectories)
               |> Seq.filter(fun pathName -> not <| containsEditorFolder pathName)
    | VisualStudioEditor
    | MonoDevelopEditor ->
      Directory.GetFiles(Application.dataPath, FSharpBuildTools.fsExtensionWildcard, SearchOption.AllDirectories)
               |> Seq.filter(fun pathName -> containsEditorFolder pathName)

  [<CompiledName "CreateProjectFile">]
  let createProjectFile (pathName:string, resourceFile:string, projectFileType:ProjectFileType) = 
    use enterSr = new StreamReader(resourceFile + "-enter" + FSharpBuildTools.txtExtension, new UTF8Encoding(false))
    use finishSr = new StreamReader(resourceFile + "-finish" + FSharpBuildTools.txtExtension, new UTF8Encoding(false))
    use sw = File.CreateText(pathName)

    let guidValue = System.Guid.NewGuid()
    let enter = Regex.Replace(enterSr.ReadToEnd(), "#TargetFrameworkVersion#", FSharpBuildToolsWindow.FSharpOption.netFramework |> toAliasName)
                |> fun enter -> Regex.Replace(enter, "#TargetFSharpCoreVersion#", FSharpBuildToolsWindow.FSharpOption.fsharpCoreVersion |> toAliasName)
                |> fun enter -> Regex.Replace(enter, "#RootNamespace#", templateRootNamespace pathName)
                |> fun enter -> Regex.Replace(enter, "#AssemblyName#", templateAssemblyName pathName)
                |> fun enter -> Regex.Replace(enter, "#UnityEnginePath#", UnityEditorInternal.InternalEditorUtility.GetEngineAssemblyPath())
                |> fun enter -> Regex.Replace(enter, "#ProjectGuid#", guidValue |> string)
                |> fun enter -> Regex.Replace(enter, "#OutputPath#", getNormalOutputAssemblyPath())
                |> fun enter -> Regex.Replace(enter, "#OutputPathEditor#", getEditorOutputAssemblyPath())
    sw.Write(enter)

    let files = getAllFSharpScriptAssets(projectFileType)
    files |> Seq.iter (fun file ->
      let basePath = Application.dataPath |> (getUpDirectory 1)
      let absolutePath = basePath |> PathUtil.getAbsolutePath(file)
      if File.Exists(absolutePath) then
        let relativePath = basePath |> getRelativePath(absolutePath)
        sw.Write("  <ItemGroup>")
        sw.Write(xmlSourceFileDataEnter + relativePath + xlmSourceFileDataEnding)
        sw.Write("  </ItemGroup>"))

    let finish = finishSr.ReadToEnd()
    sw.Write(finish)
    sw.Flush()
    guidValue |> string

  [<CompiledName "CreateFSharpProjectFile">]
  let createFSharpProjectFile (projectFileType:ProjectFileType) = 
    let fsprojFileName = getFSharpProjectFileName projectFileType
    let vsFsprojPath = getFSharpProjectPath fsprojFileName
    if (File.Exists(vsFsprojPath)) then
      File.Delete(vsFsprojPath)

    let templatePath = getFSharpProjectTemplateFilePath projectFileType
    let projectGUID = createProjectFile(vsFsprojPath, templatePath + fsprojFileName, projectFileType)
    projectGUID

  [<CompiledName "GetProjectGuid">]
  let getProjectGuid (projectFileType:ProjectFileType) = 
    let fsprojFileName = getFSharpProjectFileName projectFileType
    let vsFsprojPath = getFSharpProjectPath fsprojFileName
    if (not <| File.Exists(vsFsprojPath)) then
      createFSharpProjectFile(projectFileType) |> ignore

    let fsprojXDoc = XDocument.Load(vsFsprojPath)
    let ( !! ) : string -> XName = XName.op_Implicit
    let ns = "{" + String.Format("{0}", fsprojXDoc.Root.Attribute(!!"xmlns").Value) + "}"
    let projectGuid = fsprojXDoc.Root
                        .Elements(!!(ns + "PropertyGroup"))
                        .Elements(!!(ns + "ProjectGuid")) 
                        |> Seq.tryPick(fun x -> Some x)
    match projectGuid with
    | Some x -> x.Value
    | None -> ""

  let openExternalScriptEditor (openFileName:Func<string>, editorPath:Func<string>) = 
    let fsprojPath = getFSharpProjectPath(openFileName.Invoke())
    let p = new System.Diagnostics.Process()
    p.StartInfo.Arguments <- fsprojPath
    p.StartInfo.FileName <- editorPath.Invoke()
    p.Start()

  let getUnityProjectName () =
    let path = (Application.dataPath |> (getUpDirectory 1)) 
    let directory = path |> removeDirSep
    let index = directory.LastIndexOf(Path.DirectorySeparatorChar)
    let projectName = directory.Substring(index + 1, directory.Length - index - 1)
    projectName 

  let fsharpScriptCeatable targetPath =
    let allDirectoryName path =
      let path = path |> replaceDirSepFromAltSep |> appendDirSep |> (getAbsolutePath Application.dataPath) |> replaceDirSepFromAltSep 
      Directory.GetFiles(path, FSharpBuildTools.fsExtensionWildcard, SearchOption.AllDirectories) 
      |> Seq.filter (fun x -> x <> targetPath)
      |> Seq.map (getDirectoryName >> replaceDirSepFromAltSep) 
      |> Seq.append (seq { yield replaceDirSepFromAltSep Application.dataPath }) |> Seq.distinct

    let allDirectories = allDirectoryName Application.dataPath

    let path = targetPath |> (getAbsolutePath Application.dataPath) |> replaceDirSepFromAltSep 
    if allDirectories |> Seq.exists ((=)path) then
      true
    else
      let directories  = 
        allDirectories |> Seq.map (getRelativePath Application.dataPath) |> Seq.filter (fun x -> x <> "")
        |> Seq.map (fun x -> x.Split(Path.AltDirectorySeparatorChar)) |> Seq.collect id |> Seq.distinct 

      let getNewFolderRelativePath path =
        let targetPath = path
        let rec getNewFolderName' path  =
          let path = path |> replaceDirSepFromAltSep
          let beforePath = path
          let path = path |> (getUpDirectory 1) |> removeDirSep
          if allDirectories |> Seq.exists ((=)path) then
            beforePath
          else
            getNewFolderName' path
        getRelativePath (getNewFolderName' path) targetPath

      let folderNames = (getNewFolderRelativePath path).Split(Path.AltDirectorySeparatorChar) |> Seq.distinct 
      directories |> Seq.exists (fun x -> folderNames |> Seq.exists ((=)x)) |> not
       