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
                _mesh = GetComponent<MeshFilter>().mesh;
            return _mesh;
        }
    }

}
