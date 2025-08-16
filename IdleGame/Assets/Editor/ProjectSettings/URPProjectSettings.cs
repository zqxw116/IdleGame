using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// 프로젝트 레벨에서 URP 설정을 관리하고 GetTransformInfoExpectUpToDate 에러를 방지합니다.
/// </summary>
public class URPProjectSettings : EditorWindow
{
    [MenuItem("Tools/Project Settings/URP Project Settings")]
    public static void ShowWindow()
    {
        GetWindow<URPProjectSettings>("URP Project Settings");
    }
    
    private bool autoFixOnStart = true;
    private bool disableSRPBatcher = true;
    private bool enableDynamicBatching = true;
    private bool forceRendererUpdate = false;
    
    private void OnEnable()
    {
        LoadSettings();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("URP 프로젝트 설정", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        autoFixOnStart = EditorGUILayout.Toggle("에디터 시작 시 자동 수정", autoFixOnStart);
        disableSRPBatcher = EditorGUILayout.Toggle("SRP Batcher 비활성화", disableSRPBatcher);
        enableDynamicBatching = EditorGUILayout.Toggle("Dynamic Batching 활성화", enableDynamicBatching);
        forceRendererUpdate = EditorGUILayout.Toggle("강제 렌더러 업데이트", forceRendererUpdate);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("설정 적용"))
        {
            ApplySettings();
        }
        
        if (GUILayout.Button("설정 저장"))
        {
            SaveSettings();
        }
        
        if (GUILayout.Button("설정 리셋"))
        {
            ResetSettings();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "이 설정들은 GetTransformInfoExpectUpToDate 에러를 방지하기 위한 것입니다.\n" +
            "• SRP Batcher 비활성화: Transform 업데이트 문제 해결\n" +
            "• Dynamic Batching 활성화: 렌더링 성능 향상\n" +
            "• 강제 렌더러 업데이트: Transform 정보 동기화", 
            MessageType.Info);
    }
    
    private void LoadSettings()
    {
        autoFixOnStart = EditorPrefs.GetBool("URP_AutoFixOnStart", true);
        disableSRPBatcher = EditorPrefs.GetBool("URP_DisableSRPBatcher", true);
        enableDynamicBatching = EditorPrefs.GetBool("URP_EnableDynamicBatching", true);
        forceRendererUpdate = EditorPrefs.GetBool("URP_ForceRendererUpdate", false);
    }
    
    private void SaveSettings()
    {
        EditorPrefs.SetBool("URP_AutoFixOnStart", autoFixOnStart);
        EditorPrefs.SetBool("URP_DisableSRPBatcher", disableSRPBatcher);
        EditorPrefs.SetBool("URP_EnableDynamicBatching", enableDynamicBatching);
        EditorPrefs.SetBool("URP_ForceRendererUpdate", forceRendererUpdate);
        
        Debug.Log("URP 프로젝트 설정을 저장했습니다.");
    }
    
    private void ApplySettings()
    {
        try
        {
            // URP Asset 설정
            var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset != null)
            {
                var serializedURP = new SerializedObject(urpAsset);
                
                if (disableSRPBatcher)
                {
                    var useSRPBatcher = serializedURP.FindProperty("m_UseSRPBatcher");
                    if (useSRPBatcher != null)
                    {
                        useSRPBatcher.boolValue = false;
                        Debug.Log("SRP Batcher를 비활성화했습니다.");
                    }
                }
                
                if (enableDynamicBatching)
                {
                    var supportsDynamicBatching = serializedURP.FindProperty("m_SupportsDynamicBatching");
                    if (supportsDynamicBatching != null)
                    {
                        supportsDynamicBatching.boolValue = true;
                        Debug.Log("Dynamic Batching을 활성화했습니다.");
                    }
                }
                
                serializedURP.ApplyModifiedProperties();
            }
            
            // 강제 렌더러 업데이트
            if (forceRendererUpdate)
            {
                ForceAllRendererUpdate();
            }
            
            Debug.Log("URP 프로젝트 설정을 적용했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"설정 적용 중 오류 발생: {e.Message}");
        }
    }
    
    private void ResetSettings()
    {
        autoFixOnStart = true;
        disableSRPBatcher = true;
        enableDynamicBatching = true;
        forceRendererUpdate = false;
        
        SaveSettings();
        Debug.Log("URP 프로젝트 설정을 리셋했습니다.");
    }
    
    private void ForceAllRendererUpdate()
    {
        try
        {
            var renderers = FindObjectsOfType<Renderer>();
            int count = 0;
            
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    renderer.transform.hasChanged = true;
                    count++;
                }
            }
            
            Debug.Log($"총 {count}개의 렌더러를 강제 업데이트했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"렌더러 강제 업데이트 중 오류 발생: {e.Message}");
        }
    }
}

/// <summary>
/// 프로젝트 시작 시 자동으로 URP 설정을 적용합니다.
/// </summary>
[InitializeOnLoad]
public class URPProjectAutoSetup
{
    static URPProjectAutoSetup()
    {
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnEditorUpdate()
    {
        if (EditorApplication.isUpdating || EditorApplication.isCompiling)
            return;
            
        EditorApplication.update -= OnEditorUpdate;
        
        // 자동 수정이 활성화되어 있으면 실행
        if (EditorPrefs.GetBool("URP_AutoFixOnStart", true))
        {
            AutoSetupURP();
        }
    }
    
    private static void AutoSetupURP()
    {
        try
        {
            var urpAsset = GraphicsSettings.renderPipelineAsset as UniversalRenderPipelineAsset;
            if (urpAsset == null)
                return;
                
            var serializedURP = new SerializedObject(urpAsset);
            bool hasChanges = false;
            
            // SRP Batcher 비활성화
            if (EditorPrefs.GetBool("URP_DisableSRPBatcher", true))
            {
                var useSRPBatcher = serializedURP.FindProperty("m_UseSRPBatcher");
                if (useSRPBatcher != null && useSRPBatcher.boolValue)
                {
                    useSRPBatcher.boolValue = false;
                    hasChanges = true;
                }
            }
            
            // Dynamic Batching 활성화
            if (EditorPrefs.GetBool("URP_EnableDynamicBatching", true))
            {
                var supportsDynamicBatching = serializedURP.FindProperty("m_SupportsDynamicBatching");
                if (supportsDynamicBatching != null && !supportsDynamicBatching.boolValue)
                {
                    supportsDynamicBatching.boolValue = true;
                    hasChanges = true;
                }
            }
            
            if (hasChanges)
            {
                serializedURP.ApplyModifiedProperties();
                Debug.Log("URP 프로젝트 설정이 자동으로 적용되었습니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"URP 자동 설정 중 오류 발생: {e.Message}");
        }
    }
}
