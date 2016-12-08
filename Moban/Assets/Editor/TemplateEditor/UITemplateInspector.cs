using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(XUITemplate))]
public class UITemplateInspector : Editor
{
    [MenuItem("GameObject/UITemplate/Creat To Prefab", false, 11)]
    static void CreatToPrefab(MenuCommand menuCommand)
    {
        if (menuCommand.context != null)
        {
			CreatDirectory();

            GameObject selectGameObject = menuCommand.context as GameObject;

            //判断有没有源头对象
            if (IsTemplatePrefab_Only_InHierarchy(selectGameObject))
            {
                UITemplateEditorWindow.AddWindow(UITemplateEditorWindow.TemplateWindowType.type1, selectGameObject);
            }
            else
            {
                string createPath = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(selectGameObject));

                CreatePrefab(selectGameObject, createPath);
            }
        }
        else
        {
            EditorUtility.DisplayDialog("错误！", "请选择一个GameObject", "OK");
        }
    }

    private XUITemplate uiTemplate;


    void OnEnable()
    {
        uiTemplate = (XUITemplate)target;

        if (IsTemplatePrefabInInProjectView(uiTemplate.gameObject))
        {
            ShowHierarchy();
        }
		CreatDirectory();
    }

    public override void OnInspectorGUI()
    {

 	    base.OnInspectorGUI();
	    bool isPrefabInProjectView = IsTemplatePrefabInInProjectView(uiTemplate.gameObject);
        EditorGUILayout.LabelField("GUID:" + uiTemplate.GUID);
	
        GUILayout.BeginHorizontal();

		if (GUILayout.Button("Select"))
        {
			DirectoryInfo directiory = CreatDirectory();

            FileInfo[] infos = directiory.GetFiles("*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < infos.Length; i++)
            {
                FileInfo file = infos[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file.FullName.Substring(file.FullName.IndexOf("Assets")));
                if(prefab.GetComponent<XUITemplate>().GUID == uiTemplate.GUID)
                {
                    EditorGUIUtility.PingObject(prefab);
                    return;
                }
            }
        }
        
		if (isPrefabInProjectView)
        {

	        if (GUILayout.Button("Search"))
	        {
	            TrySearchPrefab(uiTemplate.headName, out uiTemplate.searPrefabs);
				return;
	        }

	        if (GUILayout.Button("Apply"))
	        {
                //读取属性保留的配置文件
                NewApply.ReadRetainConfig();

                //查找，然后替换
                List<GameObject> references;
                if (TrySearchPrefab(uiTemplate.headName, out references))//查找目录下所有的引用有typeTemplateHead标示的prefab
                {
                    foreach (var v in references)
                    {
                        
                        NewApply.ApplyPrefab(v, uiTemplate.gameObject);

                    }
                }

                EditorUtility.DisplayDialog("完成", "操作完成", "确定");

                return;
	        }

            //先暂时屏蔽删除
	   //     if (GUILayout.Button("Delete"))
	   //     {
	   //         if (IsTemplatePrefabInHierarchy(uiTemplate.gameObject))
	   //         {
	   //             DeletePrefab(GetPrefabPath(uiTemplate.gameObject));
	   //         }
	   //         else
	   //         {
	   //             DeletePrefab(AssetDatabase.GetAssetPath(uiTemplate.gameObject));
	   //         }
				//return;
	   //     }
		}
        GUILayout.EndHorizontal();

        if (isPrefabInProjectView)
        {
            if (uiTemplate != null && uiTemplate.searPrefabs.Count > 0)
            {
                EditorGUILayout.LabelField("Prefab :" + uiTemplate.name);

                foreach (GameObject p in uiTemplate.searPrefabs)
                {
                    EditorGUILayout.Space();
                    if (GUILayout.Button(AssetDatabase.GetAssetPath(p)))
                    {
                        EditorGUIUtility.PingObject(p);
                    }
                }
            }
        }

    }

    static private bool TrySearchPrefab(string headName,out List<GameObject> searchList)
    {
        List<GameObject> prefabs = new List<GameObject>();
        bool trySearch = false;
        foreach(string forder in UITemplatePath.GetUIPrefabPathList())
        {
            //找出对应目录下的所有prefab
            DirectoryInfo directiory = new DirectoryInfo(Application.dataPath + "/" + forder.Replace("Assets/", ""));
            FileInfo[] infos = directiory.GetFiles("*.prefab", SearchOption.AllDirectories);

            //遍历
            for (int i = 0; i < infos.Length; i++)
            {
                FileInfo file = infos[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(file.FullName.Substring(file.FullName.IndexOf("Assets")));

                Debug.Log("开始查找" + prefab.name);
                if (prefab.GetComponentsInChildren<XUITemplate>(true).Length > 0)
                {
                    GameObject go = Instantiate<GameObject>(prefab);
                    XUITemplate[] templates = go.GetComponentsInChildren<XUITemplate>(true);
                    foreach (XUITemplate template in templates)
                    {
                        //查找出ID一样的，加入队列
                        if (template.headName == headName && template.index == "0" && !prefabs.Contains(prefab))
                        {
                            prefabs.Add(prefab);
                        }
                    }
                    GameObject.DestroyImmediate(go);
                }
            }
        }

        searchList = prefabs;
        return !trySearch;
    }

    static private void DeletePrefab(string path)
    {
        if (EditorUtility.DisplayDialog("注意！", "是否进行递归查找批量删除模板？", "ok", "cancel"))
        {
            Debug.Log("DeletePrefab : " + path);
            GameObject deletePrefab =  AssetDatabase.LoadAssetAtPath<GameObject>(path);
            XUITemplate template = deletePrefab.GetComponent<XUITemplate>();
            if (template != null)
            {
                List<GameObject> references;
                if (TrySearchPrefab(template.headName, out references))
                {
                    
                    foreach (GameObject reference in references)
                    {
                        GameObject go = PrefabUtility.InstantiatePrefab(reference) as GameObject;
                        XUITemplate[] instanceTemplates = go.GetComponentsInChildren<XUITemplate>(true);
                        for (int i = 0; i < instanceTemplates.Length; i++)
                        {
                            XUITemplate instance = instanceTemplates[i];
                            if (instance.GUID == template.GUID)
                            {
                                DestroyImmediate(instance.gameObject);
                            }
                        }
                        PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go), ReplacePrefabOptions.ConnectToPrefab);
                        DestroyImmediate(go);
                    }
                }
            }
            AssetDatabase.DeleteAsset(path);
            ClearHierarchy();
            Refresh();
        }
    }


    public static void RemoveTemplate(GameObject prefab, string createPath)
    {
        //有没有源对象
        if (AssetDatabase.LoadAssetAtPath<GameObject>(createPath) == null)//不存在源对象
        {
            XUITemplate[] temps = prefab.GetComponentsInChildren<XUITemplate>(true);
            for (int i = 0; i < temps.Length; i++)
            {
                DestroyImmediate(temps[i]);
            }

            //刷新
            Refresh();
        }
        else
        {
            //步骤
            //1.确保prefab是检视面板的一个对象，如果没有，则创建
            //2.删除源对象
            //3.重新生成
            //4.删除检视面板的对象
            //5.刷新

            //在Hierarchy的GameObject(右键菜单)
            bool isInHierarchy = false;
            if (AssetDatabase.GetAssetPath(prefab) == "")
            {
                isInHierarchy = true;
            }

            //如果不在检视面板，则实例化一个对象，根据这个对象创建prefab
            if (isInHierarchy == false)
            {
                //实例化一个对象
                GameObject clone = GameObject.Instantiate(prefab);
                clone.name = prefab.name;
                prefab = clone;
            }

            XUITemplate[] temps = prefab.GetComponentsInChildren<XUITemplate>(true);
            for (int i = 0; i < temps.Length; i++)
            {
                DestroyImmediate(temps[i]);
            }

            //删除
            AssetDatabase.DeleteAsset(createPath);

            //重新生成
            PrefabUtility.CreatePrefab(createPath, prefab);

            DestroyImmediate(prefab);
            Refresh();
        }
    }


    public static void CreatePrefab(GameObject prefab, string createPath)
    {
        //有没有源对象
        if (AssetDatabase.LoadAssetAtPath<GameObject>(createPath) == null)//不存在源对象
        {
            Debug.Log("CreatPrefab : " + createPath);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //方法A步骤
            //1.删除所有的模板类
            //2.给每个对象加入模板类
            //{
            //    UITemplate[] temps = prefab.GetComponentsInChildren<UITemplate>(true);
            //    for (int i = 0; i < temps.Length; i++)
            //    {
            //        DestroyImmediate(temps[i]);
            //    }

            //    Transform[] childs = prefab.GetComponentsInChildren<Transform>(true);
            //    string headName = prefab.name;
            //    //为每个对象都加入模板类
            //    for (int i = 0; i < childs.Length; ++i)
            //    {
            //        childs[i].gameObject.AddComponent<UITemplate>().InitGUID(headName, i);
            //    }
            //}


            //方法B步骤
            //不删除以前的XUITemplate，然后依次增加
            List<int> tmpsIndexList = new List<int>();
            XUITemplate[] temps = prefab.GetComponentsInChildren<XUITemplate>(true);
            for (int i = 0; i < temps.Length; ++i)
            {
                tmpsIndexList.Add(int.Parse(temps[i].index));
            }
            tmpsIndexList.Sort();

            Transform[] childs = prefab.GetComponentsInChildren<Transform>(true);
            string headName = prefab.name;

            if (temps.Length <= 0)
            {
                //为每个没有UITemplate的对象都加入模板类
                for (int i = 0; i < childs.Length; ++i)
                {
                    childs[i].gameObject.AddComponent<XUITemplate>().InitGUID(headName, i);
                }
            }
            else
            {
                int index = 1;
                int maxIndex = tmpsIndexList[temps.Length - 1];
                //为每个没有UITemplate的对象都加入模板类
                for (int i = 0; i < childs.Length; ++i)
                {
                    if (childs[i].gameObject.GetComponent<XUITemplate>() == null)
                    {
                        childs[i].gameObject.AddComponent<XUITemplate>().InitGUID(headName, maxIndex + index);
                        ++index;
                    }
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //创建prefab
            PrefabUtility.CreatePrefab(createPath, prefab);

            //刷新
            Refresh();

        }
        else//存在源对象
        {
            //步骤
            //1.确保prefab是检视面板的一个对象，如果没有，则创建
            //2.删除源对象
            //3.重新生成prefab
            //4.删除检视面板的对象
            //5.刷新

            //在Hierarchy的GameObject(右键菜单)
            bool isInHierarchy = false;
            if (AssetDatabase.GetAssetPath(prefab) == "")
            {
                isInHierarchy = true;
            }

            //如果不在检视面板，则实例化一个对象，根据这个对象创建prefab
            if (isInHierarchy == false)
            {
                //实例化一个对象
                GameObject clone = GameObject.Instantiate(prefab);
                clone.name = prefab.name;
                prefab = clone;
            }

            //删除
            AssetDatabase.DeleteAsset(createPath);

            //重新生成
            CreatePrefab(prefab, createPath);
            DestroyImmediate(prefab);
            Refresh();

        }
        
    }



    static private void Refresh()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.SaveOpenScenes();
    }


    static private void ClearHierarchy()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform t = canvas.transform.GetChild(i);
                if (t.GetComponent<XUITemplate>() != null)
                {
                    GameObject.DestroyImmediate(t.gameObject);
                }
            }
        }
    }

    private void ShowHierarchy()
    {
       
        if (!IsTemplatePrefab_Only_InHierarchy(uiTemplate.gameObject))
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
			if (canvas != null)
            {
				if((canvas.transform.childCount == 0) ||
				 (canvas.transform.childCount == 1 && canvas.transform.GetChild((0)).GetComponent<XUITemplate>() != null)){
					ClearHierarchy();
	                GameObject go = PrefabUtility.InstantiatePrefab(uiTemplate.gameObject) as GameObject;
	                go.name = uiTemplate.gameObject.name;

					GameObjectUtility.SetParentAndAlign(go, canvas.gameObject);
	                EditorGUIUtility.PingObject(go);
				}
             
            }

        }


    }

    //是否是在检视面板而且没有源头对象
    static private bool IsTemplatePrefab_Only_InHierarchy(GameObject go)
    {
        return (PrefabUtility.GetPrefabParent(go) == null);
    }

    //对象是否在Project面板
    static private bool IsTemplatePrefabInInProjectView(GameObject go)
    {
        string path = AssetDatabase.GetAssetPath(go);
        if (!string.IsNullOrEmpty(path))
            return (path.Contains(UITemplatePath.GetTemplatePrefabPath()));
        else
            return false;
    }

    //创建目录信息
	static private DirectoryInfo CreatDirectory()
	{
        DirectoryInfo directiory = new DirectoryInfo(Application.dataPath + "/" + UITemplatePath.GetTemplatePrefabPath().Replace("Assets/", ""));
		if(!directiory.Exists)
		{
			directiory.Create();
			Refresh();
		}
		return directiory;
   	}

    //获取prefab源头路径
    static private string GetPrefabPath(GameObject prefab)
    {
        Object prefabObj = PrefabUtility.GetPrefabParent(prefab);
        if (prefabObj != null)
        {
            return AssetDatabase.GetAssetPath(prefabObj);
        }
        return null;
    }

}
