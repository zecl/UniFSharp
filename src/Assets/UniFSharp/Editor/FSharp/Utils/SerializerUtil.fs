namespace UniFSharp
open System
open System.IO
open System.Text
open System.Xml.Serialization
open System.Reflection
open UnityEngine
open UnityEditor

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module SerializerUtil =
  [<CompiledName "Load">]
  let load (target:'T)  = 
    async {
      let serializer = new XmlSerializer(typeof<'T>)
      let fileName = typeof<'T>.Name
      let filePath = String.Format(@"{0}{1}.xml", FSharpBuildTools.settingsPath, fileName)

      if (File.Exists(filePath) |> not) then
        return new 'T()
      else
        use sr = new StreamReader(filePath, new UTF8Encoding(false))
        return serializer.Deserialize(sr) :?> 'T
    } |> Async.RunSynchronously 

  [<CompiledName "Save">]
  let save (target:'T) = 
     Async.StartWithContinuations(
         async {
              let fileName = typeof<'T>.Name
              let filePath = String.Format(@"{0}{1}.xml", FSharpBuildTools.settingsPath, fileName)

              let serializer = new XmlSerializer(typeof<'T>)
              use sw = new StreamWriter(filePath, false, new UTF8Encoding(false))
              serializer.Serialize(sw, target)
            },
         (fun _ -> ()(* AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate) *)),
         (fun ex -> Debug.LogException(ex)),
         (fun _ -> ()))


