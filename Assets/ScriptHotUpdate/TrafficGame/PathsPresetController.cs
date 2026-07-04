using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UG20260527
{
    [System.Serializable]
    public class PathsPresetInfo
    {
        // 路线起始点所处的方位
        public enum DirectionType { North, West, East, South }

        // 路线的行动方向类型
        public enum MovementType { Straight, TurnLeft, TurnRight }


        [Tooltip("路线起始方位")] public DirectionType directionType;
        [Tooltip("路线行动类型")] public MovementType movementType;
        [Tooltip("起始点坐标")]   public Transform startPoint;
        [Tooltip("终点坐标")]     public Transform endPoint;
        [Tooltip("转弯中点坐标")] public Transform midPoint;
        [Tooltip("使用自定义 midPoint->转折点的距离")]
        public bool isCustomDis;
        [Tooltip("midPoint->转折点的距离（控制圆的弧度大小）")]
        public float dis;
    }

    public class PathsPresetController : MonoBehaviour, IController
    {
        [Tooltip("midPoint->转折点的距离（控制圆的弧度大小）")] public float dis = 1f;
        [Tooltip("弯道细分精度")] public int precision = 20;
        [Tooltip("路线预设信息列表")] public List<PathsPresetInfo> pathsPresetInfos = new List<PathsPresetInfo>();

        // 数据
        private ITrafficGameModel _gameModel;
        // 计算后的路线缓存
        private Dictionary<string, List<Vector3>> _pathsCache = new Dictionary<string, List<Vector3>>();


        private void Awake()
        {
            _gameModel = this.GetModel<ITrafficGameModel>();
            CalculateAllPaths();
        }


        // 计算所有预设路径
        private void CalculateAllPaths()
        {
            foreach(var pathInfo in pathsPresetInfos)
            {
                // 单条路线缓存
                List<Vector3> pathCache = new List<Vector3>();

                // 计算
                if(pathInfo.movementType == PathsPresetInfo.MovementType.Straight)
                {
                    // 直线路径
                    pathCache.Add(pathInfo.startPoint.position);
                    pathCache.Add(pathInfo.endPoint.position);
                }
                else
                {
                    // 弯道路径
                    float r = pathInfo.isCustomDis ? pathInfo.dis : dis;
                    pathCache = CalculateTurnPath(
                        pathInfo.startPoint.position,
                        pathInfo.endPoint.position,
                        pathInfo.midPoint.position,
                        r,
                        precision);
                }

                // 加入字典缓存
                _pathsCache.Add(_gameModel.GetKey(pathInfo.directionType, pathInfo.movementType), pathCache);
            }

            // 更新数据
            this.SendCommand(new TrafficGameModel_PathsCache_SetCommand(_pathsCache));
        }

        private List<Vector3> CalculateTurnPath(Vector3 start, Vector3 end, Vector3 mid, float dis, int precision)
        {
            List<Vector3> pathCache = new List<Vector3>();

            // 计算 起点/终点 的方向向量 （x, z）y是竖直方向坐标
            Vector3 v1 = mid - start; v1.y = 0; v1.Normalize();
            Vector3 v2 = end - mid; v2.y = 0; v2.Normalize();

            // v1/v2的法线向量
            Vector3 n1;
            Vector3 n2;
            // 起点向量 叉乘 终点向量，根据叉乘的y轴正负 判断 转弯的方向（顺时针 / 逆时针）
            float cross = v1.x * v2.z - v1.z * v2.x;
            if(cross < 0)
            {
                // 顺时针，法线向量
                n1 = new Vector3(v1.z, 0, -v1.x);
                n2 = new Vector3(v2.z, 0, -v2.x);
            }
            else
            {
                // 逆时针，法线向量
                n1 = new Vector3(-v1.z, 0, v1.x);
                n2 = new Vector3(-v2.z, 0, v2.x);
            }
            // 转折点坐标
            Vector3 turnStart = mid - v1 * dis;
            Vector3 turnEnd = mid + v2 * dis;
            // 计算 圆心坐标 + 半径
            float t = ((turnEnd.x - turnStart.x) * n2.z - (turnEnd.z - turnStart.z) * n2.x) /
                      (n1.x * n2.z - n1.z * n2.x);
            float x = turnStart.x + t * n1.x;
            float z = turnStart.z + t * n1.z;
            Vector3 center = new Vector3(x, 0f, z);
            float radius = Vector3.Distance(center, turnStart);

            // 计算转弯角度
            float startAngle = Mathf.Atan2(turnStart.z - center.z, turnStart.x - center.x);
            float endAngle = Mathf.Atan2(turnEnd.z - center.z, turnEnd.x - center.x);

            // 修正 end转弯角度
            if (cross < 0)
            {
                // 正常情况：顺时针，endAngle < startAngle
                if(endAngle > startAngle)
                {
                    endAngle -= 2f * Mathf.PI;
                }
            }
            else
            {
                // 正常情况：逆时针，endAngle > startAngle
                if (endAngle < startAngle)
                {
                    endAngle += 2f * Mathf.PI;
                }
            }

            // 添加 star -> turnStart 的直线坐标点
            pathCache.Add(start);
            pathCache.Add(turnStart);

            // 插值角度，计算弯道坐标点
            for(int i = 1; i <= precision; i++)
            {
                float tt = (float)i / precision;
                float ang = Mathf.Lerp(startAngle, endAngle, tt);
                float xx = center.x + Mathf.Cos(ang) * radius;
                float zz = center.z +  Mathf.Sin(ang) * radius;
                float y = Mathf.Lerp(turnStart.y, turnEnd.y, tt);
                pathCache.Add(new Vector3(xx, y, zz));
            }

            // 添加 turnEnd -> end 的直线坐标点
            pathCache.Add(turnEnd);
            pathCache.Add(end);

            // 返回单条路线
            return pathCache;
        }

        



#if UNITY_EDITOR
        //private void OnDrawGizmosSelected()
        //{

        //}
        private void OnDrawGizmos()
        {
            if (pathsPresetInfos.Count <= 0) return;
            foreach (var pathInfo in pathsPresetInfos)
            {
                // 计算
                if (pathInfo.movementType == PathsPresetInfo.MovementType.Straight)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(pathInfo.startPoint.position, pathInfo.endPoint.position);
                }
                else
                {
                    // 弯道路径
                    float r = pathInfo.isCustomDis ? pathInfo.dis : dis;
                    List<Vector3> pathCache = CalculateTurnPath(
                        pathInfo.startPoint.position,
                        pathInfo.endPoint.position,
                        pathInfo.midPoint.position,
                        r,
                        precision);

                    Gizmos.color = Color.yellow;
                    for (int i = 0; i < pathCache.Count - 1; i++)
                    {
                        if (i + 1 >= pathCache.Count) continue;
                        Gizmos.DrawLine(pathCache[i], pathCache[i + 1]);
                    }
                }
            }
        }
#endif


        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}