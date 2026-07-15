using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UG20260527
{
    public class FileTool
    {
        // 复制 HybirdCLR热更新dll 到 Addressables资源路径
        [MenuItem("MyTool/1-CopyHotUpdateDll")]
        public static void CopyHotUpdateDll()
        {
            // 项目根目录
            string projectRootDirectory = Directory.GetParent(Application.dataPath).FullName;

            // 操作路径
            string sourcePath = $@"{projectRootDirectory}\HybridCLRData\HotUpdateDlls\StandaloneWindows64\HotUpdate.dll";
            string targetDirectory = $@"{projectRootDirectory}\Assets\Res_HotUpdate\Assembly\StandaloneWindows64";
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

        // 复制 Addressables远程热更资源包 到 LocalServer
        // 没用，需要复制的文件 被 Unity编辑器 占用
        //[MenuItem("MyTool/2-CopyHotDataToServer")]
        public static void CopyHotDataToServer()
        {
            // 项目根目录
            string projectRootDirectory = Directory.GetParent(Application.dataPath).FullName;

            // 热更新数据路径
            string sourcePath = $@"{projectRootDirectory}\ServerData\StandaloneWindows64";
            string targetPath = $@"{projectRootDirectory}\MyLocalServer\StandaloneWindows64";

            // 源路径是否存在
            if(!Directory.Exists(sourcePath))
            {
                Debug.LogWarning($@"源路径：{sourcePath} 不存在");
                return;
            }

            // 目标路径是否存在
            if(!Directory.Exists(targetPath))
            {
                // 新建目标路径
                Directory.CreateDirectory(targetPath);
            }

            // 将 源路径下所有文件 copy到 目标路径
            DirectoryInfo files = new DirectoryInfo(sourcePath);
            foreach(FileInfo file in files.GetFiles())
            {
                file.CopyTo(Path.Combine(sourcePath, file.Name), true );
            }
            Debug.Log($"文件复制成功，路径：{targetPath}");

        }


    }
}