using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Profiling;
//using ObjExporter;

public class Toolmenu : EditorWindow
{    
    string MeshSavePath = "";   
    bool Name;    
    //bool FindChildEnabled = false;
    Dictionary<GameObject, bool> checkUse = new Dictionary<GameObject, bool>();
    List<GameObject> beSelect = new List<GameObject>();

    bool ReBuildDone = true;
    bool UseOwnUV2 = false;
    //Texture Type    

    GameObject[] FirstSelect = new GameObject[0];
    //select 
    GameObject[] BeSelect = new GameObject[0];
    
    Vector2 ScrollPos;

    //select by tag
    string TagName;
    string ObjName;

    //has not model gameobject
    bool HasNomodel = false;
    bool HasNotEqualModel = false;

    //find child hash
    private List<GameObject> AllChilds(GameObject root)
    {
        List<GameObject> result = new List<GameObject>();
        if (root.transform.childCount > 0)
        {
            foreach (Transform VARIABLE in root.transform)
            {
                Searcher(result, VARIABLE.gameObject);
            }
        }
        return result;
    }
    private void Searcher(List<GameObject> list, GameObject root)
    {
        list.Add(root);
        if (root.transform.childCount > 0)
        {
            foreach (Transform VARIABLE in root.transform)
            {
                Searcher(list, VARIABLE.gameObject);
            }
        }
    }


    [MenuItem("OSMBuilding/Menu")]
    public static void ModelRebuild_Open()
    {
        EditorWindow.GetWindow(typeof(Toolmenu), false, "OSMBuilding Menu", true);
    }
    void OnGUI()
    {
        //Init select
        FirstSelect = new GameObject[0];
        //Clear selectItem before
        checkUse.Clear();
        beSelect.Clear();
        //Assign select model may have useless gameobj
        FirstSelect = Selection.gameObjects;
        //count gameobj with mesh
        int meshobjectnumber = 0;
        HasNotEqualModel = false;
        for (int i = 0; i < FirstSelect.Length; i++)
        {
            //if some gameibject not have mesh or repeat select
            List<GameObject> allChilds = AllChilds(FirstSelect[i]);
            for (int j = 0; j < allChilds.Count; j++)
            {
                if (!checkUse.ContainsKey(allChilds[j]))
                {
                    checkUse[allChilds[j]] = true;
                    beSelect.Add(allChilds[j]);
                    meshobjectnumber++;
                }
                else
                {
                    HasNotEqualModel = true;
                }
            }
            if (FirstSelect[i].GetComponent<MeshFilter>() == null)
            {
                HasNotEqualModel = true;
            }
        }
        HasNomodel = false;
        //if dont have any gameobj has mesh
        if (meshobjectnumber == 0)
        {
            HasNomodel = true;
        }

        ObjName = "";
        if (beSelect != null)
        {
            if (beSelect.Count > 0)
            {
                ObjName = beSelect[0].name;
            }
            for (int i = 1; i < beSelect.Count; i++)
            {
                ObjName = ObjName + "\n" + beSelect[i].name;
            }
        }
        GUILayout.Label("Find GameObject", EditorStyles.boldLabel);
        if (beSelect.Count > 15)
        {
            ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUILayout.Height(200));
        }
        ObjName = EditorGUILayout.TextArea(ObjName);
        if (beSelect.Count > 15)
        {
            EditorGUILayout.EndScrollView();
        }


        MeshSavePath = EditorGUILayout.TextField("Save Mesh at", MeshSavePath);
        if (GUILayout.Button("Brower Mesh Path"))
        {
            MeshSavePath = EditorUtility.OpenFolderPanel("Save Mesh at", "", "");
        }
        if (GUILayout.Button("ReBuild"))
        {
            if (BeSelect != null)
            {
                MeshSavePath = MeshSavePath.Replace('\\', '/');

                MeshSimplify.MeshSimplify.saveasobj(beSelect, MeshSavePath, "0");

                //ReBuildDone = ImpostorTool.RebuilModel_Indenpent_one_Mat(beSelect.ToArray(), Name, NewTextureSize, NewMeshName, NewMaterialName, ModelSavePath, MaterialSavePath, MeshSavePath, TextureSavePath, UseOwnUV2, ImposterRebuildType_MODE, metallicINRGBA, SmoothnessINRGBA, OcclusionINRGBA);
                for (int i = 0; i < beSelect.Count; i++)
                {
                    string savePath = MeshSavePath + "/" + beSelect[i].name + ".mat";
                    savePath = FileUtil.GetProjectRelativePath(savePath);
                    AssetDatabase.CreateAsset(beSelect[i].GetComponent<MeshRenderer>().sharedMaterial, savePath);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    savePath = MeshSavePath + "/" + beSelect[i].name + ".obj";
                    AssetDatabase.Refresh();
                    savePath = FileUtil.GetProjectRelativePath(savePath);
                    ////Bind Mesh
                    beSelect[i].GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(savePath, typeof(Mesh));

                    //create model.prefab
                    savePath = MeshSavePath + "/" + beSelect[i].name + ".prefab";
                    savePath = FileUtil.GetProjectRelativePath(savePath);
                    PrefabUtility.SaveAsPrefabAsset(beSelect[i], savePath);
                    AssetDatabase.Refresh();
                    //DestroyImmediate(outputgameobj[i]);
                    beSelect[i] = AssetDatabase.LoadAssetAtPath(savePath, typeof(GameObject)) as GameObject;
                }
            }
            else
            {
                Debug.Log("Tool Not Ready");
            }
        }

    }
}
