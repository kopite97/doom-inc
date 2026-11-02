using MobileGame.Foundation;
using MobileGame.Gameplay;
using UnityEngine.UI;
using System;

namespace MobileGame.Presentation
{
    public sealed partial class LaboratoryView
    {
        public string FormatMessage(LocalizedMessage message) => localization.Format(message);
        private string L(string key, params TextArgument[] arguments) => localization.Format("Laboratory", key, arguments);
        private string R(string key, params TextArgument[] arguments) => localization.Format("Results", key, arguments);
        private static LocalizedMessage M(string key, params TextArgument[] arguments) => new LocalizedMessage("Laboratory", key, arguments);
        private static TextArgument A(string name, object value) => new TextArgument(name, value);
        private static LocalizedMessage C(string key, params TextArgument[] arguments) => new LocalizedMessage("Common", key, arguments);
        private static LocalizedMessage DefinitionName(ThingData definition) => new LocalizedMessage("Definitions", definition.DisplayNameKey);
        private LocalizedMessage ThingMessage(Guid id) => Simulation != null && Simulation.Registry.TryGet(id, out var thing)
            ? M("thing_label", A("name", DefinitionName(thing.Definition)), A("order", thing.CreationOrder)) : M("no_target");
        public LocalizedMessage ReportMessage(InvasionReport report) => new LocalizedMessage("Reports", "entry",
            A("sequence", report.Sequence), A("invader", ThingMessage(report.InvaderId)),
            A("event", new LocalizedMessage("Reports", report.IsMajor ? "major" : "normal")), A("score", report.Score), A("seconds", report.Time));
        public LocalizedMessage StatusMessage(FacilityStatus status) => status.HasProgress
            ? M("status_progress", A("state", LaboratoryStatusText.Short(status)), A("percent", status.ProgressFraction * 100))
            : LaboratoryStatusText.Short(status);
        private string fixedTextLanguage;
        private void RefreshFixedText()
        {
            var language = localization.Service.CurrentLanguage;
            if (fixedTextLanguage == language) return;
            fixedTextLanguage = language;
            titleText.text = L("heading");
            SetLabel(gameMenuButton, "menu"); SetLabel(zoomOutButton, "zoom_out"); SetLabel(zoomInButton, "zoom_in"); SetLabel(fitButton, "fit");
            SetLabel(rotateButton, "rotate"); SetLabel(confirmButton, "confirm_placement"); SetLabel(cancelButton, "cancel");
            SetLabel(buildMenuButton, "build"); SetLabel(creatureMenuButton, "creatures"); SetLabel(invasionMenuButton, "invasion");
            SetLabel(detailsButton, "details"); SetLabel(closeButton, "close");
        }
        private void SetLabel(UiButton button, string key) => button.GetComponentInChildren<Text>().text = L(key);
    }
}
