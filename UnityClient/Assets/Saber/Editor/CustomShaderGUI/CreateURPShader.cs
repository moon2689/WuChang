using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace TATools
{
    public class CreateURPShader
    {
        const string ShaderTemplatePath = "Assets/FashionBeat_ArtSVN/Asset/NewRoleAsset/TA/Shaders/Lit/PBR/CombineSkinMesh/Opaque_StandardPBR.shader";

        [MenuItem("Assets/Create/Shader/Standard PBR Shader")]
        static void CreatURPShaderTemplate()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<URPShadertAsset>(),
            GetSelectedPathOrFallback() + "/Opaque_StandardPBR.shader",
            null,
           ShaderTemplatePath);
        }


        //获取选择的路径
        static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UObject obj in Selection.GetFiltered(typeof(UObject), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }


    class URPShadertAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            UObject obj = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(obj);
        }

        internal static UObject CreateScriptAssetFromTemplate(string pathName, string resourceFile)
        {
            string fullPath = Path.GetFullPath(pathName);
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();//读取模板内容
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            text = Regex.Replace(text, "#NAME#", fileNameWithoutExtension);//将模板的#NAME# 替换成文件名

            //写入文件，并导入资源
            UTF8Encoding encoding = new UTF8Encoding(true, false);
            StreamWriter streamWriter = new StreamWriter(fullPath, false, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UObject));
        }

    }
}