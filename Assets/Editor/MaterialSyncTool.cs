using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MaterialSyncTool : EditorWindow
{
    private Material _sourceMaterial;
    private List<Material> _targetMaterials = new List<Material>();
    private List<string> _propertiesToSync = new List<string>();

    [MenuItem("Tools/Material Sync Tool")]
    public static void ShowWindow()
    {
        GetWindow<MaterialSyncTool>("Material Sync Tool");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Material Sync Tool", EditorStyles.boldLabel);//文字

        // 源材质选择
        _sourceMaterial = (Material)EditorGUILayout.ObjectField("Source Material", _sourceMaterial, typeof(Material), false);

        // 目标材质列表
        EditorGUILayout.LabelField("Target Materials", EditorStyles.boldLabel); //文字
        int removeIndex = -1;
        for (int i = 0; i < _targetMaterials.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();//以下排版變成水平
            _targetMaterials[i] = (Material)EditorGUILayout.ObjectField(_targetMaterials[i], typeof(Material), false);
            if (GUILayout.Button("Remove"))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();//結束水平排版
        }
        if (removeIndex >= 0)
        {
            _targetMaterials.RemoveAt(removeIndex);
        }
        if (GUILayout.Button("Add Target Material"))
        {
            _targetMaterials.Add(null);
        }

        // 同步的属性列表
        EditorGUILayout.LabelField("Properties to Sync", EditorStyles.boldLabel);
        int removePropIndex = -1;
        for (int i = 0; i < _propertiesToSync.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            _propertiesToSync[i] = EditorGUILayout.TextField(_propertiesToSync[i]);
            if (GUILayout.Button("Remove"))
            {
                removePropIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (removePropIndex >= 0)
        {
            _propertiesToSync.RemoveAt(removePropIndex);
        }
        if (GUILayout.Button("Add Property"))
        {
            _propertiesToSync.Add("");
        }

        // 同步按钮
        if (GUILayout.Button("Sync Properties"))
        {
            if (_sourceMaterial != null && _targetMaterials.Count > 0 && _propertiesToSync.Count > 0)
            {
                SyncMaterialProperties();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "make sure you already fill source material and at least one target material and property", "confirm");
            }
        }
    }

    // 同步材质属性的函数
    private void SyncMaterialProperties()
    {
        foreach (var targetMaterial in _targetMaterials)
        {
            if (targetMaterial != null)
            {
                foreach (var property in _propertiesToSync)
                {
                    if (_sourceMaterial.HasProperty(property))
                    {
                        // 同步顏色屬性
                        if (_sourceMaterial.HasProperty(property) && _sourceMaterial.GetColor(property) != new Color(0.04f, 0.01f, 79.1f))
                        {
                            Color sourceColor = _sourceMaterial.GetColor(property);
                            targetMaterial.SetColor(property, sourceColor);
                        }
                        // 同步貼圖屬性
                        if (_sourceMaterial.HasProperty(property) && _sourceMaterial.GetTexture(property) != null)
                        {
                            Texture sourceTexture = _sourceMaterial.GetTexture(property);
                            targetMaterial.SetTexture(property, sourceTexture);
                        }
                        // 同步浮點數屬性
                        if (_sourceMaterial.HasProperty(property) && _sourceMaterial.GetFloat(property) != 0.271f)
                        {
                            float sourceFloat = _sourceMaterial.GetFloat(property);
                            targetMaterial.SetFloat(property, sourceFloat);
                        }
                        // 同步向量屬性
                        if (_sourceMaterial.HasProperty(property) && _sourceMaterial.GetVector(property) != new Vector4(0.04f, 0.01f, 79.1f, 12.955f))
                        {
                            Vector4 sourceVector = _sourceMaterial.GetVector(property);
                            targetMaterial.SetVector(property, sourceVector);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"屬性 {property} 不存在於源材質中。");
                    }
                }
            }
        }  

        // 彈出成功對話框
        EditorUtility.DisplayDialog("Success", "Success to sync properties from source to target", "confirm");
    }
}
