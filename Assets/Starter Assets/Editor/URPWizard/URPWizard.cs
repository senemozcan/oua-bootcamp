using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Rendering;
#if USE_URP
using UnityEngine.Rendering.Universal;
#endif

public class URPWizard : EditorWindow
{
    [InitializeOnLoadMethod]
    static void OnInitialize()
    {
        URPCheck();
    }

    static void URPCheck()
    {
        if (GraphicsSettings.currentRenderPipeline != null) 
            return;

        var request = Client.List();
        while (!request.IsCompleted) { }

        if (request.Status != StatusCode.Success) 
            return;
        
        if (request.Result.All(info => info.name != "com.unity.render-pipelines.universal"))
        {
            var addRequest = Client.Add("com.unity.render-pipelines.universal");
            
            while (!addRequest.IsCompleted) { }
                    
            Client.Resolve();
        }
        else
        {
            FindAndAssignPipeline();
        }
    }

#if USE_URP
    static void FindAndAssignPipeline()
    {
        var existingPipelines = AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset");

        if (existingPipelines.Length == 0)
        {
            return;
        }
        
        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(existingPipelines[0]));
        GraphicsSettings.defaultRenderPipeline = pipeline;
    }
    
    class PipelineAssetProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            //if we have no pipeline set, we try to find one as one may have been imported
            if (GraphicsSettings.currentRenderPipeline != null) 
                return;
            
            FindAndAssignPipeline();
        }
    }
#else
    static void FindAndAssignPipeline(){}
#endif
}
