namespace UniFSharp
open System.IO
open UnityEditor
open UnityEngine

type MoreFSharpScriptWindow () =
  inherit EditorWindow ()
  [<DefaultValue>]val mutable index : int

  static member ShowWindow() = 
    let window = ScriptableObject.CreateInstance<MoreFSharpScriptWindow>()
    window.title <- FSharpBuildTools.ToolName + " - F# Script"
    window.ShowUtility()

  member this.OnGUI() =

    let scripts = this.GetFSharpScript()
    this.index <- EditorGUILayout.Popup(this.index, scripts)
    if GUILayout.Button("Create") then
      let fileName = scripts.[this.index] 
      FSharpScriptCreateAsset.CreateFSharpScript fileName

  member this.GetFSharpScript () : string array = 
    Directory.GetFiles(FSharpBuildTools.fsharpScriptTemplatePath, FSharpBuildTools.txtExtensionWildcard)
    |> Array.map (fun x -> Path.GetFileName(x).Replace(Path.GetExtension(x),""))