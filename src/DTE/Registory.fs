namespace DTE
open System
open System.Runtime.InteropServices
open System.IO
open Microsoft.Win32

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Registory =
  let getReg keyPath valueName = 
    try
      use rKey = Registry.LocalMachine.OpenSubKey(keyPath)
      let location = rKey.GetValue(valueName) |> string
      rKey.Close()
      location
    with e ->
      new Exception(String.Format("registry key:[{0}] value:[{1}] is not found.", keyPath, valueName)) |> raise