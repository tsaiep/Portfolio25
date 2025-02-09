using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class AdjustMeshBounds : MonoBehaviour
{
    [Header("Bounding Box Settings")]
    [Tooltip("Bounding Box的大小")]
    public Vector3 boundsSize = Vector3.one;
    [Tooltip("Bounding Box的中心偏移量")]
    public Vector3 boundsPivot = Vector3.zero;
    void Start()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Bounds bounds = new Bounds();

        // 將 Bounds 擴展指定的範圍
        bounds.size = boundsSize;
        bounds.center = boundsPivot;
        mesh.bounds = bounds;
    }
    private void Awake()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Bounds bounds = new Bounds();

        // 將 Bounds 擴展指定的範圍
        bounds.size = boundsSize;
        bounds.center = boundsPivot;
        mesh.bounds = bounds;
    }

    private void OnEnable()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Bounds bounds = new Bounds();

        // 將 Bounds 擴展指定的範圍
        bounds.size = boundsSize;
        bounds.center = boundsPivot;
        mesh.bounds = bounds;
    }
    
    [Header("Gizmo Settings")]
    [Tooltip("是否顯示Gizmo")]
    public bool showGizmo = true;
    [Tooltip("Gizmo的顏色")]
    public Color gizmoColor = Color.green;
    
    // 繪製Gizmo
    void OnDrawGizmos()
    {
        if (!showGizmo) return;

        // 設置Gizmo顏色
        Gizmos.color = gizmoColor;

        // 保存原來的矩陣(世界空間)
        Matrix4x4 originalMatrix = Gizmos.matrix;

        // 設置Gizmos矩陣為物件空間的矩陣，包含位置、旋轉和縮放
        Gizmos.matrix = transform.localToWorldMatrix;

        // 繪製Wire Cube，位置在boundsPivot，尺寸為boundsSize
        Gizmos.DrawWireCube(boundsPivot, boundsSize);

        // 還原Gizmos矩陣
        Gizmos.matrix = originalMatrix;
    }
    
    // 當腳本屬性改變時更新
    void OnValidate()
    {
        // 確保boundsSize不為零，避免無法繪製
        boundsSize = Vector3.Max(boundsSize, Vector3.one * 0.01f);
    }
}
