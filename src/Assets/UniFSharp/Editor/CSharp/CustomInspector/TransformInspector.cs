using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UniFSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[CustomEditor(typeof(UnityEngine.Transform))]
public class TransformInspector : Editor
{
    Vector3 position;

    void OnEnable()
    {
        Repaint();
    }

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginVertical();
        (this.target as Transform).localRotation = Quaternion.Euler(EditorGUILayout.Vector3Field("Local Rotation", (this.target as Transform).localRotation.eulerAngles));
        (this.target as Transform).localPosition = EditorGUILayout.Vector3Field("Local Position", (this.target as Transform).localPosition);
        (this.target as Transform).localScale = EditorGUILayout.Vector3Field("Local Scale", (this.target as Transform).localScale);
        EditorGUILayout.EndVertical();

        // F# Script Drag % Drop
        if (DragAndDrop.objectReferences.Length > 0 && AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]).EndsWith(".fs"))
        {
            DragDropArea<UnityEngine.Object>(null, draggedObjects => 
            {
                var dropTarget = this.target as Transform;


                foreach (var draggedObject in draggedObjects)
                {

                    var outputPath = FSharpProject.GetNormalOutputAssemblyPath();
                    if (!Directory.Exists(outputPath))
                    {
                        EditorUtility.DisplayDialog("Warning", "F# Assembly is not found.\nPlease Build.", "OK");
                        break;
                    }


                    var notfound = true;
                    foreach (var dll in Directory.GetFiles(outputPath, "*.dll"))
                    {
                        var fileName = Path.GetFileName(dll);
                        if (fileName == "FSharp.Core.dll") continue;

                        var assem = Assembly.LoadFrom(dll);
                        IEnumerable<Type> behaviors = null;
                        switch (UniFSharp.FSharpBuildToolsWindow.FSharpOption.assemblySearch)
	                    {
                            case AssemblySearch.Simple:
                                var @namespace = GetNameSpace(AssetDatabase.GetAssetPath(draggedObject));
                                var typeName = GetTypeName(AssetDatabase.GetAssetPath(draggedObject));
                                behaviors = assem.GetTypes().Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && type.FullName == @namespace + typeName);
                                break;
		                    case AssemblySearch.CompilerService:
                                var types = GetTypes(AssetDatabase.GetAssetPath(draggedObject));
                                behaviors = assem.GetTypes().Where(type => typeof(MonoBehaviour).IsAssignableFrom(type) && types.Contains(type.FullName));
                                break;
                            default:
                                 break;
                        }

                        if (behaviors != null && behaviors.Any())
                        {
                            DragAndDrop.AcceptDrag();
                            foreach (var behavior in behaviors)
                            {
                                dropTarget.gameObject.AddComponent(behavior);
                                notfound = false;
                            }
                        }
                    }

                    if (notfound)
                    {
                        EditorUtility.DisplayDialog("Warning", "MonoBehaviour is not found in the F # assembly.", "OK");
                        return;
                    }
                }
            }, null, 50);
        }
    }

    public static void DragDropArea<T>(string label, Action<IEnumerable<T>> onDrop, Action onMouseUp, float height = 50) where T : UnityEngine.Object
    {
        GUILayout.Space(15f);
        Rect dropArea = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
        if (label != null) GUI.Box(dropArea, label);

        Event currentEvent = Event.current;
        if (!dropArea.Contains(currentEvent.mousePosition)) return;

        if (onMouseUp != null)
            if (currentEvent.type == EventType.MouseUp)
                onMouseUp();
        
        if (onDrop != null)
        {
            if (currentEvent.type == EventType.DragUpdated ||
                currentEvent.type == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (currentEvent.type == EventType.DragPerform)
                {
                    EditorGUIUtility.AddCursorRect(dropArea, MouseCursor.CustomCursor);
                    onDrop(DragAndDrop.objectReferences.OfType<T>());
                }
                Event.current.Use();
            }
        }
    }

    private string GetNameSpace(string path)
    {
        var @namespace = "";
        using (var sr = new StreamReader(path, new UTF8Encoding(false)))
        {
            var text = sr.ReadToEnd();
            string pattern = @"(?<![/]{2,})[\x01-\x7f]*namespace[\s]*(?<ns>.*?)\n";

            var re = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            foreach (Match m in re.Matches(text))
            {
                @namespace = m.Groups["ns"].Value.Trim() != "" ? m.Groups["ns"].Value.Trim() + "." : "";
                break;
            }
        }
        return @namespace;
    }

    private string GetTypeName(string path)
    {
        var typeName = "";
        using (var sr = new StreamReader(path, new UTF8Encoding(false)))
        {
            var text = sr.ReadToEnd();
            string pattern = @"(?<![/]{2,}\s{0,})type[\s]*(?<type>.*?)(?![\S\(\)\=\n])";
            var re = new Regex(pattern);
            foreach (Match m in re.Matches(text))
            {
                typeName = m.Groups["type"].Value.Trim();
                break;
            }
        }
        return typeName;
    }

    private string[] GetTypes(string path)
    {
        var path2 = UniFSharp.PathUtilModule.GetAbsolutePath(Application.dataPath, path);
        var p = new Process();
        p.StartInfo.FileName = @"C:\Code\Unity\Editor\FSharp\Assets\UniFSharp\Assembly\GN_merge.exe";
        p.StartInfo.Arguments = path2 + " " + "DEBUG";
        p.StartInfo.CreateNoWindow = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardOutput = true;
        p.Start();
        p.WaitForExit();
        var outputString = p.StandardOutput.ReadToEnd();
        var types = outputString.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        return types;
    }
}