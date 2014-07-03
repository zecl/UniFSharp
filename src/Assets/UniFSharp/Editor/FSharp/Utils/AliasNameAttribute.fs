namespace UniFSharp
open System
open System.Runtime.CompilerServices 

[<AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)>]
type AliasNameAttribute (aliasName:string)  =
  inherit Attribute ()
  member this.AliasName with get () = aliasName

[<AutoOpen>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module AliasNameAttribute = 

  [<CompiledName "ToAliasName">]
  let toAliasName (value:'TEnum when 'TEnum : enum<'U>) = 
    let result = value.GetType()
                  .GetField(value.ToString())
                  .GetCustomAttributes(typeof<AliasNameAttribute>, false)
                  |> Seq.cast<AliasNameAttribute> 
                  |> Seq.tryFind (fun x -> box x = null |> not)
     
    match result with
    | None -> new ArgumentException("AliasNameAttribute is not found.") |> raise
    | Some x -> x.AliasName

  let toAliasName'<'TEnum, 'U when 'TEnum : enum<'U>> (value:'U) = 
    Enum.ToObject(typeof<'TEnum>, value) :?> 'TEnum |> toAliasName  