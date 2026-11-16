#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using MobileGame.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace MobileGame.SpatialTests
{
    public sealed partial class AppSceneFlowTests
    {
        [UnityTest, Category("BatchArtIntegration")]
        public IEnumerator BatchArtTitleAndLoadingFramesAdvanceWithoutDrivingGameState()
        {
            flow.EnterCurrent(AppScene.Bootstrap); yield return Settled();
            var title=Object.FindFirstObjectByType<TitleView>();
            var room=title.GetComponentsInChildren<Image>().Single(i=>i.sprite!=null&&i.sprite.name.StartsWith("Title"));
            var first=room.sprite;yield return new WaitForSecondsRealtime(.17f);
            Assert.That(room.sprite,Is.Not.SameAs(first));
            Assert.That(room.sprite.name,Does.Contain("Ambient"));Assert.That(room.raycastTarget,Is.False);
            flow.Loading.Show(LoadingScreenKind.Loading,"Preparing test scene");yield return null;
            var decoration=flow.Loading.GetComponentsInChildren<Image>().Single(i=>i.name=="LoadingDecoration");
            Assert.That(decoration.enabled,Is.True);var bubble=decoration.sprite;
            yield return new WaitForSecondsRealtime(.17f);Assert.That(decoration.sprite,Is.Not.SameAs(bubble));
            Assert.That(flow.Laboratory,Is.Null,"Ambient animation cannot create a game session.");
            flow.Loading.Show(LoadingScreenKind.Error,"Test recovery",()=>{});yield return null;
            Assert.That(decoration.enabled,Is.False);flow.Loading.Hide();
            LogAssert.NoUnexpectedReceived();
        }
    }
}
#endif
