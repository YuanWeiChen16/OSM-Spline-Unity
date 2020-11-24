using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MercatorProjection;

class BuildingMaker : InfrstructureBehaviour
{
    public Material building;

    Vector2[] UV;
    public ControllPoint GenerateController;
    public int ControllerPointsX = 4;
    public int ControllerPointsY = 4;
    public int buildCount = 0;

    public  Vector3[,] NowControlPos;


    IEnumerator Start()
    {
        GenerateController = new ControllPoint();
        GenerateController.sphere_m = ControllerPointsX;
        GenerateController.sphere_n = ControllerPointsY;

        //GenerateController.Spheres = new GameObject[ControllerPoints, ControllerPoints];
        GenerateController.ControlCube = new GameObject[ControllerPointsX, ControllerPointsY];
        NowControlPos = new Vector3[ControllerPointsX, ControllerPointsY];
        while (!map.IsReady)
        {
            yield return null;
        }
        int totalCount = 0;
        double boundx = lonToX(map.bounds.MaxLon) - lonToX(map.bounds.MinLon);
        double boundz = latToY(map.bounds.MaxLat) - latToY(map.bounds.MinLat);

        Debug.Log(boundx);
        Debug.Log(boundz);

        for (int i = 0; i < ControllerPointsX; i++)
        {
            for (int j = 0; j < ControllerPointsY; j++)
            {
                GenerateController.ControlCube[i, j] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                GenerateController.ControlCube[i, j].GetComponent<Transform>().localScale = new Vector3(50, 50, 50);
                GenerateController.ControlCube[i, j].GetComponent<Transform>().position = new Vector3((float)(((boundx / (double)(ControllerPointsX - 1))) * i - (boundx / 2)), 0, (float)(((boundz / (double)(ControllerPointsY - 1))) * j - (boundz / 2)));
                NowControlPos[i, j] = new Vector3((float)(((boundx / (double)(ControllerPointsX - 1))) * i - (boundx / 2)), 0, (float)(((boundz / (double)(ControllerPointsY - 1))) * j - (boundz / 2)));
                //Debug.Log(GenerateController.ControlCube[i, j].GetComponent<Transform>().position);
            }
        }


        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {
            totalCount++;
        }

        GenerateController.Spheres = new GameObject[totalCount];
        UV = new Vector2[totalCount];


        foreach (var way in map.ways.FindAll((w) => { return w.IsBuilding && w.NodeIDs.Count > 1; }))
        {

            //GenerateController.Spheres[0,0] = new GameObject();
            GenerateController.Spheres[buildCount] = new GameObject();
            Vector3 localOrigin = GetCentre(way);
            Debug.Log(localOrigin);
            Debug.Log(map.bounds.Centre);
            GenerateController.Spheres[buildCount].transform.position = localOrigin - map.bounds.Centre;
            float he = way.Height;
            if (way.Height < 1.0f)
            {
                he = 1.0f;
            }
            //set uv (0~1)
            UV[buildCount] = new Vector2((float)((double)((localOrigin.z - map.bounds.Centre.z) / boundz) + 0.5), (float)((double)((localOrigin.x - map.bounds.Centre.x) / boundx) + 0.5));

            Debug.Log(UV[buildCount]);


            MeshFilter mf = GenerateController.Spheres[buildCount].AddComponent<MeshFilter>();
            MeshRenderer mr = GenerateController.Spheres[buildCount].AddComponent<MeshRenderer>();

            //mr.material = building;
            mr.material.shader = Shader.Find("Standard");
            Color tempC = new Color();
            tempC.r = 0.15f * (float)(buildCount % 7);
            tempC.g = 0.25f * (float)(buildCount % 5);
            tempC.b = 0.12f * (float)(buildCount % 9);
            mr.material.color = tempC;


            List<Vector3> vectors = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            for (int i = 1; i < way.NodeIDs.Count; i++)
            {
                OsmNode p1 = map.nodes[way.NodeIDs[i - 1]];
                OsmNode p2 = map.nodes[way.NodeIDs[i]];

                Vector3 v1 = p1 - localOrigin;
                Vector3 v2 = p2 - localOrigin;
                Vector3 v3 = v1 + new Vector3(0, way.Height, 0);
                Vector3 v4 = v2 + new Vector3(0, way.Height, 0);

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

                normals.Add(new Vector3(0, 1, 0));
                normals.Add(new Vector3(0, 1, 0));
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
            Vector3 _p = P(UV[i].x, UV[i].y);
            Vector3 _p2 = P(UV[i].x + 0.001f, UV[i].y);

            if (GenerateController.Spheres[i] != null)
            {
                _p2 = _p2 - _p;
                GenerateController.Spheres[i].GetComponent<Transform>().position = _p;
                GenerateController.Spheres[i].GetComponent<Transform>().rotation = Quaternion.LookRotation(_p2, new Vector3(0, 1, 0));
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
    //compute the position of the point with (u,v) image coordinate 
    public Vector3 P(float u, float v)
    {
        int m = GenerateController.sphere_m;
        int n = GenerateController.sphere_n;
        float tempX = 0;
        float tempY = 0;
        float tempZ = 0;
        for (int j = 0; j < m; j++)
        {
            for (int k = 0; k < n; k++)
            {
                tempX += GenerateController.ControlCube[j, k].GetComponent<Transform>().position.x * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
                tempY += GenerateController.ControlCube[j, k].GetComponent<Transform>().position.y * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
                tempZ += GenerateController.ControlCube[j, k].GetComponent<Transform>().position.z * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
            }
        }
        return new Vector3(tempX, tempY - 2, tempZ);
    }
}