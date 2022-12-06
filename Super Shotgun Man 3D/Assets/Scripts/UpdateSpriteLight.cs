using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateSpriteLight : MonoBehaviour
{
    private int last_triangleindex;

    void UpdateLight()
    {
        //first check beneath the sprite to see if there is a valid sector
        RaycastHit hit;
        if(Physics.Raycast(transform.position, -Vector3.up, out hit, float.PositiveInfinity, LayerMask.GetMask("Ground"))){

            //now search that subsector for the submesh that contains the material struck
            MeshCollider col = hit.collider as MeshCollider;

            Mesh mesh = col.sharedMesh;

            //optimization check to avoid the for loop
            if (last_triangleindex == hit.triangleIndex) return;
            else last_triangleindex = hit.triangleIndex;

            int limit = hit.triangleIndex * 3;
            int submesh;

            for(submesh = 0; submesh < mesh.GetTriangles(submesh).Length; submesh++)
            {
                int num_indices = mesh.GetTriangles(submesh).Length;
                if (num_indices > limit) break;

                limit -= num_indices;
            }

            Material material = col.GetComponent<MeshRenderer>().sharedMaterials[submesh];

            //set this material's light level to the lightlevel of the material
            GetComponent<MeshRenderer>().material.SetFloat("_Light", material.GetFloat("_Light"));
            return;
        }
        last_triangleindex = -1;
    }

    private void Start()
    {
        last_triangleindex = -1;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLight();
    }
}
