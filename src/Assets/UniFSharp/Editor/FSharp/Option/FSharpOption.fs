namespace UniFSharp
open System.Xml.Serialization

type MonoDevelopVersion =
  | [<AliasName("built-in")>] MonoDevelopBuiltIn = 0

type VsVersion =
  | [<AliasName("11.0")>] Vs2012 = 0
  | [<AliasName("12.0")>] Vs2013 = 1

type NetFramework =
  | [<AliasName("v2.0")>] Net20 = 0
  | [<AliasName("v3.5")>] Net35 = 1

type FSharpCoreVersion =
  | [<AliasName("2.3.0.0")>] FSharpCore2300 = 0

type MsBuildVersion =
  | [<AliasName("4.0")>] V40 = 0

type AssemblySearch =
  | [<AliasName("Simple")>] Simple = 0
  | [<AliasName("F# Compiler Service")>] CompilerService = 1

[<AllowNullLiteral>]
type FSharpOption () =

    // IDE
    member val foldoutIDE = true with get,set
    member val monoVersion = MonoDevelopVersion.MonoDevelopBuiltIn with get,set
    member val vsVersion = VsVersion.Vs2013 with get, set

    //fsproj
    member val foldoutFsprojDetail = false with get,set
    //"#TargetFrameworkVersion#"
    member val netFramework = NetFramework.Net35 with get, set

    //#TargetFSharpCoreVersion#
    member val fsharpCoreVersion = FSharpCoreVersion.FSharpCore2300 with get, set

    //#AssemblyName#
    member val assemblyName = "AssemblyFSharp" with get, set

    //"#RootNamespace#"
    member val rootNameSpace = "AssemblyFSharp" with get, set

    //#AssemblyName#
    member val assemblyNameEditor = "AssemblyFSharpEditor" with get, set

    // "#RootNamespace#"
    member val rootNameSpaceEditor = "AssemblyFSharpEditor" with get, set

    //MSBuild Version
    member val msBuildVersion = MsBuildVersion.V40 with get, set

    // Build Log Console Outoput
    member val foldoutOther = true with get,set
    member val buildLogConsoleOutput = true with get, set

    // AssemblySearch
    member val assemblySearch = AssemblySearch.Simple with get, set
