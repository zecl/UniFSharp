namespace UniFSharp
open System
open System.IO
open System.Text
open System.Xml.Serialization
open System.Reflection
open UnityEngine
open UnityEditor

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AudioUtil =

  [<CompiledName "PlayClip">]
  let playClip (clip:AudioClip) =
    let method' = 
      let unityEditorAssembly = typeof<AudioImporter>.Assembly
      let audioUtilClass = unityEditorAssembly.GetType("UnityEditor.AudioUtil")
      audioUtilClass.GetMethod(
          "PlayClip",
          BindingFlags.Static ||| BindingFlags.Public,
          null,
          [|typeof<AudioClip>|],
          null)
    method'.Invoke(null, [|clip|]) |> ignore
