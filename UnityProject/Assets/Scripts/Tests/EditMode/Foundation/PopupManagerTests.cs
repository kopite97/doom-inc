using System;
using System.Collections.Generic;
using MobileGame.Foundation;
using MobileGame.Presentation;
using NUnit.Framework;

namespace MobileGame.Foundation.Tests
{
    public sealed class PopupManagerTests
    {
        private sealed class View : IPopupView
        {
            public Action Opened, Closed;
            public bool Enabled;
            public bool FailOpen, FailClose, FailRefresh;
            public void Open(Action ready) { if (FailOpen) throw new InvalidOperationException("open hook"); Opened = ready; }
            public void Close(Action finished) { if (FailClose) throw new InvalidOperationException("close hook"); Closed = finished; }
            public void SetInteractable(bool enabled) { if (FailRefresh) throw new InvalidOperationException("refresh"); Enabled = enabled; }
        }
        private sealed class Host : IPopupHost
        {
            public readonly List<View> Views = new List<View>();
            public bool Blocked, FailCreate;
            public bool FailOpen;
            public IPopupView Create(PopupRequest r, Action<PopupResult> choose)
            { if (FailCreate) throw new InvalidOperationException("creation failed"); var v = new View { FailOpen = FailOpen }; Views.Add(v); return v; }
            public void Remove(IPopupView view) { Views.Remove((View)view); }
            public void SetBlocked(bool blocked) { Blocked = blocked; }
        }
        private Host host;
        private PopupManager manager;
        private PopupScope scope;
        private readonly LocalizedMessage text = new LocalizedMessage("Common", "confirm");
        [SetUp] public void Setup() { host = new Host(); manager = new PopupManager(host) { Ready = true }; scope = new PopupScope(); }
        [TearDown] public void Cleanup() { manager.Dispose(); scope.Dispose(); }
        private PopupHandle Show(Action yes = null, Action no = null, string key = "test", PopupHandle parent = null)
            => manager.ShowConfirm(parent?.Children ?? scope, key, text, text, yes, no, parent: parent);
        private View Open() { var view = host.Views[host.Views.Count - 1]; view.Opened(); return view; }

        [Test] public void OpenCloseDuplicateAndCallbackReentryHaveOneOwner()
        {
            var calls = 0; PopupHandle next = null;
            var first = Show(() => { calls++; Assert.That(host.Blocked); next = Show(key: "next"); });
            var oldView = Open();
            Assert.That(Show(() => calls += 100), Is.SameAs(first));
            Assert.That(manager.Choose(first, PopupResult.Confirmed));
            Assert.That(manager.Choose(first, PopupResult.Confirmed), Is.False);
            Assert.That(oldView.Enabled, Is.False); oldView.Closed(); oldView.Closed();
            Assert.That(calls, Is.EqualTo(1)); Assert.That(first.Result, Is.EqualTo(PopupResult.Confirmed));
            Assert.That(manager.Top, Is.SameAs(next)); Assert.That(manager.Count, Is.EqualTo(1));
        }
        [Test] public void ConfirmWaitsForForegroundAndRevalidatesScope()
        {
            var calls = 0; var handle = Show(() => calls++); var view = Open();
            manager.Choose(handle, PopupResult.Confirmed); manager.SetForeground(false); view.Closed();
            Assert.That(handle.State, Is.EqualTo(PopupState.WaitingForForeground)); Assert.That(calls, Is.Zero); Assert.That(host.Blocked);
            manager.SetForeground(true); manager.SetForeground(true);
            Assert.That(calls, Is.EqualTo(1)); Assert.That(host.Blocked, Is.False);
        }
        [Test] public void OwnerEndDuringBackgroundDropsAcceptedIntentWithoutUserCancel()
        {
            var calls = 0; var cancel = 0; var handle = Show(() => calls++, () => cancel++); var view = Open();
            manager.Choose(handle, PopupResult.Confirmed); manager.SetForeground(false); view.Closed(); scope.Dispose();
            manager.SetForeground(true);
            Assert.That(handle.Result, Is.EqualTo(PopupResult.OwnerEnded)); Assert.That(calls + cancel, Is.Zero); Assert.That(host.Blocked, Is.False);
        }
        [Test] public void OpeningCancellationCannotBeResurrectedByLateAnimation()
        {
            var handle = Show(); var view = host.Views[0]; handle.Cancel(); view.Opened();
            Assert.That(handle.Result, Is.EqualTo(PopupResult.CallerCanceled)); Assert.That(manager.Count, Is.Zero); Assert.That(host.Blocked, Is.False);
        }
        [Test] public void ChildIsOnlyInteractiveAndParentCancellationEndsBoth()
        {
            var calls = 0; var parent = Show(no: () => calls++); var pv = Open();
            var child = Show(no: () => calls++, parent: parent); var cv = Open();
            Assert.That(pv.Enabled, Is.False); Assert.That(cv.Enabled); Assert.That(manager.Choose(parent, PopupResult.Confirmed), Is.False);
            Assert.That(Show(key: "unrelated").Result, Is.EqualTo(PopupResult.Busy));
            parent.Cancel();
            Assert.That(child.Result, Is.EqualTo(PopupResult.OwnerEnded)); Assert.That(parent.Result, Is.EqualTo(PopupResult.CallerCanceled));
            Assert.That(calls, Is.Zero); Assert.That(host.Blocked, Is.False);
        }
        [Test] public void BackConsumesNondismissibleAndRestoresParentAfterChildClose()
        {
            var parent = Show(); var pv = Open();
            var child = manager.Show(new DialogPopupRequest(parent.Children, "child", text, text, text, parent: parent, canDismiss: false));
            var cv = Open(); Assert.That(manager.Back()); Assert.That(child.State, Is.EqualTo(PopupState.Open));
            manager.Choose(child, PopupResult.Confirmed); cv.Closed();
            Assert.That(manager.Top, Is.SameAs(parent)); Assert.That(pv.Enabled);
        }
        [Test] public void CreationAndCallbackFailureReleaseOwnedLocks()
        {
            host.FailCreate = true; var failed = Show();
            Assert.That(failed.Result, Is.EqualTo(PopupResult.Failed)); Assert.That(host.Blocked, Is.False);
            host.FailCreate = false; var handle = Show(() => throw new InvalidOperationException("callback")); var view = Open();
            manager.Choose(handle, PopupResult.Confirmed); view.Closed();
            Assert.That(handle.IsCompleted); Assert.That(host.Blocked, Is.False); Assert.That(manager.Count, Is.Zero);
        }
        [Test] public void NotReadySuspendedAndExpiredOwnerAreExplicitRejections()
        {
            manager.Ready = false; Assert.That(Show().Result, Is.EqualTo(PopupResult.NotReady));
            manager.Ready = true; manager.SetSuspended(true); Assert.That(Show().Result, Is.EqualTo(PopupResult.Busy));
            manager.SetSuspended(false); scope.Dispose(); Assert.That(Show().Result, Is.EqualTo(PopupResult.OwnerEnded));
            Assert.That(host.Blocked, Is.False);
        }
        [Test] public void ParentCompletionObserverCannotLeakChildScope()
        {
            var parent = Show(); Open(); var child = Show(parent: parent); Open();
            manager.Dispose();
            Assert.That(parent.IsCompleted && child.IsCompleted); Assert.That(host.Views, Is.Empty);
        }
        [Test] public void ViewFailuresRestoreParentAndDoNotLeakLocks()
        {
            var parent = Show(); var parentView = Open();
            host.FailOpen = true; var failed = Show(parent: parent);
            Assert.That(failed.Result, Is.EqualTo(PopupResult.Failed)); Assert.That(parentView.Enabled);
            host.FailOpen = false; var child = Show(parent: parent); var childView = Open();
            childView.FailClose = true; manager.Choose(child, PopupResult.Confirmed);
            Assert.That(child.Result, Is.EqualTo(PopupResult.Failed)); Assert.That(parentView.Enabled);
            parentView.FailRefresh = true; manager.SetForeground(false);
            Assert.That(parent.Result, Is.EqualTo(PopupResult.Failed)); Assert.That(host.Views, Is.Empty); Assert.That(host.Blocked, Is.False);
        }
        [Test] public void SuspensionDefersAcceptedIntentAndDisposeDropsIt()
        {
            var calls = 0; var handle = Show(() => calls++); var view = Open();
            manager.Choose(handle, PopupResult.Confirmed); manager.SetSuspended(true); view.Closed();
            Assert.That(calls, Is.Zero); Assert.That(host.Blocked);
            manager.SetSuspended(false); Assert.That(calls, Is.EqualTo(1));
            handle = Show(() => calls++); view = Open(); manager.Choose(handle, PopupResult.Confirmed);
            manager.SetSuspended(true); view.Closed(); manager.Dispose(); manager.SetSuspended(false);
            Assert.That(calls, Is.EqualTo(1)); Assert.That(handle.Result, Is.EqualTo(PopupResult.OwnerEnded)); Assert.That(host.Blocked, Is.False);
        }
    }
}
