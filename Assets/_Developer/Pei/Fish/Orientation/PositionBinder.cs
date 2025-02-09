using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PositionBinder : MonoBehaviour
{
    [Header("Settings")]

    [Tooltip("Orientation of the position")]
    public Transform targetTransform;

    [Tooltip("Name of the parameter to be bound")]
    public string vectorParamName;

    private Renderer objectRenderer;

    private void Awake()
    {
        InitializeRenderer();
    }

    private void OnEnable()
    {
        InitializeRenderer();
    }

    private void Update()
    {
        if (objectRenderer == null)
        {
            InitializeRenderer();
            if (objectRenderer == null)
                return;
        }

        Vector3 worldPosition = targetTransform.position;

        if (Application.isPlaying)
        {
            // 在運行時直接更新材質參數
            objectRenderer.material.SetVector(vectorParamName, worldPosition);
        }
        else
        {
            if (objectRenderer.sharedMaterial.HasProperty(vectorParamName))
            {
                objectRenderer.sharedMaterial.SetVector(vectorParamName, worldPosition);
#if UNITY_EDITOR
                EditorUtility.SetDirty(objectRenderer.sharedMaterial);
#endif
            }
        }
    }

    private void OnValidate()
    {
        Update();
    }

    private void InitializeRenderer()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"[{nameof(PositionBinder)}] 當前 GameObject ({gameObject.name}) 沒有 Renderer 組件。", this);
            return;
        }

        if (Application.isPlaying)
        {
            // 確保使用實例化的材質
            Material matInstance = objectRenderer.material;
        }
    }
}