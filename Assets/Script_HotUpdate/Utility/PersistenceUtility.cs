using QFramework;
using System;
using System.IO;
using UnityEngine;

namespace UG20260527
{

    public interface IPersistenceUtility : IUtility
    {
        /// <summary>
        /// 序列化 路径 = 目录 + 文件名(要加.json)
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="directory">目录</param>
        /// <param name="fileName">文件名（要带.json后缀）</param>
        public void Serialize(object data, string directory, string fileName);
        /// <summary>
        /// 反序列化 路径 = 目录 + 文件名(要加.json)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public object Deserialize(Type type, string directory, string fileName);
        /// <summary>
        /// 反序列化 路径 = 目录 + 文件名(要加.json)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Deserialize<T>(out T result, string directory, string fileName);

        /// <summary>
        /// 序列化 从 JsonString
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        public void SerializeFromJsonString(string jsonString, string directory, string fileName);
        /// <summary>
        /// 反序列化 为 JsonString
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string DeserializeToJsonString(string directory, string fileName);

        /// <summary>
        /// Json 转 数据
        /// </summary>
        /// <param name="jsonString"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public object FromJson(string jsonString, Type type);
        /// <summary>
        /// Json 转 数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public T FromJson<T>(string jsonString);
        /// <summary>
        /// 数据 转 json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string ToJson(object data);

    }

    /// <summary>
    /// 持久化工具
    /// </summary>
    public class PersistenceUtility : IPersistenceUtility
    {
        // 序列化
        public void Serialize(object data, string directory, string fileName)
        {
            // 路径 = 目录 + 文件名
            string savePath = Path.Combine(directory, fileName);

            // Json
            string jsonString = JsonUtility.ToJson(data, true);
            // 若目录不存在 创建目录
            if(!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            // 写入文件
            File.WriteAllText(savePath, jsonString);
        }

        // 反序列化
        public object Deserialize(Type type, string directory, string fileName)
        {
            // 路径 = 目录 + 文件名
            string savePath = Path.Combine(directory, fileName);

            // 若目录不存在 创建目录
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            // 若路径不存在 直接返回null
            if(!File.Exists(savePath)) return null;

            // 读取文件
            string jsonString = File.ReadAllText(savePath);
            // json转data
            return JsonUtility.FromJson(jsonString, type);
        }

        // 反序列化
        public bool Deserialize<T>(out T result, string directory, string fileName)
        {
            // 路径 = 目录 + 文件名
            string savePath = Path.Combine(directory, fileName);

            // 若目录不存在 创建目录
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            // 若路径不存在 直接返回null
            if (!File.Exists(savePath))
            {
                result = default(T);
                return false;
            }

            // 读取文件
            string jsonString = File.ReadAllText(savePath);
            // json转data
            result = JsonUtility.FromJson<T>(jsonString);
            return true;
        }


        // 序列化 从 JsonString
        public void SerializeFromJsonString(string jsonString, string directory, string fileName)
        {
            // 路径 = 目录 + 文件名
            string savePath = Path.Combine(directory, fileName);

            // 若目录不存在 创建目录
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            // 写入文件
            File.WriteAllText(savePath, jsonString);
        }

        // 反序列化 为 JsonString
        public string DeserializeToJsonString(string directory, string fileName)
        {
            // 路径 = 目录 + 文件名
            string savePath = Path.Combine(directory, fileName);

            // 若目录不存在 创建目录
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            // 若路径不存在 直接返回null
            if (!File.Exists(savePath)) return null;

            // 读取文件 返回
            return File.ReadAllText(savePath);
        }


        // Json 转 数据
        public object FromJson(string jsonString, Type type)
        {
            return JsonUtility.FromJson(jsonString, type);
        }
        
        // Json 转 数据
        public T FromJson<T>(string jsonString)
        {
            return JsonUtility.FromJson<T>(jsonString);
        }

        // 数据 转 json
        public string ToJson(object data)
        {
            return JsonUtility.ToJson(data);
        }
    }
}
