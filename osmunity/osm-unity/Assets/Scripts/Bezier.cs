using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bezier : MonoBehaviour
{
    Vector2[] UV;
    public ControllPoint GenerateController;
    public int ControllerPoints = 4;

    public int samplePoint = 50;
    public Vector2 Range;



    void Start()
    {
        //Range = new Vector2(40, 60);
        //GenerateController = new ControllPoint();
        //GenerateController.sphere_m = ControllerPoints;
        //GenerateController.sphere_n = ControllerPoints;
        //GenerateController.Spheres = new GameObject[samplePoint];
        //GenerateController.ControlCube = new GameObject[ControllerPoints, ControllerPoints];
        //UV = new Vector2[samplePoint];
        ////set Control Point
        //for (int i = 0; i < GenerateController.sphere_m; i++)
        //{
        //    for (int j = 0; j < GenerateController.sphere_n; j++)
        //    {
        //        GenerateController.ControlCube[i, j] = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //        GenerateController.ControlCube[i, j].GetComponent<Transform>().position = new Vector3(i * ((int)Range.x / GenerateController.sphere_m), 0, j * ((int)Range.y / GenerateController.sphere_n));
        //    }
        //}
        
        //for (int i = 0; i < samplePoint; i++)
        //{
        //    GenerateController.Spheres[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //    Vector3 tempVec3 = new Vector3(UnityEngine.Random.Range(0, Range.x), 0, UnityEngine.Random.Range(0, Range.y));
        //    GenerateController.Spheres[i].GetComponent<Transform>().position = tempVec3;
        //    UV[i] = new Vector2((tempVec3.x) / (Range.x), (tempVec3.z) / (Range.y));
        //}
    }

    // Update is called once per frame
    void Update()
    {        
        //for each uv
        //for (int i = 0; i < samplePoint; i++)
        //{
        //    Vector3 _p = P(UV[i].x, UV[i].y);
        //    GenerateController.Spheres[i].transform.position = _p;
        //}
    }

    // public float Factorial(int n)
    //{
    //    float product = 1;
    //    while (n != 0)
    //    {
    //        product *= n;
    //        n--;
    //    }
    //    return product;
    //}
    // public float Combin(int n, int k)
    //{
    //    if (n >= k)
    //    {
    //        float result = Factorial(n) / (Factorial(k) * Factorial(n - k));
    //        return result;
    //    }
    //    else
    //    {
    //        return 0;

    //    }
    //}
    // public float BEZ(int k, int n, float u)
    //{
    //    float result = Combin(n, k) * Mathf.Pow(u, k) * Mathf.Pow(1 - u, n - k);
    //    return result;
    //}
    ////compute the position of the point with (u,v) image coordinate 
    // public Vector3 P(float u, float v)
    //{
    //    int m = GenerateController.sphere_m;
    //    int n = GenerateController.sphere_n;
    //    float tempX = 0;
    //    float tempY = 0;
    //    float tempZ = 0;
    //    for (int j = 0; j < m; j++)
    //    {
    //        for (int k = 0; k < n; k++)
    //        {
    //            tempX += GenerateController.ControlCube[j, k].GetComponent<Transform>().position.x * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
    //            tempY += GenerateController.ControlCube[j, k].GetComponent<Transform>().position.y * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
    //            tempZ += GenerateController.ControlCube[j, k].GetComponent<Transform>().position.z * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
    //        }
    //    }
    //    return new Vector3(tempX, tempY - 2, tempZ);
    //}

}

public class ControllPoint
{
    public int sphere_m;
    public int sphere_n;
    public GameObject[] ControlCube;
    public GameObject[] Spheres;
}