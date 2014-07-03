namespace UniFSharp
open System
open UnityEngine
open UnityEditor

[<AbstractClass; Sealed; InitializeOnLoad>]
type CustomProjectView private () =

  static let projectWindowListElementOnGUI (guid:string) (selectionRect:Rect) =
    if (AssetDatabase.GetAssetPath(Selection.activeObject).EndsWith(FSharpBuildTools.fsExtension)) then
      let evt = Event.current
      CustomProjectView.DoubleClickHook(evt)
      CustomProjectView.KeyDownReturnHook(evt)
      CustomProjectView.HookContextMenu(selectionRect, evt)

  static let getActiveObjectFilePath () = 
    let path = AssetDatabase.GetAssetPath(Selection.activeObject)
    let basePath = FSharpProject.getProjectRootPath()
    getAbsolutePath basePath path

  static let addDefaultAssetsMenuItem (menuItem:string) (menu:GenericMenu)=
    let callMenuItemName = menuItem.Replace(" %R","") 
    let executeAssetsMenuItem menuItem = EditorApplication.ExecuteMenuItem("Assets/" + menuItem) |> ignore
    menu.AddItem(new GUIContent(menuItem), false, (fun _ -> executeAssetsMenuItem(callMenuItemName)), menuItem.Replace("/", ""))

  static do
    let handler = new EditorApplication.ProjectWindowItemCallback(projectWindowListElementOnGUI)
    EditorApplication.projectWindowItemOnGUI <- EditorApplication.ProjectWindowItemCallback.Combine(EditorApplication.projectWindowItemOnGUI, handler) :?> EditorApplication.ProjectWindowItemCallback

  static member DoubleClickHook (evt:Event) =
    if (evt.isMouse && evt.``type`` = EventType.MouseDown && evt.clickCount = 2) then
      FSharpSolution.OpenExternalVisualStudio(SolutionType.FSharp, getActiveObjectFilePath()) |> ignore
      evt.Use()

  static member HookContextMenu(selectionRect:Rect, evt:Event) =
    if (evt.``type`` = EventType.ContextClick) then
      let mousePos = evt.mousePosition
      if (selectionRect.Contains(mousePos)) then
        let menu = CustomProjectView.CreateContexetMenu ()
        menu.ShowAsContext()
        evt.Use()

  static member KeyDownReturnHook(evt:Event) =
    if (evt.``type`` = EventType.KeyDown && evt.keyCode = KeyCode.Return) then
      FSharpSolution.OpenExternalVisualStudio(SolutionType.FSharp,getActiveObjectFilePath()) |> ignore
      evt.Use()

  static member CreateContexetMenu () : GenericMenu =
    let menu = new GenericMenu()
    // Create
    menu |> addDefaultAssetsMenuItem("Create/Folder")
    menu.AddItem(new GUIContent("Create/"), false, (fun _ -> ()), "CreateSeparator")
    menu |> addDefaultAssetsMenuItem("Create/F# Script/NewBehaviourScript")
    menu |> addDefaultAssetsMenuItem("Create/F# Script/NewModule")
    menu |> addDefaultAssetsMenuItem("Create/F# Script/")
    menu |> addDefaultAssetsMenuItem("Create/F# Script/NewTabEditorWindow")
    menu |> addDefaultAssetsMenuItem("Create/F# Script/")
    menu |> addDefaultAssetsMenuItem("Create/F# Script/more...")
    menu |> addDefaultAssetsMenuItem("Create/Javascript")
    menu |> addDefaultAssetsMenuItem("Create/C# Script")
    menu |> addDefaultAssetsMenuItem("Create/Boo Script")
    menu |> addDefaultAssetsMenuItem("Create/Shader")
    menu |> addDefaultAssetsMenuItem("Create/Compute Shader")
    menu.AddItem(new GUIContent("Create/"), false, (fun _ -> ()), "CreateSeparator")
    menu |> addDefaultAssetsMenuItem("Create/Prefab")
    menu.AddItem(new GUIContent("Create/"), false, (fun _ -> ()), "CreateSeparator")
    menu |> addDefaultAssetsMenuItem("Create/Material")
    menu |> addDefaultAssetsMenuItem("Create/Cubemap")
    menu |> addDefaultAssetsMenuItem("Create/Lens Flare")
    menu.AddItem(new GUIContent("Create/"), false, (fun _ -> ()), "CreateSeparator")
    menu |> addDefaultAssetsMenuItem("Create/Animator Controller")
    menu |> addDefaultAssetsMenuItem("Create/Animation")
    menu |> addDefaultAssetsMenuItem("Create/Animator Override Controller")
    menu |> addDefaultAssetsMenuItem("Create/Avatar Mask")
    menu.AddItem(new GUIContent("Create/"), false, (fun _ -> ()), "CreateSeparator")
    menu |> addDefaultAssetsMenuItem("Create/Physic Material")
    menu |> addDefaultAssetsMenuItem("Create/Phyiscs2D Material")
    menu.AddItem(new GUIContent("Create/"), false, (fun _ -> ()), "CreateSeparator")
    menu |> addDefaultAssetsMenuItem("Create/GUI Skin")
    menu |> addDefaultAssetsMenuItem("Create/Custom Font")
 
    menu |> addDefaultAssetsMenuItem("Show in Explorer")
    menu.AddItem(new GUIContent("Open"), false, (fun _ -> FSharpSolution.OpenExternalVisualStudio(SolutionType.FSharp,getActiveObjectFilePath()) |> ignore), "Open")
    menu |> addDefaultAssetsMenuItem("Delete")
    menu.AddSeparator("")
    menu |> addDefaultAssetsMenuItem("Import New Asset...")

    // Import Package
    menu |> addDefaultAssetsMenuItem("Import Package/Custom Package...")
    menu.AddItem(new GUIContent("Import Package/"), false, (fun _ -> ()), "ImportPackageSeparator")
    menu |> addDefaultAssetsMenuItem("Import Package/Character Controller")
    menu |> addDefaultAssetsMenuItem("Import Package/Glass Refraction (Pro Only)")
    menu |> addDefaultAssetsMenuItem("Import Package/Image Effects (Pro Only)")
    menu |> addDefaultAssetsMenuItem("Import Package/Light Cookies")
    menu |> addDefaultAssetsMenuItem("Import Package/Light Flares")
    menu |> addDefaultAssetsMenuItem("Import Package/Particles")
    menu |> addDefaultAssetsMenuItem("Import Package/Physic Material")
    menu |> addDefaultAssetsMenuItem("Import Package/Projectors")
    menu |> addDefaultAssetsMenuItem("Import Package/Scripts")
    menu |> addDefaultAssetsMenuItem("Import Package/Skyboxes")
    menu |> addDefaultAssetsMenuItem("Import Package/Standard Assets (Mobile)")
    menu |> addDefaultAssetsMenuItem("Import Package/Terrain Assets")
    menu |> addDefaultAssetsMenuItem("Import Package/Tessellation Shaders (DX11)")
    menu |> addDefaultAssetsMenuItem("Import Package/Toon Shading")
    menu |> addDefaultAssetsMenuItem("Import Package/Tree Creator")
    menu |> addDefaultAssetsMenuItem("Import Package/Water (Basic)")
    menu |> addDefaultAssetsMenuItem("Import Package/Water (Pro Only)")

    menu |> addDefaultAssetsMenuItem("Export Package...")
    menu |> addDefaultAssetsMenuItem("Find References In Scene")
    menu |> addDefaultAssetsMenuItem("Select Dependencies")
    menu.AddSeparator("")
    menu |> addDefaultAssetsMenuItem("Refresh %R")
    menu |> addDefaultAssetsMenuItem("Reimport")
    menu.AddSeparator("")
    menu |> addDefaultAssetsMenuItem("Reimport All")
    menu.AddSeparator("")
    menu |> addDefaultAssetsMenuItem("Sync MonoDevelop Project")
    menu
