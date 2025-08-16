using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor.SceneManagement;

/// <summary>
/// Unity 에디터에서 GetTransformInfoExpectUpToDate 에러를 해결하기 위한 스크립트
/// </summary>
public class URPErrorFix : EditorWindow
{
    [MenuItem("Tools/URP Error Fix")]
    public static void ShowWindow()
    {
        GetWindow<URPErrorFix>("URP Error Fix");
    }

    private void OnGUI()
    {
        GUILayout.Label("URP GetTransformInfoExpectUpToDate 에러 해결 도구", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("1. URP 설정 최적화"))
        {
            OptimizeURPSettings();
        }
        
        if (GUILayout.Button("2. 렌더러 강제 업데이트"))
        {
            ForceRendererUpdate();
        }
        
        if (GUILayout.Button("3. 씬 리프레시"))
        {
            RefreshScene();
        }
        
        if (GUILayout.Button("4. 모든 설정 리셋"))
        {
            ResetAllSettings();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "GetTransformInfoExpectUpToDate 에러는 주로 다음과 같은 경우에 발생합니다:\n" +
            "• Transform 정보가 업데이트되지 않은 상태에서 렌더링 시도\n" +
            "• URP 설정 문제\n" +
            "• 씬의 렌더러 컴포넌트 문제\n" +
            "• Unity 버전 호환성 문제", 
            MessageType.Info);
    }
    
    private void OptimizeURPSettings()
    {
        try
        {
            // URP Asset 찾기
            var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                // 에디터에서만 수정 가능한 설정들
                SerializedObject serializedURP = new SerializedObject(urpAsset);
                
                // SRP Batcher 비활성화 (문제 해결을 위해)
                var useSRPBatcher = serializedURP.FindProperty("m_UseSRPBatcher");
                if (useSRPBatcher != null)
                {
                    useSRPBatcher.boolValue = false;
                    Debug.Log("SRP Batcher를 비활성화했습니다.");
                }
                
                // Dynamic Batching 활성화
                var supportsDynamicBatching = serializedURP.FindProperty("m_SupportsDynamicBatching");
                if (supportsDynamicBatching != null)
                {
                    supportsDynamicBatching.boolValue = true;
                    Debug.Log("Dynamic Batching을 활성화했습니다.");
                }
                
                serializedURP.ApplyModifiedProperties();
            }
            
            // Quality Settings 확인
            QualitySettings.renderPipeline = urpAsset;
            
            Debug.Log("URP 설정을 최적화했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"URP 설정 최적화 중 오류 발생: {e.Message}");
        }
    }
    
    private void ForceRendererUpdate()
    {
        try
        {
            // 모든 렌더러 찾기
            var renderers = FindObjectsOfType<Renderer>();
            int count = 0;
            
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    // Transform 정보 강제 업데이트
                    renderer.transform.hasChanged = true;
                    
                    // Material 강제 업데이트
                    if (renderer.sharedMaterial != null)
                    {
                        renderer.sharedMaterial.SetFloat("_Time", Time.time);
                    }
                    
                    count++;
                }
            }
            
            // 씬 강제 리프레시
            EditorUtility.SetDirty(FindObjectOfType<Camera>());
            
            Debug.Log($"총 {count}개의 렌더러를 강제 업데이트했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"렌더러 강제 업데이트 중 오류 발생: {e.Message}");
        }
    }
    
    private void RefreshScene()
    {
        try
        {
            // 현재 씬 리프레시
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            
            // 모든 게임오브젝트 강제 업데이트
            var allObjects = FindObjectsOfType<GameObject>();
            foreach (var obj in allObjects)
            {
                EditorUtility.SetDirty(obj);
            }
            
            // 에디터 뷰 강제 리프레시
            SceneView.RepaintAll();
            
            Debug.Log("씬을 리프레시했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"씬 리프레시 중 오류 발생: {e.Message}");
        }
    }
    
    private void ResetAllSettings()
    {
        try
        {
            // URP 설정 리셋
            OptimizeURPSettings();
            
            // 렌더러 업데이트
            ForceRendererUpdate();
            
            // 씬 리프레시
            RefreshScene();
            
            // 에디터 뷰 리프레시
            EditorUtility.RequestScriptReload();
            
            Debug.Log("모든 설정을 리셋했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"설정 리셋 중 오류 발생: {e.Message}");
        }
    }
}

/// <summary>
/// 에디터 시작 시 자동으로 URP 에러를 체크하고 수정하는 스크립트
/// </summary>
[InitializeOnLoad]
public class URPErrorAutoFix
{
    static URPErrorAutoFix()
    {
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnEditorUpdate()
    {
        // 에디터가 완전히 로드된 후에만 실행
        if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            return;
            
        // 한 번만 실행
        EditorApplication.update -= OnEditorUpdate;
        
        // URP 에러 자동 수정
        AutoFixURPErrors();
    }
    
    private static void AutoFixURPErrors()
    {
        try
        {
            // URP Asset이 설정되어 있는지 확인
            var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogWarning("URP Asset이 설정되지 않았습니다. Graphics Settings를 확인하세요.");
                return;
            }
            
            // 기본 설정 최적화
            var serializedURP = new SerializedObject(urpAsset);
            var useSRPBatcher = serializedURP.FindProperty("m_UseSRPBatcher");
            
            if (useSRPBatcher != null && useSRPBatcher.boolValue)
            {
                useSRPBatcher.boolValue = false;
                serializedURP.ApplyModifiedProperties();
                Debug.Log("자동으로 SRP Batcher를 비활성화했습니다. GetTransformInfoExpectUpToDate 에러를 방지합니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"URP 자동 수정 중 오류 발생: {e.Message}");
        }
    }
}
