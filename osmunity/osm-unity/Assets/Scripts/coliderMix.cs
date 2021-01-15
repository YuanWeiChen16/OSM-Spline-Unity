using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class coliderMix : MonoBehaviour
{
    public Color OrgColor;
    // Start is called before the first frame update
    void Start()
    {        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
   
    private void OnCollisionStay(Collision collision)
    {
        this.GetComponent<MeshRenderer>().sharedMaterial.color = Color.black;
    }
    private void OnCollisionExit(Collision collision)
    {
        this.GetComponent<MeshRenderer>().sharedMaterial.color = OrgColor;
    }
   
}
