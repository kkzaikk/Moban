using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Text;
using System.IO;

public class NewApply
{
    /// <summary>
    /// 应用替换，传入前要实例化好
    /// 在Hierarchy
    /// </summary>
    /// <param name="find_Prefab">需要替换的根节点</param>
    /// <param name="template_Prafab">目标预设</param>
    private static GameObject ReplaceComponent(GameObject find_Prefab, GameObject template_Prafab)
    {

        GameObject result = find_Prefab;

        //查找出一个克隆对象的所有子节点有的UITemplate
        XUITemplate[] template_allComps = template_Prafab.GetComponentsInChildren<XUITemplate>(true);

        //目标模板的索引字典
        Dictionary<string, XUITemplate> template_GuidToCompDict = new Dictionary<string, XUITemplate>();

        string template_headName = "";

        //遍历
        for (int i = 0; i < template_allComps.Length; i++)
        {
            //获取组件
            XUITemplate template_comp = template_allComps[i];

            template_GuidToCompDict[template_comp.GUID] = template_comp;
            template_headName = template_comp.headName;
        }

        //
        XUITemplate[] find_allComps = find_Prefab.GetComponentsInChildren<XUITemplate>(true);

        //判断是否有相同模板嵌套
        for (int i = 0; i < find_allComps.Length; ++i)
        {
            XUITemplate find_comp = find_allComps[i];
            if (find_comp.headName == template_headName && find_comp.index == "0")
            {

                int count = 0;
                XUITemplate[] find_tmp_allComps = find_comp.GetComponentsInChildren<XUITemplate>(true);
                for (int j = 0; j < find_tmp_allComps.Length; ++j)
                {
                    if (find_tmp_allComps[j].headName == find_comp.headName && find_tmp_allComps[j].index == "0")
                    {
                        ++count;
                    }
                }

                //int count = 0;
                //XUITemplate[] find_tmp_allComps = find_Prefab.GetComponentsInChildren<XUITemplate>(true);
                //for (int j = 0; j < find_tmp_allComps.Length; ++j)
                //{
                //    if (find_tmp_allComps[j].headName == find_comp.headName && find_tmp_allComps[j].index == "0")
                //    {
                //        ++count;
                //    }
                //}

                if (count > 1)
                {
                    if (EditorUtility.DisplayDialog("提示", find_Prefab.name + "有模板嵌套\r\n(请处理再替换)\r\n(请处理再替换)\r\n(请处理再替换)", "确定"))
                    {
                        GameObject.DestroyImmediate(find_Prefab);
                        GameObject.DestroyImmediate(template_Prafab);
                        return null;
                    }
                    else
                    {
                        GameObject.DestroyImmediate(find_Prefab);
                        GameObject.DestroyImmediate(template_Prafab);
                        return null;
                    }
                }

            }
        }

        //当前模块使用了模板数量
        int useTemplateCount = 0;

        for (int i = 0; i < find_allComps.Length; ++i)
        {
            XUITemplate find_XUITemplate = find_allComps[i];
            if (find_XUITemplate.headName == template_headName && find_XUITemplate.index == "0")
            {
                ++useTemplateCount;
            }
        }

        //理论数量
        int theoryCount = useTemplateCount * template_GuidToCompDict.Count;

        //操作功能索引标志
        int operateSing = 0;

        List<GameObject> willDelObjList = new List<GameObject>();
        if (theoryCount > find_allComps.Length)
        {
            Debug.Log("模板有新节点增加");
            operateSing = 1;
        }
        else if (theoryCount < find_allComps.Length)
        {
            Debug.Log("模板有新节点删除");
            if (EditorUtility.DisplayDialog("提示", "新Prefab的节点有删除，确定要替换么?\r\n(请谨慎处理)\r\n(请谨慎处理)\r\n(请谨慎处理)", "确定", "取消") == false)
            {
                GameObject.DestroyImmediate(find_Prefab);
                GameObject.DestroyImmediate(template_Prafab);
                return null;
            }
            operateSing = -1;
        }
        else
        {
            Debug.Log("模板节点数没变化");
            operateSing = 0;
        }

        //已经复制了的对象
        Dictionary<int, GameObject> yet_copyDict = new Dictionary<int, GameObject>();

        //待替换prefab的所有子节点
        Transform[] find_allChilds = find_Prefab.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < find_allChilds.Length; ++i)
        {
            GameObject find_childObj = find_allChilds[i].gameObject;

            if (find_childObj == null) continue;

            XUITemplate find_comp = find_childObj.GetComponent<XUITemplate>();

            if (find_comp != null && template_GuidToCompDict.ContainsKey(find_comp.GUID))
            {

                int find_childId = find_childObj.GetInstanceID();

                if (yet_copyDict.ContainsKey(find_childId)) continue;

                yet_copyDict[find_childId] = find_childObj;

                //新建一个新的对象，然后粘贴属性
                GameObject new_Instance = new GameObject();
                new_Instance.name = find_childObj.name;
                if (find_childObj.transform.parent != null)
                {
                    new_Instance.transform.SetParent(find_childObj.transform.parent);
                }
                else
                {
                    result = new_Instance;
                }
                new_Instance.transform.localPosition = find_childObj.transform.localPosition;
                new_Instance.transform.localRotation = find_childObj.transform.localRotation;
                new_Instance.transform.localScale = find_childObj.transform.localScale;

                ////////////////////
                new_Instance.transform.SetSiblingIndex(find_childObj.transform.GetSiblingIndex());

                //复制子节点
                int count = find_childObj.transform.childCount;
                List<Transform> find_childList = new List<Transform>();
                for (int j = 0; j < count; ++j)
                {
                    Transform child = find_childObj.transform.GetChild(j);
                    find_childList.Add(child);
                }
                foreach (var v in find_childList)
                {
                    v.SetParent(new_Instance.transform);
                }

                //原先的对象组件数组
                Component[] find_copiedComponents = find_childObj.GetComponents<Component>();

                //复制对象所有组件，不包括子节点
                Component[] template_copiedComponents = template_GuidToCompDict[find_comp.GUID].GetComponents<Component>();

                //开始复制
                foreach (var template_copiedComponent in template_copiedComponents)
                {

                    string template_typeString = template_copiedComponent.GetType().ToString();
                    if (retainDict.ContainsKey(template_typeString))
                    {
                        //配置表填写要保留的属性
                        List<string> propertyList = retainDict[template_typeString];

                        //查找该Object的所有属性，是否满足条件需要保留
                        Type template_type = template_copiedComponent.GetType();
                        System.Reflection.PropertyInfo[] template_propertyInfos = template_type.GetProperties();
                        for (int k = 0; k < template_propertyInfos.Length; k++)
                        {
                            string template_name = template_propertyInfos[k].Name;

                            if (propertyList.Contains(template_name))
                            {

                                //从原先的组件找出要保留的属性，然后赋值给新创建的组件
                                foreach (var find_copiedComponent in find_copiedComponents)
                                {
                                    if (find_copiedComponent.GetType() == template_type)
                                    {
                                        Type find_type = find_copiedComponent.GetType();
                                        System.Reflection.PropertyInfo[] find_propertyInfos = find_type.GetProperties();

                                        for (int m = 0; m < find_propertyInfos.Length; ++m)
                                        {
                                            if (find_propertyInfos[m].Name == template_name)
                                            {
                                                object before_v = find_propertyInfos[m].GetValue(find_copiedComponent, null);

                                                template_propertyInfos[k].SetValue(template_copiedComponent, before_v, null);

                                                break;
                                            }
                                        }

                                    }
                                }

                            }

                        }

                    }

                    //复制组件
                    UnityEditorInternal.ComponentUtility.CopyComponent(template_copiedComponent);
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(new_Instance);
                }

                //增加新节点////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (operateSing == 1)
                {
                    //已经添加的模板组件，存起来，方便找出是否目标prefab是否有新节点增加或者删除
                    Dictionary<string, XUITemplate> find_yetAddDict = new Dictionary<string, XUITemplate>();

                    foreach (var v in find_childList)
                    {
                        //加入已经有的组件
                        XUITemplate tmp = v.GetComponent<XUITemplate>();
                        if (tmp != null && tmp.headName == find_comp.headName)
                        {
                            find_yetAddDict[tmp.GUID] = tmp;
                        }
                    }

                    //如果模板节点有增加，则加入新增的模板节点
                    XUITemplate template_tmp = template_GuidToCompDict[find_comp.GUID];
                    for (int j = 0; j < template_tmp.transform.childCount; ++j)
                    {
                        Transform child = template_tmp.transform.GetChild(j);

                        XUITemplate tmp = child.GetComponent<XUITemplate>();

                        //如果没有，则为目标模板新添加的节点，则实例化加入到新节点
                        if (!find_yetAddDict.ContainsKey(tmp.GUID))
                        {
                            GameObject obj = GameObject.Instantiate(child.gameObject);
                            obj.name = child.name;
                            obj.transform.SetParent(new_Instance.transform);
                        }

                    }

                }

                //删除节点////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                if (operateSing == -1)
                {
                    //已经添加的模板组件，存起来，方便找出是否目标prefab是否有新节点增加或者删除
                    Dictionary<string, XUITemplate> find_yetAddDict = new Dictionary<string, XUITemplate>();
                    foreach (var v in find_childList)
                    {
                        //加入已经有的组件
                        XUITemplate tmp = v.GetComponent<XUITemplate>();
                        if (tmp != null && tmp.headName == find_comp.headName)
                        {
                            find_yetAddDict[tmp.GUID] = tmp;
                        }
                    }

                    //
                    Dictionary<string, XUITemplate> template_yetAddDict = new Dictionary<string, XUITemplate>();
                    int template_childCount = template_GuidToCompDict[find_comp.GUID].transform.childCount;
                    for (int j = 0; j < template_childCount; ++j)
                    {
                        Transform child = template_GuidToCompDict[find_comp.GUID].transform.GetChild(j);
                        XUITemplate tmp = child.GetComponent<XUITemplate>();
                        if (tmp != null && tmp.headName == find_comp.headName)
                        {
                            template_yetAddDict[tmp.GUID] = tmp;
                        }
                    }

                    foreach (var d in find_yetAddDict)
                    {
                        //如果没有，则为目标模板已经删除的节点，则删除
                        if (!template_yetAddDict.ContainsKey(d.Key))
                        {
                            willDelObjList.Add(d.Value.gameObject);
                        }
                    }

                }

            }

        }

        //删除旧的
        foreach (var d in yet_copyDict)
        {
            GameObject.DestroyImmediate(d.Value);
        }
        yet_copyDict.Clear();

        foreach (var v in willDelObjList)
        {
            GameObject.DestroyImmediate(v);
        }
        willDelObjList.Clear();

        return result;

    }

    private static Dictionary<string, List<string>> retainDict = new Dictionary<string, List<string>>();

    //读取配置文件
    public static void ReadRetainConfig()
    {
        retainDict.Clear();

        DirectoryInfo TheFolder = new DirectoryInfo(UITemplatePath.GetRetainProperyPath());
        //遍历文件
        foreach (FileInfo nextFile in TheFolder.GetFiles())
        {
            if (nextFile.FullName.EndsWith(".txt"))
            {
                string path = nextFile.FullName;
                StreamReader sr = new StreamReader(path, Encoding.Default);
                string line;

                string typeString = "";
                List<string> propertyList = new List<string>();

                int index = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (index == 0)
                    {
                        typeString = line;
                    }
                    else
                    {
                        propertyList.Add(line);
                    }

                    ++index;

                }

                retainDict[typeString] = propertyList;

                sr.Close();
                sr.Dispose();
            }
        }

    }

    //不支持嵌套
    public static void ChangeDepth(GameObject find_Prefab, GameObject template_Prafab)
    {
        //查找出一个克隆对象的所有子节点有的UITemplate
        XUITemplate[] template_templateComps = template_Prafab.GetComponentsInChildren<XUITemplate>(true);
        //目标模板的索引字典
        Dictionary<string, XUITemplate> template_Guid2TemplateDict = new Dictionary<string, XUITemplate>();
        //
        string template_headName = "";
        //遍历
        for (int i = 0; i < template_templateComps.Length; i++)
        {
            //获取组件
            XUITemplate tmp = template_templateComps[i];
            template_Guid2TemplateDict[tmp.GUID] = tmp;
            template_headName = tmp.headName;
        }

        XUITemplate[] find_allComps = find_Prefab.GetComponentsInChildren<XUITemplate>(true);

        //设置父子节点关系
        for (int i = 0; i < find_allComps.Length; ++i)
        {
            //获取组件
            XUITemplate find_tmp = find_allComps[i];

            if (find_tmp.headName == template_headName && find_tmp.index == "0")
            {
                XUITemplate[] find_tmps = find_tmp.GetComponentsInChildren<XUITemplate>(true);
                Dictionary<string, XUITemplate> find_Guid2TmpDict = new Dictionary<string, XUITemplate>();
                for (int j = 0; j < find_tmps.Length; ++j)
                {
                    find_Guid2TmpDict[find_tmps[j].GUID] = find_tmps[j];
                }

                for (int j = 0; j < find_tmps.Length; ++j)
                {
                    if (find_tmps[j].headName == template_headName && find_tmps[j].index != "0")
                    {
                        XUITemplate find_parent = find_tmps[j].transform.parent.GetComponent<XUITemplate>();

                        XUITemplate template_parent = template_Guid2TemplateDict[find_tmps[j].GUID].transform.parent.GetComponent<XUITemplate>();

                        //父子关系有变化，要重新设置
                        if (template_parent.GUID != find_parent.GUID)
                        {
                            find_tmps[j].transform.SetParent(find_Guid2TmpDict[template_parent.GUID].transform);
                        }
                    }
                }
                break;
            }
        }

        //设置深度
        for (int i = 0; i < find_allComps.Length; ++i)
        {
            //获取组件
            XUITemplate find_comp = find_allComps[i];

            if (find_comp.headName == template_headName && find_comp.index == "0")
            {
                int signIndex = find_comp.transform.GetSiblingIndex();//5

                XUITemplate[] find_tmps = find_comp.GetComponentsInChildren<XUITemplate>(true);

                //重新计算，深度
                for (int j = 0; j < find_tmps.Length; ++j)
                {
                    if (find_tmps[j].headName == template_headName && find_tmps[j].index != "0")
                    {
                        int index = template_Guid2TemplateDict[find_tmps[j].GUID].transform.GetSiblingIndex();//1

                        find_tmps[j].transform.SetSiblingIndex(index);
                    }
                }

                break;
            }
        }

    }

    public static void ApplyPrefab(GameObject find_Obj, GameObject template_Obj)
    {
        //在Hierarchy实例化找到的替换对象
        GameObject find_Prefab = GameObject.Instantiate(find_Obj);
        find_Prefab.name = find_Obj.name;

        //在Hierarchy实例化模板对象
        GameObject template_Prafab = GameObject.Instantiate(template_Obj);
        template_Prafab.name = template_Obj.name;

        //替换组件到找到的对象
        GameObject new_Obj = ReplaceComponent(find_Prefab, template_Prafab);

        if (new_Obj == null) return;

        //替换深度
        ChangeDepth(new_Obj, template_Prafab);

        //路径名字
        string pathName = AssetDatabase.GetAssetPath(find_Obj);

        //删除旧的prefab
        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(find_Prefab)));

        //创建新的
        PrefabUtility.CreatePrefab(pathName, new_Obj);

        //在Hierarchy删除对象
        GameObject.DestroyImmediate(new_Obj);
        GameObject.DestroyImmediate(template_Prafab);

        //保存
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.SaveOpenScenes();
    }

}
