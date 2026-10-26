using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MobileGame.EnvironmentTests
{
    public class ProjectEnvironmentTests
    {
        [Test]
        public void ApprovedEditorAndroidAndUniversal2DConfigurationAreReady()
        {
            const string scenePath = "Assets/Scenes/SampleScene.unity";
            const string pipelinePath = "Assets/Settings/UniversalRP.asset";
            const string rendererPath = "Assets/Settings/Renderer2D.asset";

            Assert.That(Application.unityVersion, Is.EqualTo("6000.3.27f1"),
                "Run the project with the approved Unity Editor version.");
            Assert.That(EditorUserBuildSettings.activeBuildTarget, Is.EqualTo(BuildTarget.Android),
                "The active project target must be Android.");
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath), Is.Not.Null,
                "The template's playable scene must exist and import successfully.");
            Assert.That(EditorBuildSettings.scenes.Any(scene => scene.enabled && scene.path == scenePath),
                Is.True, "SampleScene must be enabled in the project's build scene list.");

            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(rendererPath);
            Assert.That(pipeline, Is.Not.Null, "The Universal RP asset must load as a URP asset.");
            Assert.That(renderer, Is.Not.Null, "The 2D Renderer asset must load as Renderer2DData.");
            Assert.That(QualitySettings.renderPipeline ?? GraphicsSettings.defaultRenderPipeline,
                Is.EqualTo(pipeline), "The current quality level must use the template URP asset.");

            using (var serializedPipeline = new SerializedObject(pipeline))
            {
                var defaultRenderer = serializedPipeline.FindProperty("m_DefaultRendererIndex");
                Assert.That(defaultRenderer, Is.Not.Null, "The URP default renderer index must be available.");
                var index = defaultRenderer.intValue;
                Assert.That(index, Is.InRange(0, pipeline.rendererDataList.Length - 1),
                    "The default renderer index must reference a configured renderer.");
                Assert.That(pipeline.rendererDataList[index], Is.EqualTo(renderer),
                    "The default renderer must be the template's 2D Renderer.");
            }
        }
    }
}
