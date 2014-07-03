namespace UniFSharp
open System
open System.IO
open System.Runtime.CompilerServices 

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PathUtil =

  [<CompiledName "GetDirectoryName">]
  let getDirectoryName path = 
    let directoryName = Path.GetDirectoryName path
    if directoryName = null then "" else directoryName

  [<CompiledName "ReplaceDirSepFromAltSep">]
  let replaceDirSepFromAltSep path =
    if path = null then path else
    (path:string).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)

  [<CompiledName "ReplaceDirAltSepFromSep">]
  let replaceDirAltSepFromSep path =
    if path = null then path else
    (path:string).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)

  [<CompiledName "AppendDirSep">]
  let appendDirSep path =
    let path = path |> replaceDirSepFromAltSep
    let path' = path |> getDirectoryName |> replaceDirSepFromAltSep
    if (path <> path') then
      path + string Path.DirectorySeparatorChar
    else path

  [<CompiledName "RemoveDirSep">]
  let removeDirSep (path:string) =
    let r = path |> replaceDirSepFromAltSep
    if r.Substring(r.Length - 1,1) = string Path.DirectorySeparatorChar then 
      r.Substring(0, r.Length - 1) 
    else r

  [<CompiledName "GetCurrentFolderName">]
  let getCurrentFolderName (path:string) =
    let path = path |> (getDirectoryName >> replaceDirSepFromAltSep)
    let index = path.LastIndexOf(Path.DirectorySeparatorChar)
    if index > 0 then
      path.Substring(index + 1, path.Length - index - 1)
    else failwith "NotFound"

  [<CompiledName "GetUpDirectory">]
  let getUpDirectory count (filePath:string) = 
    let rec getUpDirectory' (filePath:string) n = 
      match n - 1 < 0 with
      | true -> filePath
      | _ -> 
        let filePath = filePath |> replaceDirSepFromAltSep
        let index = filePath.LastIndexOf(string Path.DirectorySeparatorChar)
        if index < 0 then filePath else
        let dinfo = (Directory.GetParent filePath) 
        let result = if dinfo = null then "" else dinfo.FullName
        if n - 1 = 0 then
          result
        else
          getUpDirectory' result (n-1)
    getUpDirectory' filePath count |> replaceDirSepFromAltSep |> removeDirSep

  [<CompiledName "GetRelativePath">]
  let getRelativePath (basePath:string) (targetPath:string) = 
    let targetPath = targetPath |> replaceDirSepFromAltSep
    let basePath = basePath.Replace("%", "%25")
    let filePath = targetPath.Replace("%", "%25")
    let u1 = new Uri(basePath)
    let u2 = new Uri(targetPath)
    let relativeUri = u1.MakeRelativeUri(u2)
    let relativePath = relativeUri.ToString() |> Uri.UnescapeDataString
    relativePath.Replace("%25", "%")

  [<CompiledName "GetAbsolutePath">]
  let getAbsolutePath (basePath:string) (targetPath:string) = 
    let basePath = basePath |> replaceDirSepFromAltSep
    let targetPath = targetPath |> replaceDirSepFromAltSep
    let basePath = basePath.Replace("%", "%25")
    let filePath = targetPath.Replace("%", "%25")
    let u1 = new Uri(basePath)
    let u2 = new Uri(u1, targetPath)
    let absolutePath = (u2.LocalPath).Replace("%25", "%")
    absolutePath
