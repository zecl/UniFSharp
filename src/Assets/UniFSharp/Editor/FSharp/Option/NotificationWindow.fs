namespace UniFSharp
open System
open UnityEngine
open UnityEditor

[<Sealed>]
type public NotificationWindow () =
  inherit EditorWindow ()

  static let showNotification (texturePath:string) = 
    let unityChanTexture = Resources.LoadAssetAtPath(FSharpBuildTools.unityChanRootPath + texturePath, typeof<Texture2D>) :?>Texture
    let guiContent = new GUIContent()
    guiContent.image <- unityChanTexture
    let window = NotificationWindow.ShowWindow()
    window.ShowNotification (guiContent)

  static member val size = 275.f with get
  static member ShowNotificationUnityChan () = @"Texture\unity-chan.png" |> showNotification
  static member ShowNotificationCompile () = @"Texture\sign_o_S.png" |> showNotification 
  static member ShowNotificationFailed () = @"Texture\sign_x_S.png" |> showNotification 

  static member ShowWindow() : NotificationWindow = 
    let window = EditorWindow.GetWindow<NotificationWindow>(false, FSharpBuildTools.ToolName, true)
    let size = NotificationWindow.size
    window.maxSize <- new Vector2(size, size)
    window.minSize <- window.maxSize
    window.ShowTab()
    window

  member this.OnGUI() =
    let unityChanTexture = Resources.LoadAssetAtPath(FSharpBuildTools.unityChanRootPath + @"Texture\author_bg1.png", typeof<Texture2D>) :?> Texture2D
    let size = NotificationWindow.size 
    let rowMax = float32 Screen.width / NotificationWindow.size + 1.f|> decimal |> Math.Ceiling |> int 
    let colMax = float32 Screen.height / NotificationWindow.size + 1.f |> decimal |> Math.Ceiling |> int
    let f row col = EditorGUI.DrawPreviewTexture(new Rect(size * float32 col, size * float32 row, size, size), unityChanTexture)
    for row in [0..rowMax] do
      for col in [0..colMax] do
        f row col
      
    let evt = Event.current
    
    let contextRect = new Rect(0.f, 0.f, Screen.width |> float32, Screen.height |> float32)
    if (evt.``type`` = EventType.ContextClick) then
        let mousePos = evt.mousePosition
        if (contextRect.Contains(mousePos)) then
          let menu = new GenericMenu()
          menu.AddItem(new GUIContent("Rebuild(Debug) %&D"), false, (fun _ -> EditorApplication.ExecuteMenuItem(FSharpBuildTools.ToolName + "/Rebuild(Debug)") |> ignore), "Rebuild(Debug)")
          menu.AddItem(new GUIContent("Rebuild(Release) %&R"), false, (fun _ -> EditorApplication.ExecuteMenuItem(FSharpBuildTools.ToolName + "/Rebuild(Release)") |> ignore), "Rebuild(Release)")
          menu.AddSeparator("")
          menu.AddItem(new GUIContent("Editor Rebuild(Debug) %&D"), false, (fun _ -> EditorApplication.ExecuteMenuItem(FSharpBuildTools.ToolName + "/Editor Rebuild(Debug)") |> ignore), "Editor Rebuild(Debug)")
          menu.AddItem(new GUIContent("Editor Rebuild(Release) %&R"), false, (fun _ -> EditorApplication.ExecuteMenuItem(FSharpBuildTools.ToolName + "/Editor Rebuild(Release)") |> ignore), "Editor Rebuild(Release)")
          menu.AddSeparator("")
          menu.AddItem(new GUIContent("Option %&O"), false, (fun _ -> EditorApplication.ExecuteMenuItem(FSharpBuildTools.ToolName + "/Option %&O") |> ignore), "Option")
          menu.ShowAsContext()
          evt.Use()

  interface IHasCustomMenu with
    member this.AddItemsToMenu(menu:GenericMenu) = 
      let f () = 
        NotificationWindow.ShowNotificationUnityChan ()
        UnityChan.playCheerVoice ()
      menu.AddItem(new GUIContent("Cheer"), false, fun () -> f())