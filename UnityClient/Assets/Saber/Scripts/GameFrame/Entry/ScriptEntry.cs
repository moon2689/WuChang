using System.Collections;

namespace Saber.Frame
{
    // 万法归一
    public class ScriptEntry
    {
        public ScriptEntryGame Game { get; private set; } = new();
        public ScriptEntryUI UI { get; private set; } = new();
        public ScriptEntryAsset Asset { get; private set; } = new();
        public ScriptEntryUnity Unity { get; private set; } = new();
        public ScriptEntryConfig Config { get; private set; } = new();
    }
}