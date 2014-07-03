namespace UniFSharp
open System
open System.Text
open System.Text.RegularExpressions
open System.IO
open System.Linq
open System.Xml.Linq
open UnityEditor
open UnityEngine

type FSharpScriptAssetPostprocessor () = 
  inherit AssetPostprocessor ()
  static let ( !! ) s = XName.op_Implicit s

  static let getXDocCompileIncureds (fsprojXDoc:XDocument) (ns:string) (projectFileType:ProjectFileType) =
    let elements = fsprojXDoc.Root.Elements(!!(ns + "ItemGroup")).Elements(!!(ns + "Compile"))
    elements |> Seq.map (fun x -> x.Attribute(!!"Include").Value |> replaceDirSepFromAltSep)

  static let getNewCompileIncludeElement(ns:string) (file:string) = XElement(!!(ns + "Compile"), new XAttribute(!!"Include", file))
  static let getNewItemGroupCompileIncludeElement (ns:string) (file:string) = XElement(!!(ns + "ItemGroup"), new XElement(!!(ns + "Compile"), new XAttribute(!!"Include", file)))
  static let getXDocComiles (fsprojXDoc:XDocument) (ns:string) = fsprojXDoc.Root.Elements(!!(ns + "ItemGroup")).Elements(!!(ns + "Compile"))

  static let getNotExitsFiles (compileIncludes:seq<string>) (projectFileType:ProjectFileType) =
    let basePath = FSharpProject.getProjectRootPath()
    let files = FSharpProject.getAllFSharpScriptAssets(projectFileType) 
                |> Seq.map (fun x -> getRelativePath basePath x)
    Seq.fold(fun acc file -> 
      let file = file |> replaceDirSepFromAltSep
      if not (compileIncludes |> Seq.exists ((=)file)) then 
        seq { yield! acc
              yield file } 
      else acc) Seq.empty files

  static let addCompileIncludeFiles (fsprojXDoc:XDocument) (ns:string) (compileIncludes:seq<string>) (projectFileType:ProjectFileType) =
    let notExists = getNotExitsFiles compileIncludes projectFileType
    notExists |> Seq.iter (fun file ->
      let newElem = getNewCompileIncludeElement ns file
      let compiles = getXDocComiles fsprojXDoc ns
      if (compiles.Any()) then
        let addPoint () =
          let directoryPoint = 
            compiles |> Seq.toList |> Seq.filter (fun x -> 
              let includeFile = x.Attribute(!!"Include").Value
              let includeDirectory = getDirectoryName(includeFile) |> replaceDirSepFromAltSep 
              let directory = getDirectoryName(file) |> replaceDirSepFromAltSep 
              includeDirectory = directory)

          if directoryPoint.Any() then
            directoryPoint |> Seq.toList
          else compiles|> Seq.toList
        addPoint().Last().AddAfterSelf(newElem)
      else
        let newItemGroupElem = getNewItemGroupCompileIncludeElement ns file
        fsprojXDoc.Root.Add(newItemGroupElem))

  static let getRemoveFiles (compileIncludes:seq<string>) (projectFileType:ProjectFileType) =
    let basePath = FSharpProject.getProjectRootPath()
    Seq.fold(fun acc ``include`` -> 
      let ``include`` = ``include`` |> replaceDirSepFromAltSep
      let files = FSharpProject.getAllFSharpScriptAssets(projectFileType) |> Seq.map (fun x -> getRelativePath basePath x) |> Seq.map (fun x -> x |> replaceDirSepFromAltSep)
      if (not <| files.Contains(``include``)) then 
        seq { yield! acc
              yield ``include`` } 
      else acc) Seq.empty compileIncludes

  static let removeCompileIncludeFiles (fsprojXDoc:XDocument) (ns:string) (compileIncludes:seq<string>) (projectFileType:ProjectFileType) =
    let removedFiles = getRemoveFiles compileIncludes projectFileType
    removedFiles |> Seq.iter (fun file -> 
      let compileItems = (fsprojXDoc.Root.Elements(!!(ns + "ItemGroup")).Elements(!!(ns + "Compile")))
      if compileItems |> Seq.length = 1 && (compileItems |> Seq.exists (fun x -> x.Attribute(!!"Include").Value = file)) then
        let parent = compileItems |> Seq.map(fun x -> x.Parent) |> Seq.head 
        parent.Remove()
      else
        (compileItems |> Seq.filter (fun x -> x.Attribute(!!"Include").Value = file)).Remove())    

  static let createOrUpdateProject (projectFileType:ProjectFileType) =
    let fsprojFileName = FSharpProject.getFSharpProjectFileName(projectFileType)
    let fsprojFilePath = FSharpProject.getFSharpProjectPath(fsprojFileName)
    if (not <| File.Exists(fsprojFilePath)) then
      FSharpProject.createFSharpProjectFile(projectFileType) |> ignore
    else
      let fsprojXDoc = XDocument.Load(fsprojFilePath)
      let ns = "{" + String.Format("{0}", fsprojXDoc.Root.Attribute(!!"xmlns").Value) + "}"
      let compileIncludes = getXDocCompileIncureds fsprojXDoc ns projectFileType
      addCompileIncludeFiles fsprojXDoc ns compileIncludes projectFileType
      removeCompileIncludeFiles fsprojXDoc ns compileIncludes projectFileType
      fsprojXDoc.Save(fsprojFilePath)

  static let deleteProject (projectFileType:ProjectFileType) (assetPath:string) =
    let assetPath = assetPath |> replaceDirSepFromAltSep 
    let fsprojFileName = FSharpProject.getFSharpProjectFileName projectFileType
    if (File.Exists(fsprojFileName)) then
      let basePath = FSharpProject.getProjectRootPath()
      let fsprojXDoc = XDocument.Load(fsprojFileName)
      let ns = "{" + String.Format("{0}", fsprojXDoc.Root.Attribute(!!"xmlns").Value) + "}"
      let compileIncludes = fsprojXDoc.Root
                                      .Elements(!!(ns + "ItemGroup"))
                                      .Elements(!!(ns + "Compile")) 
                            |> Seq.map (fun x -> x.Attribute(!!"Include").Value)
      let compileIncludes = compileIncludes |> Seq.map (fun x -> x |> replaceDirSepFromAltSep)
      fsprojXDoc.Root
                .Elements(!!(ns + "ItemGroup"))
                .Elements(!!(ns + "Compile")) 
                .Where(fun x -> x.Attribute(!!"Include").Value |> replaceDirSepFromAltSep = assetPath).Remove()
      fsprojXDoc.Save(fsprojFileName)
    else ()

  static let createOrUpdateEditor () =
      ProjectFileType.VisualStudioEditor |> createOrUpdateProject
      ProjectFileType.MonoDevelopEditor  |> createOrUpdateProject

  static let createOrUpdateNormal () = 
      ProjectFileType.VisualStudioNormal |> createOrUpdateProject
      ProjectFileType.MonoDevelopNormal  |> createOrUpdateProject

  static let createOrUpdate () = 
    createOrUpdateNormal()
    createOrUpdateEditor()

  static let filterFSharpScript x = x |> Seq.filter(fun assetPath -> Path.GetExtension(assetPath) = FSharpBuildTools.fsExtension)

  static let onImportedAssets(importedAssets) = 
    importedAssets |> filterFSharpScript |> fun _ -> createOrUpdate ()
    UniFSharp.FSharpSolution.CreateSolutionFile()

  static let onDeletedAssets(deletedAssets) = 
    deletedAssets |> filterFSharpScript
    |> Seq.iter (fun assetPath ->
      if (FSharpProject.containsEditorFolder assetPath) then
        deleteProject ProjectFileType.VisualStudioEditor assetPath
        deleteProject ProjectFileType.MonoDevelopEditor assetPath
      else
        deleteProject ProjectFileType.VisualStudioNormal assetPath
        deleteProject ProjectFileType.MonoDevelopNormal assetPath)

  static let onMovedAssets(movedAssets) = 
    movedAssets |> filterFSharpScript
    |> Seq.iter (fun assetPath ->
      let assetAbsolutePath = assetPath |> (getAbsolutePath Application.dataPath)
      let fileName = assetAbsolutePath |> Path.GetFileName 
      if fsharpScriptCeatable assetAbsolutePath |> not then
        EditorUtility.DisplayDialog("Warning", "Folder name that contains the F# Script file,\n must be unique in the entire F# Project.\nMove to Assets Folder.", "OK") |> ignore
        AssetDatabase.MoveAsset(assetPath, "Assets/" + fileName) |> ignore
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate))

  static let onMovedFromPathAssets(movedFromPath) = 
    if movedFromPath |> filterFSharpScript |> Seq.exists (fun _ -> true) then
      createOrUpdateNormal()
    
  static member OnPostprocessAllAssets (importedAssets:string array, deletedAssets:string array, movedAssets:string array, movedFromPath:string array) = 
    onImportedAssets importedAssets
    onDeletedAssets deletedAssets
    onMovedAssets movedAssets
    onMovedFromPathAssets movedFromPath