using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class drawLine : MonoBehaviour
{
    public Transform CP1;
    public Transform CP2;
    public Transform CP3;
    public Transform CP4;
    public float Tans = 0.5f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Vector3 P1 = CP1.position;
        Vector3 P2 = CP2.position;
        Vector3 P3 = CP3.position;
        Vector3 P4 = CP4.position;



        for (int i = 0; i < 99; i++)
        {
            Gizmos.DrawLine(CatMullRom(P1, P2, P3, P4, (1.0f / 100.0f) * (float)i, Tans), CatMullRom(P1, P2, P3, P4, (1.0f / 100.0f) * (float)(i + 1.0f), Tans));
        }

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

    public float GetT(float t, float alpha, Vector3 p0, Vector3 p1)
    {
        Vector3 d = p1 - p0;
        float a = d.magnitude; // Dot product
        float b = Mathf.Pow(a, alpha * 0.5f);
        return (b + t);
    }

}
