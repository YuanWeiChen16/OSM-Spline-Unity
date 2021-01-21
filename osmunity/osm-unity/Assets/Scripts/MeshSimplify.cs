using UnityEngine;
using ObjExporter;
using System.Diagnostics;
using System.Collections;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
/*
使用方式
using MeshSimplify;

函式呼叫方式
MeshSimplify.Meshsimplify.任意函式名稱
*/

namespace MeshSimplify
{
    public class MeshSimplify
    {
      
        public static void saveasobj(List<GameObject> obj, string path, string meshname) //存檔 obj 為要存檔的那個GameObject   || path : 為要存檔的路徑 || MESH NAME 為要命名的名字
        {
            string tmppath = path;
            List<MeshFilter> combine = new List<MeshFilter>();
            Mesh tmpmesh = new Mesh();
            if (meshname == null)
            {
                meshname = "default";
            }

            for (int i = 0; i < obj.Count; i++)
            {
                path = tmppath;
                path = path + "/" + obj[i].name + ".obj";                                 // 副檔名需為.obj
                ObjExporter.ObjExporter.MeshToFile(obj[i].GetComponent<MeshFilter>(), path);
            }
        }
        
    }

}
