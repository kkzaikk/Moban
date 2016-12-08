using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class UITemplateEditorWindow : EditorWindow {

    public enum TemplateWindowType
    {
        type0,
        type1
    }

    static private TemplateWindowType _type = TemplateWindowType.type0;

    private string _defaultPath = UITemplatePath.GetTemplatePrefabPath();

    static private GameObject _source;

    [MenuItem("UI模板/一键生成模板")]
    static public void AddWindwo()
    {
        AddWindow(TemplateWindowType.type0);
    }


    static public void AddWindow(TemplateWindowType type = TemplateWindowType.type0, GameObject tmpObj = null)
    {
        _type = type;

        _source = tmpObj;

        //创建窗口
        Rect wr = new Rect(0, 0, 500, 500);
        UITemplateEditorWindow window = (UITemplateEditorWindow)EditorWindow.GetWindowWithRect(typeof(UITemplateEditorWindow), wr, true, "模板生成窗口");
        window.Show();
    }


    //绘制窗口时调用
    void OnGUI()
    {
        EditorGUILayout.Space();

        switch (_type)
        {
            case TemplateWindowType.type0:
                {
                    EditorGUILayout.LabelField("方式1：  （路径下的prefab全部重新生成模板）");

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("选择路径"))
                    {
                        _defaultPath = EditorUtility.OpenFolderPanel("选择路径", "", "");
                    }

                    //输入框控件
                    GUILayout.TextField(_defaultPath);

                    GUILayout.EndHorizontal();


                    EditorGUILayout.Space();
                    EditorGUILayout.Space();


                    if (GUILayout.Button("一键创建"))
                    {
                        //找出对应目录下的所有prefab
                        DirectoryInfo directiory = new DirectoryInfo(_defaultPath);
                        FileInfo[] infos = directiory.GetFiles("*.prefab", SearchOption.AllDirectories);

                        //遍历
                        for (int i = 0; i < infos.Length; i++)
                        {
                            FileInfo file = infos[i];

                            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file.FullName.Substring(file.FullName.IndexOf("Assets")));

                            string createPath = AssetDatabase.GetAssetPath(prefab);

                            UITemplateInspector.CreatePrefab(prefab, createPath);
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();


                    EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");


                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("方式2：  （拖入要创建模板的GameObject）");
                    _source = (GameObject)EditorGUILayout.ObjectField(_source, typeof(GameObject), false);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("创建"))
                    {
                        if (_source != null)
                        {
                            string createPath = AssetDatabase.GetAssetPath(_source);
                            UITemplateInspector.CreatePrefab(_source, createPath);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示！", "请拖入要创建模板对应的对象！", "确定");
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();


                    EditorGUILayout.LabelField("@@@@@@@@@@@@@@@@@@@@删除模板组件@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");


                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("选择路径"))
                    {
                        _defaultPath = EditorUtility.OpenFolderPanel("选择路径", "", "");
                    }

                    //输入框控件
                    GUILayout.TextField(_defaultPath);

                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("一键删除模板组件"))
                    {
                        //找出对应目录下的所有prefab
                        DirectoryInfo directiory = new DirectoryInfo(_defaultPath);
                        FileInfo[] infos = directiory.GetFiles("*.prefab", SearchOption.AllDirectories);

                        //遍历
                        for (int i = 0; i < infos.Length; i++)
                        {
                            FileInfo file = infos[i];

                            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file.FullName.Substring(file.FullName.IndexOf("Assets")));

                            string createPath = AssetDatabase.GetAssetPath(prefab);

                            UITemplateInspector.RemoveTemplate(prefab, createPath);
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("--------------------------------------------------------------------------------------------------------------------");


                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("拖入要删除模板的GameObject");
                    _source = (GameObject)EditorGUILayout.ObjectField(_source, typeof(GameObject), false);

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("删除"))
                    {
                        if (_source != null)
                        {
                            string createPath = AssetDatabase.GetAssetPath(_source);
                            UITemplateInspector.RemoveTemplate(_source, createPath);
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("提示！", "请拖入要删除模板对应的对象！", "确定");
                        }
                    }
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();



                    break;
                }
            case TemplateWindowType.type1:
                {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("选择路径"))
                    {
                        _defaultPath = EditorUtility.OpenFolderPanel("选择路径", "", "");
                    }

                    GUILayout.TextField(_defaultPath);

                    GUILayout.EndHorizontal();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                    if (GUILayout.Button("创建"))
                    {
                        _defaultPath = _defaultPath.Substring(_defaultPath.IndexOf("Assets"));

                        UITemplateInspector.CreatePrefab(_source, _defaultPath + "/" + _source.name + ".prefab");

                        this.Close();

                        GameObject.DestroyImmediate(_source);
                    }

                    break;
                }
        }

    }

}
