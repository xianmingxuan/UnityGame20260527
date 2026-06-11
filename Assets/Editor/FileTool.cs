using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UG20260527
{
    public class FileTool
    {
        [MenuItem("MyTool/CopyHotUpdateDll")]
        public static void CopyHotUpdateDll()
        {
            // 项目根目录
            string projectRootDirectory = Directory.GetParent(Application.dataPath).FullName;

            // 操作路径
            string sourcePath = $@"{projectRootDirectory}\HybridCLRData\HotUpdateDlls\StandaloneWindows64\HotUpdate.dll";
            string targetDirectory = $@"{projectRootDirectory}\Assets\AddressablesAsset\Assembly\StandaloneWindows64";
            string Rename = "HotUpdate.dll.bytes";

            if (string.IsNullOrWhiteSpace(sourcePath) || !File.Exists(sourcePath))
            {
                Debug.LogWarning($"源路径 {sourcePath} 不存在");
                return;
            }

            if(string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
            {
                Debug.LogWarning($"目标路径 {targetDirectory} 不存在");
                return;
            }

            string targetPath = Path.Combine(targetDirectory, Rename);
            File.Copy(sourcePath, targetPath, true );
            AssetDatabase.Refresh();  // 刷新Unity的资源库
            Debug.Log($"文件复制成功，路径：{targetPath}");
        }
    }
}