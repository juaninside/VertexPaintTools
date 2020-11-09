using UnityEngine;
using System;


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

        foreach (var property in originalMesh.GetType().GetProperties())
        {
            if (property.GetSetMethod() != null)
            {
                newMesh.GetType().GetProperty(property.Name).SetValue(newMesh, originalMesh.GetType().GetProperty(property.Name).GetValue(originalMesh));
            }            
        }

        newMesh.name = originalMesh.name + " Clone" + gameObject.GetInstanceID();

        return newMesh;
    }

}
