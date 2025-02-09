using UnityEngine;

[ExecuteInEditMode] // 允許在編輯模式下執行
public class MaterialSync : MonoBehaviour
{
    // 主材質（被同步的材質）
    public Material sourceMaterial;

    // 需要同步屬性的目標材質陣列
    public Material[] targetMaterials;

    // 同步特定的屬性名稱，例如 "_Color"、"_MainTex"、"_Glossiness" 等
    public string[] propertiesToSync;

    // 每幀更新材質屬性（可選）
    public bool updateEveryFrame = false;

    void Start()
    {
        if (sourceMaterial == null || targetMaterials.Length == 0 || propertiesToSync.Length == 0)
        {
            Debug.LogError("請確保主材質和目標材質，以及需要同步的屬性名稱已設置！");
            return;
        }

        // 初次同步屬性
        SyncMaterialProperties();
    }

    // 非執行時期會在 Inspector 變更時觸發
    void OnValidate()
    {
        SyncMaterialProperties();
    }

    void Update()
    {
        if (Application.isPlaying && updateEveryFrame)
        {
            SyncMaterialProperties();
        }
    }

    // 同步屬性函式
    private void SyncMaterialProperties()
    {
        foreach (var targetMaterial in targetMaterials)
        {
            if (targetMaterial != null)
            {
                foreach (var property in propertiesToSync)
                {
                    if (sourceMaterial.HasProperty(property))
                    {
                        // 檢查是否是顏色屬性
                        if (sourceMaterial.GetTag(property, false) == "_Color")
                        {
                            Color sourceColor = sourceMaterial.GetColor(property);
                            targetMaterial.SetColor(property, sourceColor);
                        }
                        // 檢查是否是貼圖屬性
                        else if (sourceMaterial.GetTexture(property) != null)
                        {
                            Texture sourceTexture = sourceMaterial.GetTexture(property);
                            targetMaterial.SetTexture(property, sourceTexture);
                        }
                        // 檢查是否是浮點數（像是光滑度等）
                        else
                        {
                            float sourceFloat = sourceMaterial.GetFloat(property);
                            targetMaterial.SetFloat(property, sourceFloat);
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"屬性 {property} 不存在於主材質中。");
                    }
                }
            }
        }
    }
}
