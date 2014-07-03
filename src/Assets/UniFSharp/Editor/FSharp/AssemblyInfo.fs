module AssemblyInfo

open System.Resources
open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyVersion("0.8.0")>]
[<assembly: AssemblyFileVersion("0.8.0")>]
[<assembly: AssemblyInformationalVersion("0.8.0")>]

[<assembly: AssemblyTitle("UniFSharp")>]
[<assembly: AssemblyDescription("F# Build Tools for Unity.")>]
[<assembly: AssemblyCompany("")>]
[<assembly: AssemblyProduct("UniFSharp")>]
[<assembly: AssemblyCopyright("Copyright (C) 2014 zecl")>]
[<assembly: AssemblyTrademark("")>]
[<assembly: AssemblyCulture("")>]
[<assembly: NeutralResourcesLanguage("ja-JP")>]

[<assembly: ComVisible(false)>]
[<assembly: Guid("15C38CCD-4594-4649-8CC9-5A6880B012F1")>]

#if DEBUG
[<assembly: InternalsVisibleTo("UniFSharp.Test")>]
#endif

#if DEBUG
[<assembly: AssemblyConfiguration("Debug")>]
#else
[<assembly: AssemblyConfiguration("Release")>]
#endif

do()