using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System.Collections.Generic;

class SeeThrough : CustomPass
{
    // 設定一個圖層遮罩，用於標識將要應用透視效果的對象
    public LayerMask seeThroughLayer = 1;
    // 新增一個圖層遮罩，用於標識要剔除透視效果的對象
    public LayerMask excludeLayer = 0;
    // 定義一個材質，用於呈現透視效果
    public Material seeThroughMaterial = null;

    

    // 用於存儲模板（stencil）著色器的變量，並隱藏在檢查器中
    [SerializeField, HideInInspector]
    Shader stencilShader;

    // 用於渲染模板的材質
    Material stencilMaterialSeeThrough;  // 用於透視對象的材質

    // 定義一組著色器標籤ID，用於識別不同的渲染通道
    ShaderTagId[] shaderTags;

    // 在渲染管道中設置自定義Pass，這個函數在每個場景初始化時調用
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        // 如果沒有指定模板著色器，則從資源中查找一個隱藏的模板著色器
        if (stencilShader == null)
            stencilShader = Shader.Find("Hidden/Renderers/SeeThroughStencil");

        // 創建兩個模板材質，一個用於透視對象，一個用於剔除對象
        stencilMaterialSeeThrough = CoreUtils.CreateEngineMaterial(stencilShader);

        // 定義將要使用的著色器標籤ID，這些ID用於指定渲染對象時使用的渲染模式
        shaderTags = new ShaderTagId[4]
        {
            new ShaderTagId("Forward"),       // 用於前向渲染的標籤
            new ShaderTagId("ForwardOnly"),   // 用於僅前向渲染的標籤
            new ShaderTagId("SRPDefaultUnlit"), // 用於無光照渲染的標籤
            new ShaderTagId("FirstPass"),     // 用於首次渲染通過的標籤
        };
    }

    // 在每個帧中執行自定義Pass，這裡進行實際的渲染操作
    protected override void Execute(CustomPassContext ctx)
    {

        // **第一步：渲染剔除透視效果的對象**
        stencilMaterialSeeThrough.SetInt("_StencilWriteMask", (int)UserStencilUsage.UserBit1);

        // 渲染excludeLayer物件，無條件的標記該Layer所在的像素的Stencil的UserBit1位為1 (參照"Hidden/Renderers/SeeThroughStencil"shader)
        RenderObjects(ctx.renderContext, ctx.cmd, stencilMaterialSeeThrough, 0, CompareFunction.Always, ctx.cullingResults, ctx.hdCamera, excludeLayer);
        // 渲染seeThroughLayer物件，使用深度測試來使該Layer於畫面最前方的部分(像素)的Stencil的UserBit1位為1
        RenderObjects(ctx.renderContext, ctx.cmd, stencilMaterialSeeThrough, 0, CompareFunction.LessEqual, ctx.cullingResults, ctx.hdCamera, seeThroughLayer);

        // **第二步：使用透視材質渲染被遮擋的對象**
        // 創建一個模板狀態，設定為讀取用UserBit1的數據，並使用 Equal 比較函數
        // 確保只有當用戶位1沒有被設置時，才會顯示透視效果
        StencilState excludeStencil = new StencilState(
            enabled: true,
            readMask: (byte)UserStencilUsage.UserBit1, // 使用用戶位1來控制剔除對象的遮擋
            compareFunction: CompareFunction.Equal // 只在用戶位1未設置的情況下通過
        );

        // 渲染透視對象，使用 Greater 深度比較函數來確保對象在牆後
        // 並且透過測試Stencil，使得設置像素UserBit1為1的部分不受透視效果影響
        RenderObjectsWithStencil(ctx.renderContext, ctx.cmd, seeThroughMaterial, seeThroughMaterial.FindPass("ForwardOnly"), CompareFunction.Greater, ctx.cullingResults, ctx.hdCamera, seeThroughLayer, excludeStencil);
    }

    // 用於在檢查器中註冊材質，使其可視化檢查
    public override IEnumerable<Material> RegisterMaterialForInspector() 
    { 
        yield return seeThroughMaterial; 
    }

    // 輔助函數，用於渲染對象，帶有模板狀態檢查
    void RenderObjectsWithStencil(ScriptableRenderContext renderContext, CommandBuffer cmd, Material overrideMaterial, int passIndex, CompareFunction depthCompare, CullingResults cullingResult, HDCamera hdCamera, LayerMask layerMask, StencilState stencilState)
    {
        // 創建一個渲染器列表描述符，指定要渲染的對象及其屬性
        var result = new UnityEngine.Rendering.RendererUtils.RendererListDesc(shaderTags, cullingResult, hdCamera.camera)
        {
            rendererConfiguration = PerObjectData.None,  // 不需要每個對象的特殊數據
            renderQueueRange = RenderQueueRange.all,    // 渲染所有隊列範圍內的對象
            sortingCriteria = SortingCriteria.BackToFront, // 從後到前排序對象
            excludeObjectMotionVectors = false,         // 包括對象的運動向量
            overrideMaterial = overrideMaterial,         // 使用傳入的覆蓋材質
            overrideMaterialPassIndex = passIndex,       // 使用指定的通道索引
            layerMask = layerMask,                       // 只渲染指定圖層的對象

            //RenderStateMask.Depth | RenderStateMask.Stencil 告訴 RenderStateBlock，我們要同時考慮深度和模板狀態。
            stateBlock = new (RenderStateMask.Depth | RenderStateMask.Stencil) // 設定深度和模板狀態
            { 
                depthState = new DepthState(true, depthCompare), // 使用指定的深度比較函數
                stencilState = stencilState                 // 使用透視對象的模板狀態
            }
        };

        // 使用渲染上下文和命令緩衝區渲染這些對象
        CoreUtils.DrawRendererList(renderContext, cmd, renderContext.CreateRendererList(result));
    }

    // 輔助函數，用於渲染對象
    void RenderObjects(ScriptableRenderContext renderContext, CommandBuffer cmd, Material overrideMaterial, int passIndex, CompareFunction depthCompare, CullingResults cullingResult, HDCamera hdCamera, LayerMask? layerMask = null)
    {
        // 設定層掩碼，如果沒有提供則使用所有層（無掩碼）
         var effectiveLayerMask = layerMask ?? ~0;  //如果layerMask為null，則使用~0（所有層）

        // 創建一個渲染器列表描述符，指定要渲染的對象及其屬性
        var result = new UnityEngine.Rendering.RendererUtils.RendererListDesc(shaderTags, cullingResult, hdCamera.camera)
        {
            rendererConfiguration = PerObjectData.None,  // 不需要每個對象的特殊數據
            renderQueueRange = RenderQueueRange.all,    // 渲染所有隊列範圍內的對象
            sortingCriteria = SortingCriteria.BackToFront, // 從後到前排序對象
            excludeObjectMotionVectors = false,         // 包括對象的運動向量
            overrideMaterial = overrideMaterial,         // 使用傳入的覆蓋材質
            overrideMaterialPassIndex = passIndex,       // 使用指定的通道索引
            layerMask = effectiveLayerMask,                       // 只渲染指定圖層的對象
            stateBlock = new RenderStateBlock(RenderStateMask.Depth) // 設定深度狀態
            { 
                depthState = new DepthState(true, depthCompare) // 使用指定的深度比較函數
            },
        };

        // 使用渲染上下文和命令緩衝區渲染這些對象
        CoreUtils.DrawRendererList(renderContext, cmd, renderContext.CreateRendererList(result));
    }

    // 清理函數，在結束時調用，用於釋放資源
    protected override void Cleanup()
    {
        // 這裡可以添加清理代碼，例如釋放材質資源
        CoreUtils.Destroy(stencilMaterialSeeThrough);
    }
}
