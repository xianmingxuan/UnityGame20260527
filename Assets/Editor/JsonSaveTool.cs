using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UG20260527
{
    // 结构体数据

    // 文件操作观察者
    [InitializeOnLoad]
    public class FileWatcher
    {

        // 监听文件夹路径
        public static string FilePathWatcher = "Assets/AddressablesAsset/Prefabs/TrafficGame";
        // 监听文件后缀名（过滤器）
        public static string FileSuffix = "*.prefab";
        //public static string FileSuffix = "";
        // 操作委托：用于将线程 转回 主线程（线程安全）
        public delegate void OperateDelegate(FileSystemEventArgs e);
        public static OperateDelegate operateDelegate;

        // 系统层 文件操作观察者
        private static FileSystemWatcher _watcher;
        //private delegate void RenameDelegate(RenamedEventArgs e);
        //private static RenameDelegate _renameDelegate;


        static FileWatcher()
        {
            InitFileWatcher();
        }


        private static void InitFileWatcher()
        {
            Debug.Log($"InitFileWatcher - {FilePathWatcher} - {FileSuffix}");

            // 初始化 FileSystemWatcher
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher();
                _watcher.BeginInit();
                _watcher.EnableRaisingEvents = true;
                _watcher.NotifyFilter = NotifyFilters.FileName;
                if(!string.IsNullOrEmpty(FileSuffix)) _watcher.Filter = FileSuffix;
                _watcher.Path = $"{Directory.GetCurrentDirectory()}/{FilePathWatcher}";
                _watcher.Created += new FileSystemEventHandler(Operate);
                _watcher.Deleted += new FileSystemEventHandler(Operate);
                //_watcher.Changed += new FileSystemEventHandler(Operate);
                //_watcher.Renamed += new RenamedEventHandler(Renamed);
                _watcher.EndInit();
            }

            // 初始化 委托
            if (operateDelegate == null)
            {
                operateDelegate = new OperateDelegate(OnOperate);
            }
        }

        // 其它线程
        private static void Operate(object sender, FileSystemEventArgs e)
        {
            // 其它线程：执行委托 转回 主线程
            operateDelegate?.Invoke(e);
        }

        // 主线程
        private static void OnOperate(FileSystemEventArgs e)
        {
            // 主线程：执行UnityAPI
            Debug.Log($"{e.ChangeType} - {e.Name} - {e.FullPath}");
        }
    }


    [InitializeOnLoad]
    public class JsonSaveTool : EditorWindow
    {
        // 需要观察的 Json文件路径
        [SerializeField] private List<string> _stringList = new List<string>();

        // 文件观察值者 数组
        private static List<FileWatcher> FileWatchers;


        // 滚动视图pos
        private Vector2 scrollPos;

        static JsonSaveTool()
        {
            //Debug.Log("45654665");
        }



        [MenuItem("MyTool/JsonSaveTool")]
        private static void ShowWindow()
        {
            JsonSaveTool tool = EditorWindow.GetWindow<JsonSaveTool>();
            tool.Show();
        }

        private void OnEnable()
        {
            // 读取
            _stringList.Clear();
            int count = PlayerPrefs.GetInt("JsonSaveTool/_stringList.Count");
            for(int i = 0; i < count; i++)
            {
                _stringList.Add(PlayerPrefs.GetString($"JsonSaveTool/_stringList[{i}]"));
            }
        }

        private void OnDestroy()
        {
            // 保存
            PlayerPrefs.SetInt("JsonSaveTool/_stringList.Count", _stringList.Count);
            for (int i = 0; i < _stringList.Count; i++)
            {
                PlayerPrefs.SetString($"JsonSaveTool/_stringList[{i}]", _stringList[i]);
            }
        }

        private void OnGUI()
        {
            // 标题
            EditorGUILayout.LabelField("读取 下列文件夹的文件路径，将这些文件路径 序列化为 json文件", EditorStyles.boldLabel);

            // 开启 垂直框
            EditorGUILayout.BeginVertical(GUILayout.Height(300));

            // 开启 滚动视图
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            // 显示stringList 的 可编辑文本框
            for(int i = 0; i < _stringList.Count; i++)
            {
                // 开启 水平框
                EditorGUILayout.BeginHorizontal();

                // 可编辑字段
                _stringList[i] = EditorGUILayout.TextField($"FilePath{i}", _stringList[i]);

                // 结束 水平框
                EditorGUILayout.EndHorizontal();
            }

            // 结束 滚动视图
            EditorGUILayout.EndScrollView();

            // 开启 水平框
            EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
            // 添加按钮
            if (GUILayout.Button("Add Item"))
            {
                _stringList.Add("New Item");
            }
            // 删除按钮
            if (GUILayout.Button("Remove Item"))
            {
                if(_stringList.Count > 0) _stringList.RemoveAt(_stringList.Count - 1);
            }
            // 结束 水平框
            EditorGUILayout.EndHorizontal();

            // 结束 垂直框
            EditorGUILayout.EndVertical();
        }
    }
}
