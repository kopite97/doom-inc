using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
namespace MobileGame.TestSupport
{
    // Opt-in evidence writer. Survives the test runner's domain reload via SessionState.
    [InitializeOnLoad]
    public sealed class TestResultFile : ICallbacks
    {
        private const string Key = "MobileGame.TestResultFile.Path";
        private static TestResultFile active;
        static TestResultFile() { Restore(); }
        private static void Restore()
        {
            if (active != null || string.IsNullOrEmpty(SessionState.GetString(Key,""))) return;
            active=new TestResultFile();TestRunnerApi.RegisterTestCallback(active);
        }
        public static void Arm(string path)
        {
            SessionState.SetString(Key,Path.GetFullPath(path));Restore();
        }
        public void RunStarted(ITestAdaptor testsToRun) { }
        public void TestStarted(ITestAdaptor test) { }
        public void TestFinished(ITestResultAdaptor result) { }
        public void RunFinished(ITestResultAdaptor result)
        {
            var path=SessionState.GetString(Key,"");
            if(!string.IsNullOrEmpty(path)){Directory.CreateDirectory(Path.GetDirectoryName(path));TestRunnerApi.SaveResultToFile(result,path);}
            SessionState.EraseString(Key);TestRunnerApi.UnregisterTestCallback(this);active=null;
        }
    }
}
