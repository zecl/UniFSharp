module Program
open System
open System.IO 
open Microsoft.FSharp.Compiler.Ast
open Parser

[<EntryPoint>]
let main argv = 
  let cmds = System.Environment.GetCommandLineArgs()
  if cmds.Length < 2 then 0 else

  let fileName = cmds.[1].Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar)
  let conditionalDefines = 
    if cmds.Length > 2 then cmds.[2].Split(';') 
    else [||]

  let input = File.ReadAllText(fileName)
  getAllFullNameOfType input conditionalDefines 
  |> Seq.iter(fun x -> printfn "%s" x)
  0