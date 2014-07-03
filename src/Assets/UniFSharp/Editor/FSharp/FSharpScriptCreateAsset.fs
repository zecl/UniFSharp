namespace UniFSharp
open System
open System.IO
open System.Text
open System.Text.RegularExpressions 
open UnityEngine
open UnityEditor
open UnityEditor.ProjectWindowCallback

type FSharpScriptCreateAsset () =
  inherit EndNameEditAction ()

  static member CreateScript defaultName templatePath =
    let directoryName = 
      let assetPath = AssetDatabase.GetAssetPath(Selection.activeObject)
      if String.IsNullOrEmpty (assetPath |> Path.GetExtension) then assetPath
      else assetPath |> getDirectoryName
    if fsharpScriptCeatable directoryName |> not then
      EditorUtility.DisplayDialog("Warning", "Folder name that contains the F# Script file,\n must be unique in the entire F# Project.", "OK") |> ignore
    else
      let icon = Resources.LoadAssetAtPath(FSharpBuildTools.fsharpIconPath, typeof<Texture2D>) :?> Texture2D
      ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<FSharpScriptCreateAsset>(), defaultName, icon, templatePath)

  static member CreateFSharpScript fileName = 
    let tempFilePath = FSharpBuildTools.fsharpScriptTemplatePath + fileName + FSharpBuildTools.txtExtension
    FSharpScriptCreateAsset.CreateScript fileName (tempFilePath)

  override this.Action(instanceId:int, pathName:string, resourceFile:string) = 
    use sr = new StreamReader(resourceFile, new UTF8Encoding(false))
    use sw = File.CreateText(pathName)
    let filename = Path.GetFileNameWithoutExtension(pathName).Replace(" ","")

    let guid () = System.Guid.NewGuid() |> string
    let text = Regex.Replace(sr.ReadToEnd(), "#ClassName#", filename)
                |> fun text -> Regex.Replace(text, "#ModuleName#", filename)
                |> fun text -> Regex.Replace(text, "#RootNamespace#", FSharpProject.templateRootNamespace pathName)
                |> fun text -> Regex.Replace(text, "#AssemblyName#", FSharpProject.templateAssemblyName pathName)
                |> fun text -> Regex.Replace(text, "#Guid#", guid())
    sw.Write(text)
    AssetDatabase.ImportAsset(pathName)
    let uo = AssetDatabase.LoadAssetAtPath(pathName, typeof<UnityEngine.Object>)
    ProjectWindowUtil.ShowCreatedAsset(uo)
