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
    int buildCount = 0;
    Vector3[] NowPos;
    // Start is called before the first frame update
    void Start()
    {
        nodes = new Dictionary<ulong, OsmNode>();
        ways = new List<OsmWay>();

        var txtAsset = Resources.Load<TextAsset>(resourceFile);

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(txtAsset.text);

        SetBounds(doc.SelectSingleNode("/osm/bounds"));
        GetNodes(doc.SelectNodes("/osm/node"));
        GetWays(doc.SelectNodes("osm/way"));

        IsReady = true;
        
        int totalCount = 0;
        double boundx = lonToX(this.bounds.MaxLon) - lonToX(this.bounds.MinLon);
        double boundz = latToY(this.bounds.MaxLat) - latToY(this.bounds.MinLat);
        
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
        
        UV = new Vector2[totalCount];
        NowPos = new Vector3[totalCount];


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
                    UV[buildCount] = new Vector2((float)((double)((v1.z) / boundz) + 0.5), (float)((double)((v1.x) / boundx) + 0.5));
                    NowPos[buildCount] = v1;
                    buildCount++;
                    UV[buildCount] = new Vector2((float)((double)((v2.z) / boundz) + 0.5), (float)((double)((v2.x) / boundx) + 0.5));
                    NowPos[buildCount] = v2;
                    buildCount++;
                }
            }
        }

    }

    void Update()
    {
        Vector3[,] NCPos = this.GetComponent<BuildingMaker>().NowControlPos;
        int CX = this.GetComponent<BuildingMaker>().ControllerPointsX;
        int CY = this.GetComponent<BuildingMaker>().ControllerPointsY;
        bool CPMove = false;
        for (int i = 0; i < CX; i++)
        {
            for (int j = 0; j < CY; j++)
            {
                if ((NCPos[i, j] - this.GetComponent<BuildingMaker>().GenerateController.ControlCube[i, j].GetComponent<Transform>().position).magnitude > 0.0001)
                {
                    CPMove = true;
                    NCPos[i, j] = this.GetComponent<BuildingMaker>().GenerateController.ControlCube[i, j].GetComponent<Transform>().position;
                }
            }
        }        
        int buildCount = 0;
        foreach (OsmWay w in ways)
        {
            if (w.Visible)
            {
                Color c = Color.cyan; // cyan for buildings
                if (!w.IsBoundary) c = Color.red; // red for roads

                for (int i = 1; i < w.NodeIDs.Count; i++)
                {
                    if (CPMove == false)
                    {
                        Vector3 v1 = NowPos[buildCount];
                        buildCount++;
                        Vector3 v2 = NowPos[buildCount];
                        buildCount++;
                        Debug.DrawLine(v1, v2, c);
                    }
                    else if (CPMove == true)
                    {
                        OsmNode p1 = nodes[w.NodeIDs[i - 1]];
                        OsmNode p2 = nodes[w.NodeIDs[i]];
                        Vector3 v1 = p1 - bounds.Centre;
                        Vector3 v2 = p2 - bounds.Centre;
                        Vector3 _p1 = P(UV[buildCount].x, UV[buildCount].y);
                        //UV[buildCount] = new Vector2((float)((double)((v1.z) / boundz) + 0.5), (float)((double)((v1.x) / boundx) + 0.5));
                        NowPos[buildCount] = _p1;
                        buildCount++;
                        Vector3 _p2 = P(UV[buildCount].x, UV[buildCount].y);
                        NowPos[buildCount] = _p2;
                        //UV[buildCount] = new Vector2((float)((double)((v2.z) / boundz) + 0.5), (float)((double)((v2.x) / boundx) + 0.5));
                        buildCount++;                        
                        Debug.DrawLine(_p1, _p2, c);
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
        if (this.GetComponent<BuildingMaker>().GenerateController == null)
        {
            return new Vector3(0, 0 - 2, 0);
        }
        int m = this.GetComponent<BuildingMaker>().GenerateController.sphere_m;
        int n = this.GetComponent<BuildingMaker>().GenerateController.sphere_n;

        float tempX = 0;
        float tempY = 0;
        float tempZ = 0;
        for (int j = 0; j < m; j++)
        {
            for (int k = 0; k < n; k++)
            {
                if (this.GetComponent<BuildingMaker>().GenerateController.ControlCube[j, k] != null)
                {
                    tempX += this.GetComponent<BuildingMaker>().GenerateController.ControlCube[j, k].GetComponent<Transform>().position.x * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
                    tempY += this.GetComponent<BuildingMaker>().GenerateController.ControlCube[j, k].GetComponent<Transform>().position.y * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
                    tempZ += this.GetComponent<BuildingMaker>().GenerateController.ControlCube[j, k].GetComponent<Transform>().position.z * BEZ(j, m - 1, v) * BEZ(k, n - 1, u);
                }
            }
        }
        return new Vector3(tempX, tempY - 2, tempZ);
    }
}

