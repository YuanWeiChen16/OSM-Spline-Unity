using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using static MercatorProjection;

class MapReader : MonoBehaviour
{

    [HideInInspector]
    public Dictionary<ulong, OsmNode> nodes;

    [HideInInspector]
    public List<OsmWay> ways;

    [HideInInspector]
    public OsmBounds bounds;

    [Tooltip("The resource file that contains the OSM map data")]
    public string resourceFile;

    public bool IsReady { get; private set; }


    Vector2[] UV;
    public ControllPoint GenerateController;
    public int ControllerPoints = 4;
    public int buildCount = 0;
    double boundx;
    double boundz;

    Vector3[] NCPos;

    Vector3[] NodePos;
    float thisA = 0;
    // Start is called before the first frame update
    void Start()
    {
        nodes = new Dictionary<ulong, OsmNode>();
        ways = new List<OsmWay>();
        NCPos = new Vector3[4];
        var txtAsset = Resources.Load<TextAsset>(resourceFile);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);

        SetBounds(doc.SelectSingleNode("/osm/bounds"));
        GetNodes(doc.SelectNodes("/osm/node"));
        GetWays(doc.SelectNodes("osm/way"));

        IsReady = true;
        boundx = lonToX(bounds.MaxLon) - lonToX(bounds.MinLon);
        boundz = latToY(bounds.MaxLat) - latToY(bounds.MinLat);


        for (int i = 0; i < 4; i++)
        {
            NCPos[i] = new Vector3(0, 0, 0);
        }



        Debug.Log(boundx);
        Debug.Log(boundz);
        int totalCount = 0;
        foreach (OsmWay w in ways)
        {
            if (w.Visible)
            {
                Color c = Color.cyan; // cyan for buildings
                if (!w.IsBoundary) c = Color.red; // red for roads

                for (int i = 1; i < w.NodeIDs.Count; i++)
                {
                    totalCount++;
                    totalCount++;
                }
            }
        }

        //GenerateController.Spheres = new GameObject[totalCount];
        UV = new Vector2[totalCount];
        NodePos = new Vector3[totalCount];
        int buildCount = 0;
        foreach (OsmWay w in ways)
        {
            if (w.Visible)
            {
                Color c = Color.cyan; // cyan for buildings
                if (!w.IsBoundary) c = Color.red; // red for roads

                for (int i = 1; i < w.NodeIDs.Count; i++)
                {
                    OsmNode p1 = nodes[w.NodeIDs[i - 1]];
                    OsmNode p2 = nodes[w.NodeIDs[i]];

                    Vector3 v1 = p1 - bounds.Centre;
                    Vector3 v2 = p2 - bounds.Centre;

                    UV[buildCount].x = v1.x / (float)boundx + 0.5f;
                    UV[buildCount].y = v1.z / (float)boundz;

                    buildCount++;
                    UV[buildCount].x = v2.x / (float)boundx + 0.5f;
                    UV[buildCount].y = v2.z / (float)boundz;

                    buildCount++;
                }
            }
        }

    }

    void Update()
    {
        bool CPMove = false;

        for (int i = 0; i < 4; i++)
        {
            if ((NCPos[i] - this.GetComponent<BuildingMaker>().GenerateController.ControlCube[i].GetComponent<Transform>().position).magnitude > 0.0001)
            {
                CPMove = true;
                NCPos[i] = this.GetComponent<BuildingMaker>().GenerateController.ControlCube[i].GetComponent<Transform>().position;
            }
            if (thisA != this.GetComponent<BuildingMaker>().Talpha)
            {
                CPMove = true;
                thisA = this.GetComponent<BuildingMaker>().Talpha;
            }
        }


        if (CPMove == true)
        {
            int buildCount = 0;
            foreach (OsmWay w in ways)
            {
                if (w.Visible)
                {
                    Color c = Color.cyan; // cyan for buildings
                    if (!w.IsBoundary) c = Color.red; // red for roads

                    for (int i = 1; i < w.NodeIDs.Count; i++)
                    {
                        Vector3 _p = P(UV[buildCount]);
                        Vector3 _p2 = P(new Vector2(UV[buildCount].x + 0.001f, UV[buildCount].y));
                        _p2 = _p2 - _p;
                        _p2.Normalize();
                        Vector3 R = Vector3.Cross(_p2, new Vector3(0, 1, 0));
                        R.Normalize();
                        R = (float)(UV[buildCount].y * boundz) * R;
                        Vector3 v1 = _p + R;
                        NodePos[buildCount] = v1;
                        buildCount++;

                        _p = P(UV[buildCount]);
                        _p2 = P(new Vector2(UV[buildCount].x + 0.001f, UV[buildCount].y));
                        _p2 = _p2 - _p;
                        _p2.Normalize();
                        R = Vector3.Cross(_p2, new Vector3(0, 1, 0));
                        R.Normalize();
                        R = (float)(UV[buildCount].y * boundz) * R;
                        Vector3 v2 = _p + R;
                        NodePos[buildCount] = v2;
                        buildCount++;
                        Debug.DrawLine(v1, v2, c);
                    }

                }
            }
        }
        else
        {
            int buildCount = 0;
            foreach (OsmWay w in ways)
            {
                if (w.Visible)
                {
                    Color c = Color.cyan; // cyan for buildings
                    if (!w.IsBoundary) c = Color.red; // red for roads

                    for (int i = 1; i < w.NodeIDs.Count; i++)
                    {
                        Vector3 v1 = NodePos[buildCount];
                        buildCount++;
                        Vector3 v2 = NodePos[buildCount];
                        buildCount++;
                        Debug.DrawLine(v1, v2, c);
                    }
                }
            }
        }
    }

    void GetWays(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode node in xmlNodeList)
        {
            OsmWay way = new OsmWay(node);
            ways.Add(way);
        }

    }

    void GetNodes(XmlNodeList xmlNodeList)
    {
        foreach (XmlNode n in xmlNodeList)
        {
            OsmNode node = new OsmNode(n);
            nodes[node.ID] = node;
        }
    }

    void SetBounds(XmlNode xmlNode)
    {
        bounds = new OsmBounds(xmlNode);
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
        //int Blockx = (int)UVData.z;
        //int Blocky = (int)UVData.w;

        //Vector3 BlockMid = CPointPos[Blockx, Blocky];

        Vector3 P1 = this.GetComponent<BuildingMaker>().GenerateController.ControlCube[0].GetComponent<Transform>().position;
        Vector3 P2 = this.GetComponent<BuildingMaker>().GenerateController.ControlCube[1].GetComponent<Transform>().position;
        Vector3 P3 = this.GetComponent<BuildingMaker>().GenerateController.ControlCube[2].GetComponent<Transform>().position;
        Vector3 P4 = this.GetComponent<BuildingMaker>().GenerateController.ControlCube[3].GetComponent<Transform>().position;


        Vector3 X1 = CatMullRom(P1, P2, P3, P4, UVData.x, this.GetComponent<BuildingMaker>().Talpha);
        //Vector3 X2 = CatMullRom(GenerateController.ControlCube[0].GetComponent<Transform>().position, GenerateController.ControlCube[1].GetComponent<Transform>().position, GenerateController.ControlCube[2].GetComponent<Transform>().position, GenerateController.ControlCube[3].GetComponent<Transform>().position, UVData.x, alpha);

        //Vector3 Y = CatMullRom(GenerateController.ControlCube[Blockx + 1, Blocky].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 1].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 2].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + 3].GetComponent<Transform>().position, UVData.y, alpha);


        //Vector3 TX = new Vector3(0, 0, 0);
        //Vector3 TY = new Vector3(0, 0, 0);
        ////Vector3 TZ = new Vector3(0, 0, 0);
        //for (int i = 0; i < 4; i++)
        //{
        //    for (int j = 0; j < 4; j++)
        //    {
        //        TX += CatMullRom(GenerateController.ControlCube[Blockx, Blocky + j].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 1, Blocky + j].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 2, Blocky + j].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + 3, Blocky + j].GetComponent<Transform>().position, UVData.x, alpha);
        //        TY += CatMullRom(GenerateController.ControlCube[Blockx + i, Blocky].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + i, Blocky + 1].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + i, Blocky + 2].GetComponent<Transform>().position, GenerateController.ControlCube[Blockx + i, Blocky + 3].GetComponent<Transform>().position, UVData.x, alpha);
        //    }
        //}

        //Debug.Log("TX");
        //Debug.Log(TX);
        //Debug.Log("TY");
        //Debug.Log(TY);


        //TX += TY;


        return (X1);
    }
    


}

