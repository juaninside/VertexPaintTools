using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(VPTMesh))]
class VPTEditor : Editor
{
    private GUISkin _guiSkin;

    private VPTConfiguration _config;
    private float _brushSize = 1;
    private float _brushOpacity = 1;

    private int colorId = 0;
    private readonly string[] _rgbToolbar = { "R", "G", "B" };

    private bool _brushMode = false;
    private GameObject _activeObject;
    private VPTMesh _activeVPTMesh;
    private GameObject _brush;
    private bool _brushNeedUpdate = true;

    private Color _selectedColor = Color.red;
    private Tool _currentTool = Tool.Custom;
    private LayerMask _objectLayer;
    private LayerMask _paintingLayer;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
    private void OnSceneGUI()
    {
        UpdateSkin();
        Rect windowRect = new Rect(20, 40, 80, 0);
        GUILayout.Window(0, windowRect, WindowVPTools, "Vertex Paint Tools");

        if (_brushMode)
        {
            HideGizmos();
            if (_activeObject != ((Component)target).gameObject)
            {
                _activeObject = Selection.activeGameObject = ((Component)target).gameObject;
                _activeVPTMesh = _activeObject.GetComponent<VPTMesh>();
                _objectLayer = _activeObject.layer;
                _activeObject.layer = _paintingLayer = LayerMask.NameToLayer(CreateLayer("PaintingLayer"));

            }

            Rect rect = new Rect(195, 40, 150, 0);
            GUILayout.Window(1, rect, WindowBrushSettings, "Brush Settings");

            if (_brush == null)
            {
                _brush = Instantiate(_config.VPT_Brush, Vector3.zero, Quaternion.identity);
            }
            else
            {
                if (!BrushPlacementOnSelectedMesh(_activeObject, Event.current.mousePosition))
                {
                    _brush.SetActive(false);
                }
                else
                {
                    _brush.SetActive(true);
                    if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
                    {
                        PaintVertex(_activeVPTMesh);
                    }
                }
            }
        }
    }
    private void OnEnable()
    {
        LoadResources();
    }
    private void OnDisable()
    {
        CleanTool();
    }

    private void CleanTool()
    {
        if (_activeObject != null)
            _activeObject.layer = _objectLayer;
        if (_brush != null)
        {
            DestroyImmediate(_brush.gameObject);
        }

    }

    void ShowGizmos()
    {
        if (_currentTool != Tool.Custom)
        {
            Tools.current = _currentTool;
            _currentTool = Tool.Custom;
        }
    }

    void HideGizmos()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        if (_currentTool == Tool.Custom)
        {
            _currentTool = Tools.current;
        }

        Tools.current = Tool.None;
    }
    private void WindowBrushSettings(int windowID)
    {
        GUILayout.Space(5);
        BrushSettingSize();

        GUILayout.Space(5);
        BrushSettingOpacity();

        if (_brushNeedUpdate)
        {
            UpdateBrush();
        }
    }
    private void BrushSettingSize()
    {
        GUILayout.Label("Brush Size : " + _brushSize.ToString("0.00"));
        var lastBrushSize = _brushSize;
        _brushSize = GUILayout.HorizontalSlider(_brushSize, _config.MinBrushSize, _config.MaxBrushSize);
        if (_brushSize != lastBrushSize) _brushNeedUpdate = true;
    }
    private void BrushSettingOpacity()
    {
        GUILayout.Label("Brush Opacity : " + _brushOpacity.ToString("0.00"));
        var lastBrushOpacity = _brushOpacity;
        _brushOpacity = GUILayout.HorizontalSlider(_brushOpacity, 0, 1);
        if (_brushOpacity != lastBrushOpacity) _brushNeedUpdate = true;
    }
    private void WindowVPTools(int windowID)
    {
        GUILayout.Space(5);
        DrawActions();

        GUILayout.Space(5);
        RBGToolbar();

        GUILayout.Space(5);
        _selectedColor = EditorGUILayout.ColorField("", _selectedColor, GUILayout.Width(150));

    }
    private void DrawActions()
    {
        GUILayout.BeginHorizontal();
        if (!_brushMode)
        {
            if (GUILayout.Button(new GUIContent(_config.BrushInactiveIcon), GUILayout.Width(75), GUILayout.Height(70)))
            {
                HideGizmos();
                _brushMode = true;
            }
        }
        else
        {
            if (GUILayout.Button(new GUIContent(_config.BrushActiveIcon), GUILayout.Width(75), GUILayout.Height(70)))
            {
                ShowGizmos();
                _brushMode = false;
            }
        }
        if (GUILayout.Button(new GUIContent(_config.FillColorIcon), GUILayout.Width(70), GUILayout.Height(70)))
        {
            _activeObject = Selection.activeGameObject = ((Component)target).gameObject; //Here! Manually assign the selection to be your object
            _activeVPTMesh = _activeObject.GetComponent<VPTMesh>();
            PaintVertexFill(_activeVPTMesh);
        }
        GUILayout.EndHorizontal();
    }
    private void RBGToolbar()
    {
        var lastColorId = colorId;
        colorId = GUILayout.Toolbar(colorId, _rgbToolbar, GUILayout.Height(30));
        if (colorId != lastColorId)
        {
            UpdateColorPicker(colorId);
            _brushNeedUpdate = true;
        }
    }
    private void LoadResources()
    {
        _config = Resources.Load<VPTConfiguration>("VPT_Configuration");
        _guiSkin = _config.GUISkin;
    }
    private void UpdateSkin()
    {
        GUI.skin = _guiSkin;
    }
    private void UpdateBrush()
    {
        if (_brush == null) return;
        var size = new Vector3(_brushSize, _brushSize, _brushSize);
        _brush.transform.localScale = size;
        var brushColor = _selectedColor;
        brushColor.a = _brushOpacity;
        _brush.GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Color", brushColor);
        _brushNeedUpdate = false;
    }
    private void UpdateColorPicker(int index)
    {
        switch (index)
        {
            case 0:
                _selectedColor = Color.red;
                break;
            case 1:
                _selectedColor = Color.green;
                break;
            case 2:
                _selectedColor = Color.blue;
                break;
        }
    }
    private bool BrushPlacementOnSelectedMesh(GameObject activeGameObject, Vector2 mousePos)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        RaycastHit hitData;
        if (Physics.Raycast(ray, out hitData, 100, 1 << _paintingLayer))
        {
            if (hitData.transform.gameObject == activeGameObject)
            {
                _brush.transform.position = hitData.point;
                return true;
            }
        }
        return false;
    }
    private void PaintVertex(VPTMesh target)
    {
        Vector3[] vertices = target.Mesh.vertices;
        Color[] colors = target.Mesh.colors;

        if (vertices.Length != colors.Length)
        {
            colors = new Color[vertices.Length];
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = target.transform.TransformPoint(vertices[i]);
            var distance = Vector3.Distance(vertices[i], _brush.transform.position);
            var color = Color.white;


            colors[i] = GetColor(colors[i], distance);
        }

        target.Mesh.SetColors(colors);

    }
    private void PaintVertexFill(VPTMesh target)
    {
        Vector3[] vertices = target.Mesh.vertices;
        Color[] colors = target.Mesh.colors;

        if (vertices.Length != colors.Length) //if the mesh doesn't have vertex color we create them.
        {
            colors = new Color[vertices.Length];
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = _selectedColor;
        }

        target.Mesh.SetColors(colors);

    }
    private Color GetColor(Color currentColor, float distance)
    {
        var fraction = Mathf.InverseLerp(_brushSize, 0, distance);
        return Color.Lerp(currentColor, _selectedColor, fraction * _brushOpacity);
    }

    string CreateLayer(string layerName)
    {
        //https://forum.unity3d.com/threads/adding-layer-by-script.41970/reply?quote=2274824
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");

        for (int i = 8; i < layers.arraySize; i++)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(i);

            if (layerSP.stringValue == layerName)
            {

                return layerName;
            }
        }

        for (int j = 8; j < layers.arraySize; j++)
        {
            SerializedProperty layerSP = layers.GetArrayElementAtIndex(j);
            if (layerSP.stringValue == "")
            {
                layerSP.stringValue = layerName;
                tagManager.ApplyModifiedProperties();
                return layerName;
            }
        }

        return layers.GetArrayElementAtIndex(layers.arraySize - 1).stringValue;

    }

}

