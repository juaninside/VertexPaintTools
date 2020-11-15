using UnityEngine;
using System;
using UnityEditor;

[RequireComponent(typeof(MeshCollider))]
public class VPTMesh : MonoBehaviour
{
    private Mesh _mesh;
#if UNITY_EDITOR
    [HideInInspector]
    public Mesh Mesh
    {
        get
        {
            if (GetComponent<VPTUniqueMesh>() == null)
            {
                ObjectFactory.AddComponent<VPTUniqueMesh>(gameObject);
                _mesh = GetComponent<MeshFilter>().sharedMesh = GetUniqueMesh();

            }
            else
            {
                _mesh = GetComponent<MeshFilter>().sharedMesh;
            }

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
#endif
}
