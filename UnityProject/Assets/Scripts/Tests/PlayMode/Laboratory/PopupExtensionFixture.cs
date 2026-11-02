#if UNITY_EDITOR
using MobileGame.Foundation;
using MobileGame.Presentation;

namespace MobileGame.SpatialTests
{
    public sealed class PopupExtensionRequest : PopupRequest
    {
        public PopupExtensionRequest(PopupScope owner)
            : base("TestExtension", owner, "extension", new LocalizedMessage("Common", "settings"), null, true) { }
    }
    public sealed class PopupExtensionFixture : PopupBase
    {
        protected override bool Accepts(PopupRequest request) => request is PopupExtensionRequest;
        protected override void Populate(float width) => Rows.Text("Extension", Host.Texts.Format("Laboratory", "map_help"));
        protected override LocalizedMessage ConfirmLabel => new LocalizedMessage("Common", "confirm");
    }
}
#endif
