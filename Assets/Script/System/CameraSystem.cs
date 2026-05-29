using QFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace UG20260527
{
    public interface ICameraSystem : ISystem
    {
        public void SetTarget(Transform target);
    }
    public class CameraSystem : AbstractSystem, ICameraSystem
    {
        // 目标点
        private Transform Target;
        private Vector3 Postion;

        protected override void OnInit()
        {
            PublicMono.Instance.OnLateUpdate += LateUpdate;
        }

        private void LateUpdate()
        {
            if (Target == null) return;
            Postion.x = Target.position.x;
            Postion.y = Target.position.y;
            Postion.z = Camera.main.transform.position.z;
            Camera.main.transform.position = Postion;
        }

        void ICameraSystem.SetTarget(Transform target)
        {
            Target = target; 
        }
    }
}
