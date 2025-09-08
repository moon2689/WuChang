using System.IO;
using YooAsset.Editor;

public class AddressByEveryDirName : IAddressRule
{
    public string GetAssetAddress(AddressRuleData data)
    {
        string path = data.AssetPath.Substring(data.CollectPath.Length + 1);
        string ext = Path.GetExtension(data.AssetPath);
        path = path.Substring(0, path.Length - ext.Length);
        string address = path.Replace('/', '@');
        address = address.ToLower();
        return address;
    }
}

public class AddressByRootFolder : IAddressRule
{
    public string GetAssetAddress(AddressRuleData data)
    {
        string rootFolderName = Path.GetFileNameWithoutExtension(data.CollectPath);
        string fileName = Path.GetFileNameWithoutExtension(data.AssetPath);
        string address = $"{rootFolderName}@{fileName}";
        address = address.ToLower();
        return address;
    }
}