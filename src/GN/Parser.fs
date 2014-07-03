module Parser
open System
open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Compiler.Ast

let private checker = InteractiveChecker.Create()
let private getUntypedTree (file, input, conditionalDefines) = 
  let otherFlags = 
    match conditionalDefines with
    | [||] -> [||] 
    | _  -> conditionalDefines |> Array.map (fun x -> "--define:" + x )

  let checkOptions = checker.GetProjectOptionsFromScript(file, input, otherFlags = otherFlags) |> Async.RunSynchronously
  let untypedRes = checker.ParseFileInProject(file, input, checkOptions) |> Async.RunSynchronously
  match untypedRes.ParseTree with
  | Some tree -> tree
  | None -> failwith "failed to parse"

let rec private getAllFullNameOfType' modulesOrNss =
  modulesOrNss |> Seq.map(fun moduleOrNs -> 
    let (SynModuleOrNamespace(lid, isModule, moduleDecls, xmlDoc, attribs, synAccess, m)) = moduleOrNs
    let topNamespaceOrModule = String.Join(".",(lid.Head::lid.Tail))
    //inner modules
    let modules = moduleDecls.Head::moduleDecls.Tail 
    getDeclarations modules |> Seq.map (fun x -> String.Join(".", [topNamespaceOrModule;x]))
    ) |> Seq.collect id

and private getDeclarations moduleDecls = 
  Seq.fold (fun acc declaration -> 
      match declaration with
      | SynModuleDecl.NestedModule(componentInfo, modules, _isContinuing, _range) ->
        match componentInfo with
        | SynComponentInfo.ComponentInfo(_,_,_,lid,_,_,_,_) ->
          let moduleName = String.Join(".",(lid.Head::lid.Tail))
          let children = getDeclarations modules
          seq {
            yield! acc
            yield! children |> Seq.map(fun child -> moduleName + "+" + child) }
      | SynModuleDecl.Types(typeDefs, _range) ->
        let types = 
          typeDefs |> Seq.map(fun typeDef ->
          match typeDef with
          | SynTypeDefn.TypeDefn(componentInfo,_,_,_) ->
          match componentInfo with
          | SynComponentInfo.ComponentInfo(_,typarDecls,_,lid,_,_,_,_) ->
            let typarString = typarDecls |> function | [] -> "" | x -> "`" + string x.Length 
            let typeName = String.Join(".",(lid.Head::lid.Tail))
            typeName + typarString)
        seq {
          yield! acc
          yield! types }
      | _ -> acc
    ) Seq.empty moduleDecls

let getAllFullNameOfType input conditionalDefines = 
  let tree = getUntypedTree("/dummy.fsx", input, conditionalDefines) 
  match tree with
  | ParsedInput.ImplFile(ParsedImplFileInput(file, isScript, qualName, pragmas, hashDirectives, modules, b)) ->
    getAllFullNameOfType' modules 
  | _ -> failwith "(*.fsi) not supported."

