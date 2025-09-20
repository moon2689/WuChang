using System;
using System.IO;
using YooAsset.Editor;

public class Collect_InvalidScenes : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        string ext = Path.GetExtension(data.AssetPath);
        if (ext != ".unity")
            return false;

        string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
        return fileName == "SLauncher" || fileName == "Empty" || fileName == "MEPDesert";
    }
}