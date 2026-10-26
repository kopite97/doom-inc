using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace MobileGame.EnvironmentTests
{
    public class SampleSceneExecutionTests
    {
        [UnityTest]
        public IEnumerator SampleSceneUsesUniversal2DAndAdvancesFramesAndTime()
        {
            const string scenePath = "Assets/Scenes/SampleScene.unity";
            var previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;

            try
            {
                Assert.That(Application.isPlaying, Is.True, "This check must run in Play Mode.");
                var load = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
                Assert.That(load, Is.Not.Null, "SampleScene must be available for runtime loading.");
                var deadline = Time.realtimeSinceStartupAsDouble + 10;
                while (!load.isDone && Time.realtimeSinceStartupAsDouble < deadline)
                    yield return null;
                Assert.That(load.isDone, Is.True, "SampleScene did not finish loading within 10 seconds.");

                // Allow the loaded scene to render before inspecting its active render pipeline.
                yield return null;
                var scene = SceneManager.GetActiveScene();
                Assert.That(scene.path, Is.EqualTo(scenePath), "The expected scene must be active.");
                var camera = Camera.main;
                Assert.That(camera, Is.Not.Null, "SampleScene must have an active Main Camera.");
                Assert.That(camera.gameObject.scene, Is.EqualTo(scene), "The camera must belong to SampleScene.");
                Assert.That(camera.orthographic, Is.True, "The template camera must use 2D projection.");
                var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
                Assert.That(cameraData, Is.Not.Null, "The camera must have its URP settings component.");
                Assert.That(RenderPipelineManager.currentPipeline, Is.TypeOf<UniversalRenderPipeline>(),
                    "Play Mode must instantiate the Universal Render Pipeline.");
                Assert.That(cameraData.scriptableRenderer, Is.Not.Null, "The camera renderer must initialize.");
                Assert.That(cameraData.scriptableRenderer.GetType().FullName,
                    Is.EqualTo("UnityEngine.Rendering.Universal.Renderer2D"),
                    "The camera must actually use the 2D Renderer.");

                var firstFrame = Time.frameCount;
                var firstTime = Time.timeAsDouble;
                yield return null;
                yield return null;
                Assert.That(Time.frameCount, Is.GreaterThan(firstFrame), "The player loop must advance frames.");
                Assert.That(Time.timeAsDouble, Is.GreaterThan(firstTime), "Game time must advance between frames.");
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                Application.runInBackground = previousRunInBackground;
            }
        }
    }
}
