using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "VPT_Configuration", menuName = "VertexPaintTools/Configuration", order = 0)]
public class VPTConfiguration : ScriptableObject
{
    [Header("Skin")]
    public GUISkin GUISkin;

    [Header("Icons")]
    public Texture BrushActiveIcon;
    public Texture BrushInactiveIcon;
    public Texture FillColorIcon;

    [Header("Brush Settings")]
    [Tooltip("Prefab to be instantiated as a brush, sphere mesh prefered because the tool don't use normals to reposition it.")]
    public GameObject VPT_Brush;
    public float MinBrushSize = 0.1f;
    public float MaxBrushSize = 2f;

}
