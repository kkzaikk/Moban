using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class XUITemplate : MonoBehaviour {

#if UNITY_EDITOR
    [HideInInspector] public string GUID = "";

    [HideInInspector]
    public const char division = '@';

    [HideInInspector]
    public string headName = "";

    [HideInInspector]
    public string index = "";

	[HideInInspector] [System.NonSerialized]public List<GameObject> searPrefabs = new List<GameObject>();

    public void InitGUID(string _name,int _index)
    {
        if (GUID == "")
        {
            headName = _name;

            index = _index.ToString();

            GUID = headName + division + index;
        }
    }
#endif

}
