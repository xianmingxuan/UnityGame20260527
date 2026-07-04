using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace UG20260527
{
    public class VehicleController : MonoBehaviour, IController
    {
        /* ------------------------------------------------------------------------- 属性 ---------------------------------------------------------------------------- */

        [Tooltip("碰撞盒")] public BoxCollider boxCollider;
        [Tooltip("箭头-左转")] public GameObject arrow_Left;
        [Tooltip("箭头-右转")] public GameObject arrow_Right;
        [Tooltip("箭头-直行")] public GameObject arrow_Straight;

        [Tooltip("是否移动")] public bool isMoving;
        [Tooltip("移动速度")] public float speed = 10f;


        // 数据
        private ITrafficGameModel _gameModel;
        // 路径
        public List<Vector3> _path;


        /* ------------------------------------------------------------------------- 生命周期 ---------------------------------------------------------------------------- */

        private void Awake()
        {
            _gameModel = this.GetModel<ITrafficGameModel>();
        }

        void Update()
        {
            UpdateMovement();
        }

        // 车辆碰撞时
        void OnTriggerEnter(Collider other)
        {
            var comp = other.gameObject.GetComponentInParent<VehicleController>();
            if (comp)
            {
                //Debug.Log($"{gameObject.name} 撞到了 {comp.gameObject.name}");
                RecycleSelf();
                comp.RecycleSelf();
                _gameModel.HP.Value--;
            }
        }

        void OnDisable()
        {
            // 被回收
            arrow_Left.SetActive(false);
            arrow_Right.SetActive(false);
            arrow_Straight.SetActive(false);

            this.isMoving = false;
            this._path = null;
            _targetIndex = 0;
            _tempDis = Vector3.zero;
            _tempDir = Vector3.zero;
        }


        /* ------------------------------------------------------------------------- API ---------------------------------------------------------------------------- */

        // 初始化
        public void Init(PathsPresetInfo.DirectionType directionType, PathsPresetInfo.MovementType movementType, float speed, bool isMoving = true)
        {
            // 设置 路线
            _path = _gameModel.GetPath(directionType, movementType);
            // 设置 属性
            this.speed = speed;
            this.isMoving = isMoving;
            // 设置 位置和旋转
            transform.position = _path[0];
            transform.localRotation = Quaternion.LookRotation((_path[1] - _path[0]).normalized, Vector3.up);
            // 设置 箭头显示
            switch (movementType)
            {
                case PathsPresetInfo.MovementType.Straight: arrow_Straight.SetActive(true); break;
                case PathsPresetInfo.MovementType.TurnLeft: arrow_Left.SetActive(true); break;
                case PathsPresetInfo.MovementType.TurnRight: arrow_Right.SetActive(true); break;
            }
        }


        private int _targetIndex = 0;
        private Vector3 _tempDis = Vector3.zero;
        private Vector3 _tempDir = Vector3.zero;
        // 移动逻辑
        public void UpdateMovement()
        {
            if (isMoving == false) return;
            if (_path == null || _targetIndex >= _path.Count)
            {
                // 已到达 终点
                RecycleSelf();
                return;
            }
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_path[_targetIndex].x, _path[_targetIndex].z)) < 0.1f)
            {
                _targetIndex++;
                return;
            }

            // 计算 位移目标点
            Vector2 dir = new Vector2(_path[_targetIndex].x, _path[_targetIndex].z) - new Vector2(transform.position.x, transform.position.z); dir.Normalize();
            Vector2 dis = dir * (speed * Time.deltaTime);
            _tempDis.x = dis.x;
            _tempDis.z = dis.y;
            transform.position += _tempDis;

            // 计算 位移目标旋转
            _tempDir.x = dir.x;
            _tempDir.z = dir.y;
            transform.localRotation = Quaternion.LookRotation(_tempDir, Vector3.up);
        }

        // 回收
        public void RecycleSelf()
        {
            this.GetSystem<IResourceSystem>().Recycle(gameObject);
            _gameModel.numberOfRecycledVehicles.Value++;
        }







        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}