namespace UniFSharp
open System
open System.Runtime.InteropServices
open UnityEngine
open UnityEditorInternal 

type ProjectFileType =
  | VisualStudioNormal
  | VisualStudioEditor
  | MonoDevelopNormal
  | MonoDevelopEditor

[<Sealed>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module FSharpBuildTools =
  [<Literal>]
  let ToolName = "UniFSharp"
  [<Literal>]
  let AutomateVisualStudioName = "DTE.exe"

  [<Literal>]
  let fsExtension = ".fs"
  [<Literal>]
  let txtExtension = ".txt"
  [<Literal>]
  let fsExtensionWildcard = "*.fs"
  [<Literal>]
  let txtExtensionWildcard = "*.txt"
  [<Literal>]
  let monoNormalFsprojFileName = "Assembly-FSharp.fsproj"
  [<Literal>]
  let monoEditorFsprojFileName = "Assembly-FSharp-Editor.fsproj"
  [<Literal>]
  let monoSolutionFileName = "Assembly-FSharp.sln"
  [<Literal>]
  let vsSolutionFileName = "Assembly-FSharp-vs.sln"
  [<Literal>]
  let vsNormalFsprojFileName = "Assembly-FSharp-vs.fsproj"
  [<Literal>]
  let vsEditorFsprojFileName = "Assembly-FSharp-Editor-vs.fsproj"
  [<Literal>]
  let projectRootPath = @"Assets\" + ToolName + @"\"
  [<Literal>]
  let projectEditorPath = @"Editor\"
  [<Literal>]
  let templatePath = projectRootPath + @"Template\"
  [<Literal>]
  let settingsPath = projectRootPath + @"Settings\"
  [<Literal>]
  let fsharpScriptTemplatePath = templatePath + @"FSharpScript\"
  [<Literal>]
  let fsharpIconPath = projectRootPath + @"Texture\fsharp_icon.png"
  [<Literal>]
  let unityChanRootPath = projectRootPath + @"UnityChan\"

