using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MercatorProjection;

class BuildingMaker : InfrstructureBehaviour
{
    public Material building;

    Vector4[] UVBlock;//vector2 UV + vector2 Block 
    Vector2[] UV;//vector2 UV + vector2 Block 
    public ControllPoint GenerateController;
    public int ControllerPoints = 4; //cardinal 6
    public int buildCount = 0;
    Vector3[,] CPointPos;

    IEnumerator Start()
    {
        GenerateController = new ControllPoint();
        GenerateController.sphere_m = 0;
        GenerateController.sphere_n = 0;
        CPointPos = new Vector3[ControllerPoints - 1, ControllerPoints - 1];
        //GenerateController.Spheres = new GameObject[ControllerPoints, ControllerPoints];
        GenerateController.ControlCube = new GameObject[ControllerPoints + 2, ControllerPoints + 2];

        while (!map.IsReady)
        {
            yield return null;
        }
        int totalCount = 0;
        double boundx = lonToX(map.bounds.MaxLon) - lonToX(map.bounds.MinLon);
        double boundz = latToY(map.bounds.MaxLat) - latToY(map.bounds.MinLat);

        Debug.Log(boundx);
        Debug.Log(boundz);

        for (int i = -1; i < ControllerPoints + 1; i++)
        {
            for (int j = -1; j < ControllerPoints + 1; j++)
            {
                GenerateController.ControlCube[i + 1, j + 1] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                GenerateController.ControlCube[i + 1, j + 1].GetComponent<Transform>().localScale = new Vector3(10, 10, 10);
                GenerateController.ControlCube[i + 1, j + 1].GetComponent<Transform>().position = new Vector3((float)(((boundx / (double)(ControllerPoints - 1))) * i - (boundx / 2)), 0, (float)(((boundz / (double)(ControllerPoints - 1))) * j - (boundz / 2)));
                //Debug.Log(GenerateController.ControlCube[i, j].GetComponent<Transform>().position);
            }
        }


        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {
            totalCount++;
        }

        GenerateController.Spheres = new GameObject[totalCount];
        UVBlock = new Vector4[totalCount];
        UV = new Vector2[totalCount];


        for (int i = 1; i < ControllerPoints; i++)
        {
            for (int j = 1; j < ControllerPoints; j++)
            {
                Vector3 total = new Vector3(0, 0, 0);
                total += GenerateController.ControlCube[i, j].GetComponent<Transform>().position;
                total += GenerateController.ControlCube[i, j + 1].GetComponent<Transform>().position;
                total += GenerateController.ControlCube[i + 1, j].GetComponent<Transform>().position;
                total += GenerateController.ControlCube[i + 1, j + 1].GetComponent<Transform>().position;
                total /= 4.0f;

                CPointPos[i - 1, j - 1] = total;

            }
        }


        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {

            //GenerateController.Spheres[0,0] = new GameObject();
            GenerateController.Spheres[buildCount] = new GameObject();
            GenerateController.Spheres[buildCount].name = "Way";
            Vector3 localOrigin = GetCentre(way);
            //Debug.Log(localOrigin);
            //Debug.Log(map.bounds.Centre);
            Vector3 BuildPos = localOrigin - map.bounds.Centre;
            GenerateController.Spheres[buildCount].transform.position = BuildPos;


            Vector2 Blockij = new Vector2();
            float ShortDisWCPoint = 100000000.0f;
            for (int i = 0; i < ControllerPoints - 1; i++)
            {
                for (int j = 0; j < ControllerPoints - 1; j++)
                {
                    if (ShortDisWCPoint > (BuildPos - CPointPos[i, j]).magnitude)
                    {
                        Blockij = new Vector2(i, j);
                        ShortDisWCPoint = (BuildPos - CPointPos[i, j]).magnitude;
                    }
                }
            }

            //set uv (0~1)
            //UVBlock[buildCount] = new Vector4((float)((double)((BuildPos.x - CPointPos[(int)Blockij.x, (int)Blockij.y].x) / (boundx /4.0f))) + 0.5f, (float)((double)((BuildPos.z - CPointPos[(int)Blockij.x, (int)Blockij.y].z) / (boundz / 4.0f))) + 0.5f, Blockij.x, Blockij.y);
            UVBlock[buildCount] = new Vector4((float)(BuildPos.x / boundx) + 0.5f, (float)(BuildPos.z / boundz) + 0.5f, 0, 0);
            //UVBlock[buildCount] = new Vector4(0.5f, 0, 1, 1);

            Debug.Log(UVBlock[buildCount]);

            MeshFilter mf = GenerateController.Spheres[buildCount].AddComponent<MeshFilter>();
            MeshRenderer mr = GenerateController.Spheres[buildCount].AddComponent<MeshRenderer>();

            mr.material = building;

            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();
            float he = way.Height;
            if (way.Height < 1.0f)
            {
                he = 1.0f;
            }

            for (int i = 1; i < way.NodeIDs.Count; i++)
            {
                OsmNode p1 = map.nodes[way.NodeIDs[i - 1]];
                OsmNode p2 = map.nodes[way.NodeIDs[i]];

                Vector3 v1 = p1 - localOrigin;
                Vector3 v2 = p2 - localOrigin;


                Vector3 v3 = v1 + new Vector3(0, he, 0);
                Vector3 v4 = v2 + new Vector3(0, he, 0);

                vectors.Add(v1);
                vectors.Add(v2);
                vectors.Add(v3);
                vectors.Add(v4);

                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);

                // index values
                int idx1, idx2, idx3, idx4;
                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;
                idx2 = vectors.Count - 3;
                idx1 = vectors.Count - 4;

                // first triangle v1, v3, v2
                indices.Add(idx1);
                indices.Add(idx3);
                indices.Add(idx2);

                // second triangle v3, v4, v2
                indices.Add(idx3);
                indices.Add(idx4);
                indices.Add(idx2);

                // third triangle v2, v3, v1
                indices.Add(idx2);
                indices.Add(idx3);
                indices.Add(idx1);

                // fourth triangle v2, v4, v3
                indices.Add(idx2);
                indices.Add(idx4);
                indices.Add(idx3);
            }

            Vector3 vv1 = new Vector3(0, he, 0);
            vectors.Add(vv1);
            normals.Add(-Vector3.forward);
            int mididx = vectors.Count - 1;

            for (int i = 0; i < way.NodeIDs.Count; i++)
            {
                OsmNode p1 = map.nodes[way.NodeIDs[(i) % way.NodeIDs.Count]];
                OsmNode p2 = map.nodes[way.NodeIDs[(i + 1) % way.NodeIDs.Count]];

                Vector3 v2 = p1 - localOrigin + new Vector3(0, way.Height, 0);
                Vector3 v3 = p2 - localOrigin + new Vector3(0, way.Height, 0);

                vectors.Add(v2);
                vectors.Add(v3);

                normals.Add(-Vector3.forward);
                normals.Add(-Vector3.forward);
                int idx3, idx4;

                idx4 = vectors.Count - 1;
                idx3 = vectors.Count - 2;

                indices.Add(idx4);
                indices.Add(idx3);
                indices.Add(mididx);
            }

            mf.mesh.vertices = vectors.ToArray();
            mf.mesh.normals = normals.ToArray();
            mf.mesh.triangles = indices.ToArray();
            buildCount = buildCount + 1;
            yield return null;
        }

    }


    private void Update()
    {
        //for each uv
        for (int i = 0; i < buildCount; i++)
        {
            Vector3 _p = P(UVBlock[i]);

            if (GenerateController.Spheres[i] != null)
            {
                GenerateController.Spheres[i].GetComponent<Transform>().position = _p;
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
    public Vector3 P(Vector4 UVData)
    {
        float alpha = 0f;
        int Blockx = (int)UVData.z;
        int Blocky = (int)UVData.w;

        Vector3 BlockMid = CPointPos[Blockx, Blocky];

        Vector3 X = CatMullRom(GenerateController.ControlCube[Blockx, Blocky + 1].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 1].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 2, Blocky + 1].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 3, Blocky + 1].GetComponent<Transform>().position, UVData.x, alpha);
        //Vector3 Y = CatMullRom(GenerateController.ControlCube[Blockx + 1, Blocky].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 1].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 2].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 3].GetComponent<Transform>().position, UVData.y, alpha);

        return (X);
    }



    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //for (int i = 0; i < 99; i++)
        //{
        //    Vector3 X1 = CatMullRom(GenerateController.ControlCube[0, 0 + 1].GetComponent<Transform>().position, GenerateController.ControlCube[0 + 1, 0 + 1].GetComponent<Transform>().position, GenerateController.ControlCube[0 + 2, 0 + 1].GetComponent<Transform>().position, GenerateController.ControlCube[0 + 3, 0 + 1].GetComponent<Transform>().position, 0.01f * (float)i, 0.5f);
        //    Vector3 X2 = CatMullRom(GenerateController.ControlCube[0, 0 + 1].GetComponent<Transform>().position, GenerateController.ControlCube[0 + 1, 0 + 1].GetComponent<Transform>().position, GenerateController.ControlCube[0 + 2, 0 + 1].GetComponent<Transform>().position, GenerateController.ControlCube[0 + 3, 0 + 1].GetComponent<Transform>().position, 0.01f * (float)(i + 1), 0.5f);
        //    Gizmos.DrawLine(X1, X2);
        //}
        //for (int i = 1; i < ControllerPoints; i++)
        //{
        //    for (int j = 1; j < ControllerPoints; j++)
        //    {
        //        Gizmos.DrawCube(CPointPos[i - 1, j - 1], new Vector3(10, 100, 10));
        //    }
        //}
    }
}