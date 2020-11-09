using UnityEngine;


[RequireComponent(typeof(MeshCollider))]
public class VPTMesh : MonoBehaviour
{
    private Mesh _mesh;
    [HideInInspector]
    public Mesh Mesh
    {
        get
        {
            if (_mesh == null)
                GetComponent<MeshFilter>().sharedMesh = _mesh = GetUniqueMesh();
            return _mesh;
        }
    }

    Mesh GetUniqueMesh()
    {
        var newMesh = new Mesh();
        var originalMesh = GetComponent<MeshFilter>().sharedMesh;
        newMesh.vertices = originalMesh.vertices;
        newMesh.triangles = originalMesh.triangles;
        newMesh.uv = originalMesh.uv;
        newMesh.normals = originalMesh.normals;
        newMesh.colors = originalMesh.colors;
        newMesh.tangents = originalMesh.tangents;
        newMesh.name = originalMesh.name + " VPT" + gameObject.GetInstanceID();

        return newMesh;
    }

}
