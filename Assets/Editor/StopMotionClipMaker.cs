using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// 創建一個名為 StopMotionClipMaker 的編輯器窗口類別，繼承自 EditorWindow
public class StopMotionClipMaker : EditorWindow
{
    // 私有變數，用於存儲原始動畫剪輯和時間間隔
    private List<AnimationClip> originalClips = new List<AnimationClip>();
    private int _frameInterval = 15;

    // 在 Unity 編輯器的菜單中添加一個選項，點擊後打開此工具窗口
    [MenuItem("Tools/Stop Motion Clip Maker")]
    public static void ShowWindow()
    {
        // 創建並顯示編輯器窗口，窗口標題為 "Stop Motion Clip Maker"
        GetWindow<StopMotionClipMaker>("Stop Motion Clip Maker");
    }

    // 定義編輯器窗口的 GUI，類似於 MonoBehaviour 的 OnGUI 方法
    private void OnGUI()
    {
        // 添加一個標籤，作為窗口的標題，使用粗體字體樣式
        GUILayout.Label("Stop Motion Clip Maker", EditorStyles.boldLabel);

        // 添加一個對象字段，讓用戶選擇原始動畫剪輯（AnimationClip）
        EditorGUILayout.LabelField("Original Clips", EditorStyles.boldLabel); //文字
        int removeIndex = -1;
        for (int i = 0; i < originalClips.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();//以下排版變成水平
            originalClips[i] = (AnimationClip)EditorGUILayout.ObjectField(originalClips[i], typeof(AnimationClip), false);
            if (GUILayout.Button("Remove"))
            {
                removeIndex = i;
            }
            EditorGUILayout.EndHorizontal();//結束水平排版
        }
        if (removeIndex >= 0)
        {
            originalClips.RemoveAt(removeIndex);
        }
        if (GUILayout.Button("Add Clips"))
        {
            originalClips.Add(null);
        }
        // 添加一個整數輸入字段，讓用戶設置幀間隔
        _frameInterval = EditorGUILayout.IntField("Frame Interval", _frameInterval);

        // 添加一個按鈕，當用戶點擊時執行生成新動畫剪輯的功能
        if (GUILayout.Button("Generate"))
        {
            // 檢查是否已選擇原始動畫剪輯且幀間隔大於0
            if (originalClips != null && _frameInterval > 0)
            {
                // 調用方法，修改動畫剪輯
                ModifyAnimationClip();
            }
            else
            {
                // 彈出錯誤對話框，提示用戶需要填寫必要的信息
                EditorUtility.DisplayDialog("Error", "Make sure you fill the source and set frame interval above 0.", "Confirm");
            }
        }
    }

    // 修改動畫剪輯的方法，實現主要功能區塊
    private void ModifyAnimationClip()
    {
        foreach (AnimationClip clip in originalClips)
        {
            // 創建一個新的動畫剪輯，用於存儲修改後的內容
            AnimationClip newClip = new AnimationClip();
            // 設置新動畫剪輯的幀率與原始剪輯相同
            newClip.frameRate = clip.frameRate;

            // 從原始動畫剪輯中獲取所有曲線綁定（動畫化的屬性）
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);

            // 遍歷每一個曲線綁定，對其進行處理
            foreach (var binding in curveBindings)
            {
                // 獲取原始曲線，表示該屬性的動畫變化
                AnimationCurve originalCurve = AnimationUtility.GetEditorCurve(clip, binding);
                // 創建一個新的曲線，用於存放修改後的關鍵幀
                AnimationCurve newCurve = new AnimationCurve();

                // 獲取動畫剪輯的總時長（以秒為單位）
                float clipLength = clip.length;
                // 計算總幀數，根據幀率和剪輯長度，並向上取整
                int totalFrames = Mathf.CeilToInt(clipLength * clip.frameRate);
                // 獲取用戶設置的幀間隔
                int interval = _frameInterval;

                // 從第0幀開始，按照指定的幀間隔遍歷到總幀數
                for (int frame = 0; frame <= totalFrames; frame += interval)
                {
                    // 計算當前幀對應的時間（秒）
                    float time = frame / clip.frameRate;
                    // 在原始曲線上取樣，獲取該時間點的值
                    float value = originalCurve.Evaluate(time);

                    // 創建一個新的關鍵幀，包含時間和取樣值
                    Keyframe key = new Keyframe(time, value);
                    // 設置關鍵幀的權重模式為無（默認）
                    key.weightedMode = WeightedMode.None;

                    // 將新的關鍵幀添加到新曲線中，並獲取其索引
                    int keyIndex = newCurve.AddKey(key);

                    // 設置關鍵幀的左、右切線模式為常量，確保曲線在關鍵幀之間保持數值不變
                    AnimationUtility.SetKeyLeftTangentMode(newCurve, keyIndex, AnimationUtility.TangentMode.Constant);
                    AnimationUtility.SetKeyRightTangentMode(newCurve, keyIndex, AnimationUtility.TangentMode.Constant);
                }

                // 將修改後的新曲線綁定到新動畫剪輯上，替換原有的曲線
                AnimationUtility.SetEditorCurve(newClip, binding, newCurve);
            }

            // 獲取原始動畫剪輯的資產路徑
            string originalPath = AssetDatabase.GetAssetPath(clip);
            // 生成新動畫剪輯的保存路徑，文件名加上 "_Modified"
            string newPath = originalPath.Replace(".anim", "_Modified.anim");
            // 將新動畫剪輯保存為資產文件，添加到資產數據庫中
            AssetDatabase.CreateAsset(newClip, newPath);
            AssetDatabase.SaveAssets();

            // 彈出成功對話框，提示用戶新動畫剪輯已生成
            EditorUtility.DisplayDialog("Success", "Generated the new clip:" + newPath, "Confirm");
        }
    }
}
