using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Profiling;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static MercatorProjection;
using mattatz.Utils;
using mattatz.Triangulation2DSystem;

class BuildingMaker : InfrstructureBehaviour
{

    //[DllImport("FillHole", CharSet = CharSet.Ansi)]
    //private static extern int FH([MarshalAs(UnmanagedType.LPStr)]string fliename);

    public Material building;

    //Vector4[] UVBlock;//vector2 UV + vector2 Block 
    Vector2[] UV;//vector2 UV + vector2 Block 
    public ControllPoint GenerateController;

    public GameObject CP1;
    public GameObject CP2;
    public GameObject CP3;
    public GameObject CP4;

    public int ControllerPointsX = 4;
    public int ControllerPointsY = 2;
    public int buildCount = 0;
    public float Talpha = 0.5f;

    public float normalP = 1.0f;

    public float CutX = 1.0f;

    Vector3[] CPointPos;
    Vector3[] NowControlPos;
    double boundx;
    double boundz;
    IEnumerator Start()
    {
        GenerateController = new ControllPoint();
        GenerateController.sphere_m = ControllerPointsX;
        GenerateController.sphere_n = ControllerPointsY;
        GenerateController.ControlCube = new GameObject[ControllerPointsX];
        while (!map.IsReady)
        {
            yield return null;
        }

        while (!map.IsReady)
        {
            yield return null;
        }
        int totalCount = 0;
        boundx = lonToX(map.bounds.MaxLon) - lonToX(map.bounds.MinLon);
        boundz = latToY(map.bounds.MaxLat) - latToY(map.bounds.MinLat);

        UnityEngine.Debug.Log(boundx);
        UnityEngine.Debug.Log(boundz);

        //for (int i = 0; i < ControllerPointsX; i++)
        //{
        //    GenerateController.ControlCube[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    GenerateController.ControlCube[i].GetComponent<Transform>().localScale = new Vector3(50, 50, 50);
        //    GenerateController.ControlCube[i].GetComponent<Transform>().position = new Vector3((float)((boundx) * i - (boundx / 2) * 3), 0, 0);
        //    GenerateController.ControlCube[i].GetComponent<MeshRenderer>().material.color = Color.blue;
        //}
        GenerateController.ControlCube[0] = CP1;
        GenerateController.ControlCube[1] = CP2;
        GenerateController.ControlCube[2] = CP3;
        GenerateController.ControlCube[3] = CP4;

        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {
            totalCount++;
        }

        GenerateController.Spheres = new GameObject[totalCount];
        //UVBlock = new Vector4[totalCount];
        UV = new Vector2[totalCount];


        //for (int i = 1; i < ControllerPointsX; i++)
        //{
        //    for (int j = 1; j < ControllerPointsY; j++)
        //    {
        //        Vector3 total = new Vector3(0, 0, 0);
        //        total += GenerateController.ControlCube[i, j].GetComponent<Transform>().position;
        //        total += GenerateController.ControlCube[i, j + 1].GetComponent<Transform>().position;
        //        total += GenerateController.ControlCube[i + 1, j].GetComponent<Transform>().position;
        //        total += GenerateController.ControlCube[i + 1, j + 1].GetComponent<Transform>().position;
        //        total /= 4.0f;

        //        CPointPos[i - 1, j - 1] = total;

        //    }
        //}
        GameObject tmep = new GameObject();
        tmep.name = "Model";

        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {

            //GenerateController.Spheres[0,0] = new GameObject();
            GenerateController.Spheres[buildCount] = new GameObject();
            GenerateController.Spheres[buildCount].transform.parent = tmep.transform;


            GenerateController.Spheres[buildCount].name = "Way_" + buildCount;
            Vector3 localOrigin = GetCentre(way);
            //Debug.Log(localOrigin);
            //Debug.Log(map.bounds.Centre);
            Vector3 BuildPos = localOrigin - map.bounds.Centre;
            GenerateController.Spheres[buildCount].transform.position = BuildPos;

            UV[buildCount].x = BuildPos.x / (float)boundx + 0.5f;
            UV[buildCount].y = BuildPos.z / (float)boundz;

            //Debug.Log(UV[buildCount]);

            MeshFilter mf = GenerateController.Spheres[buildCount].AddComponent<MeshFilter>();
            MeshRenderer mr = GenerateController.Spheres[buildCount].AddComponent<MeshRenderer>();
            //vivid color
            mr.material.shader = Shader.Find("Standard");
            Color tempC = new Color();
            tempC.r = 0.15f * (float)(buildCount % 7);
            tempC.g = 0.25f * (float)(buildCount % 5);
            tempC.b = 0.12f * (float)(buildCount % 9);
            mr.material.color = tempC;
            //mesh prepar 
            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();
            float he = way.Height;
            //if no height data
            if (way.Height < 1.0f)
            {
                he = 1.0f;
            }
            int WayNodeCount = way.NodeIDs.Count;

            //turn shape to nClock

            //find X Max point
            int MaxXID = -1;
            float MaxX = -1;
            for (int i = 0; i < WayNodeCount; i++)
            {
                OsmNode p1 = map.nodes[way.NodeIDs[i]];
                Vector3 v1 = p1 - localOrigin;
                if (MaxX < v1.x)
                {
                    MaxXID = i;
                    MaxX = v1.x;
                }
            }
            // find last one of Max Point
            int LMaxXID = (MaxXID + 1) % WayNodeCount;
            //two vertex cross
            OsmNode PNow = map.nodes[way.NodeIDs[MaxXID]];
            Vector3 VNow = PNow - localOrigin;
            OsmNode PLast = map.nodes[way.NodeIDs[LMaxXID]];
            Vector3 VLast = PLast - localOrigin;
            //Pos to Right Neg to Wrong
            float VCross = VNow.x * VLast.z - VNow.z * VLast.x;
            //Right Point sList
            Vector3[] PointList = new Vector3[WayNodeCount];
            //make ring right
            if (VCross > 0)
            {
                for (int i = 0; i < WayNodeCount; i++)
                {
                    OsmNode p1 = map.nodes[way.NodeIDs[i]];
                    PointList[i] = p1 - localOrigin;
                }
            }
            else
            {

                for (int i = 0; i < WayNodeCount; i++)
                {
                    OsmNode p1 = map.nodes[way.NodeIDs[(WayNodeCount - 1) - i]];
                    PointList[i] = p1 - localOrigin;
                }
            }

            //delaunay roof
            List<Vector2> points = new List<Vector2>();

            //add point if point too close split it
            for (int i = 0; i < WayNodeCount; i++)
            {
                int cutTime = 2;
                int nextPoint = (i + 1) % WayNodeCount;
                Vector3 nodedistence = PointList[nextPoint] - PointList[i];
                if (nodedistence.magnitude > 1f)
                {                   
                    nodedistence = nodedistence / (float)cutTime;
                    for (int j = 0; j < cutTime; j++)
                    {
                        points.Add(new Vector2(PointList[i].x + nodedistence.x * (float)j, PointList[i].z + nodedistence.z * (float)j));
                    }
                }
                else
                {
                    points.Add(new Vector2(PointList[i].x, PointList[i].z));
                }
            }
            //Delaunay
            points = Utils2D.Constrain(points, 0.8f);
            Polygon2D polygon = Polygon2D.Contour(points.ToArray());
            Triangulation2D triangulation = new Triangulation2D(polygon, 20f);
            //Delaunay Done
            Mesh roofmesh = triangulation.Build();

            // lower Point Ring
            for (int i = 0; i < WayNodeCount; i++)
            {
                vectors.Add(PointList[i]);
                normals.Add(PointList[i].normalized);
            }
            // Upper Point Ring
            for (int i = 0; i < WayNodeCount; i++)
            {
                Vector3 v3 = PointList[i] + new Vector3(0, he, 0);
                vectors.Add(v3);
                normals.Add(PointList[i].normalized);
            }

            for (int i = 0; i < WayNodeCount; i++)
            {
                // index values
                int idx1, idx2, idx3, idx4;
                idx1 = i % WayNodeCount;
                idx2 = (i + 1) % WayNodeCount;
                idx3 = (i) % WayNodeCount + WayNodeCount;
                idx4 = (i + 1) % WayNodeCount + WayNodeCount;

                // first triangle v1, v3, v2
                indices.Add(idx3);
                indices.Add(idx2);
                indices.Add(idx1);

                // second triangle v3, v4, v2
                indices.Add(idx2);
                indices.Add(idx3);
                indices.Add(idx4);

            }

            //add roof add floor 
            Vector3[] roofV = roofmesh.vertices;
            int[] roofIndex = roofmesh.triangles;
            int Pcount = vectors.Count;
            //triangles
            for (int i = 0; i < roofIndex.Length; i++)
            {
                indices.Add((int)(roofIndex[i] + Pcount));
            }
            //vertrics
            for (int i = 0; i < roofV.Length; i++)
            {
                vectors.Add(new Vector3(roofV[i].x, he, roofV[i].y));
            }



            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.triangles = indices.ToArray();
            mf.mesh.RecalculateNormals();


            GenerateController.Spheres[buildCount].AddComponent<BoxCollider>();
            GenerateController.Spheres[buildCount].AddComponent<Rigidbody>();
            GenerateController.Spheres[buildCount].GetComponent<Rigidbody>().freezeRotation = true;
            //GenerateController.Spheres[buildCount].AddComponent<coliderMix>();

            buildCount = buildCount + 1;

        }
        string SavePath = "C:/Users/user/Desktop/asd/OSM-Spline-Unity/osmunity/osm-unity/Assets";
        //save gameobj to .obj file
        List<GameObject> BuildingList = new List<GameObject>();
        for (int i = 0; i < GenerateController.Spheres.Length; i++)
        {
            BuildingList.Add(GenerateController.Spheres[i]);
        }
        //Saving
        MeshSimplify.MeshSimplify.saveasobj(BuildingList, SavePath, "0");


        //for (int i = 0; i < GenerateController.Spheres.Length; i++)
        //{
        //    string ObjFileName = SavePath + "/Way_" + i + ".obj";
        //    string EXEPath = SavePath + "/Plugins/fillhole.exe";
        //    Process.Start(EXEPath, ObjFileName);            
        //}
        //read new objfile



        //for (int i = 0; i < BuildingList.Count; i++)
        //{
        //    string MatsavePath = SavePath + "/" + BuildingList[i].name + ".mat";
        //    MatsavePath = FileUtil.GetProjectRelativePath(MatsavePath);
        //    AssetDatabase.CreateAsset(BuildingList[i].GetComponent<MeshRenderer>().sharedMaterial, MatsavePath);
        //    AssetDatabase.SaveAssets();
        //    AssetDatabase.Refresh();

        //    string ObjsavePath = SavePath + "/" + BuildingList[i].name + ".obj";
        //    AssetDatabase.Refresh();
        //    ObjsavePath = FileUtil.GetProjectRelativePath(ObjsavePath);
        //    ////Bind Mesh
        //    BuildingList[i].GetComponent<MeshFilter>().sharedMesh = (Mesh)AssetDatabase.LoadAssetAtPath(ObjsavePath, typeof(Mesh));

        //    //create model.prefab
        //    string PresavePath = SavePath + "/" + BuildingList[i].name + ".prefab";
        //    PresavePath = FileUtil.GetProjectRelativePath(PresavePath);
        //    PrefabUtility.SaveAsPrefabAsset(BuildingList[i], PresavePath);
        //    AssetDatabase.Refresh();
        //    //DestroyImmediate(outputgameobj[i]);
        //    BuildingList[i] = AssetDatabase.LoadAssetAtPath(PresavePath, typeof(GameObject)) as GameObject;
        //}


    }

    private void Update()
    {
        //for each uv
        for (int i = 0; i < buildCount; i++)
        {
            float Mx = CutX * UV[i].x;

            if (Mx > 1)
            {
                GenerateController.Spheres[i].SetActive(false);


            }
            else
            {
                GenerateController.Spheres[i].SetActive(true);


                Vector3 _p = P(new Vector2(Mx, UV[i].y));
                Vector3 _p2 = P(new Vector2(Mx + 0.01f, UV[i].y));

                _p2 = _p2 - _p;
                _p2.Normalize();
                if (GenerateController.Spheres[i] != null)
                {
                    Vector3 MidR;
                    Vector3 R = Vector3.Cross(_p2, new Vector3(0, 1, 0));
                    R.Normalize();

                    float temp = normalP;
                    if (UV[i].y < 0)
                    {
                        temp = -temp;
                    }
                    MidR = temp * R;

                    R = (float)(UV[i].y * boundz) * R;

                    GenerateController.Spheres[i].GetComponent<Transform>().position = _p + R + MidR;

                    if (UV[i].y < 0)
                    {
                        R = -R;
                    }
                    GenerateController.Spheres[i].GetComponent<Transform>().rotation = Quaternion.LookRotation(R, new Vector3(0, 1, 0));

                }

            }
        }
    }
    public float Factorial(int n)
    {
        float product = 1;
        while (n != 0)
        {
            product *= n;
            n--;
        }
        return product;
    }
    public float Combin(int n, int k)
    {
        if (n >= k)
        {
            float result = Factorial(n) / (Factorial(k) * Factorial(n - k));
            return result;
        }
        else
        {
            return 0;

        }
    }

    public float BEZ(int k, int n, float u)
    {
        float result = Combin(n, k) * Mathf.Pow(u, k) * Mathf.Pow(1 - u, n - k);
        return result;
    }

    public float GetT(float t, float alpha, Vector3 p0, Vector3 p1)
    {
        Vector3 d = p1 - p0;
        float a = d.magnitude; // Dot product
        float b = Mathf.Pow(a, alpha * 0.5f);
        return (b + t);
    }

    Vector3 CatMullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t /* between 0 and 1 */, float alpha = .5f /* between 0 and 1 */ )
    {
        float t0 = 0.0f;
        float t1 = GetT(t0, alpha, p0, p1);
        float t2 = GetT(t1, alpha, p1, p2);
        float t3 = GetT(t2, alpha, p2, p3);
        t = Mathf.Lerp(t1, t2, t);
        Vector3 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
        Vector3 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
        Vector3 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;
        Vector3 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
        Vector3 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;
        Vector3 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;
        return C;
    }

    //compute the position of the point with (u,v) image coordinate 
    public Vector3 P(Vector2 UVData)
    {
        float alpha = 0.5f;
        Vector3 X1 = CatMullRom(GenerateController.ControlCube[0].GetComponent<Transform>().position, GenerateController.ControlCube[1].GetComponent<Transform>().position, GenerateController.ControlCube[2].GetComponent<Transform>().position, GenerateController.ControlCube[3].GetComponent<Transform>().position, UVData.x, Talpha);
        return (X1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 P1 = CP1.GetComponent<Transform>().position;
        Vector3 P2 = CP2.GetComponent<Transform>().position;
        Vector3 P3 = CP3.GetComponent<Transform>().position;
        Vector3 P4 = CP4.GetComponent<Transform>().position;

        for (int i = 0; i < 99; i++)
        {
            Gizmos.DrawLine(CatMullRom(P1, P2, P3, P4, (1.0f / 100.0f) * (float)i, Talpha), CatMullRom(P1, P2, P3, P4, (1.0f / 100.0f) * (float)(i + 1.0f), Talpha));
        }
    }
}