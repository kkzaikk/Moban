using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 模板需要的路径
/// </summary>
public class UITemplatePath
{
    //模板路径
    private static string _templatePrefabPath = "";

    //Prefab存放的路径
    private static List<string> _uiPrefabPathList = new List<string>();

    //保留属性配置表路径
    private static string _retainProperyPath = "";


    public static string GetRetainProperyPath()
    {
        Init();

        return _retainProperyPath;
    }

    public static List<string> GetUIPrefabPathList()
    {
        Init();

        return _uiPrefabPathList;
    }

    public static string GetTemplatePrefabPath()
    {
        Init();

        return _templatePrefabPath;
    }


    //配置表路径
    private static string _defaultPath  = Application.dataPath + "/Editor/TemplateEditor/Retainproperty/Path.txt";

    /// <summary>
    /// 初始化数据
    /// </summary>
    public static void Init()
    {

        StreamReader sr = new StreamReader(_defaultPath, Encoding.Default);
        string line;

        List<string> propertyList = new List<string>();

        while ((line = sr.ReadLine()) != null)
        {

            if (line != "" && !line.StartsWith("#"))
            {
                string[] paths = line.Split(':');

                switch (paths[0])
                {
                    case "path0":
                        {
                            _templatePrefabPath = "Assets/" + paths[1];
                            break;
                        }

                    case "path1":
                        {

                            _uiPrefabPathList.Clear();
                            _uiPrefabPathList.AddRange(paths[1].Split(';'));

                            break;
                        }
                    case "path2":
                        {
                            _retainProperyPath = "Assets/" + paths[1];
                            break;
                        }
                    default:
                        break;
                }

            }
        }

        sr.Close();
        sr.Dispose();
    }


}
