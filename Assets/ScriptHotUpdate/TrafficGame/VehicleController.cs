using Codice.CM.Common;
using QFramework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UG20260527
{
    public class VehicleController : MonoBehaviour, IController
    {
        public bool _isMoving;
        public List<Vector3> _path;
        public float speed = 10f;


        void Start()
        {

        }

        void Update()
        {
            UpdateMovement();
        }

        public void Init(List<Vector3> path, float speed, bool isMoving = true)
        {
            this.speed = speed;
            this._isMoving = isMoving;
            _path = path;
            // 位置和旋转
            transform.position = path[0];
            transform.localRotation = Quaternion.LookRotation((path[1] - path[0]).normalized, Vector3.up);
        }


        private int _targetIndex = 0;
        private Vector3 _tempDis = Vector3.zero;
        private Vector3 _tempDir = Vector3.zero;
        public void UpdateMovement()
        {
            if (_isMoving == false) return;
            if (_path == null || _targetIndex >= _path.Count) return;
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


        void OnDisable()
        {
            this._isMoving = false;
            this._path = null;
            _targetIndex = 0;
            _tempDis = Vector3.zero;
            _tempDir = Vector3.zero;

        }





        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}