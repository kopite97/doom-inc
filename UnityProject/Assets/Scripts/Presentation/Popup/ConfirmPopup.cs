namespace MobileGame.Presentation
{
    public sealed class ConfirmPopup : PopupBase
    {
        protected override bool Accepts(PopupRequest request) => request is DialogPopupRequest dialog && dialog.CancelText != null;
        protected override void Populate(float width) => Rows.Text("Message", Host.Texts.Format(((DialogPopupRequest)Request).Message));
    }
}
