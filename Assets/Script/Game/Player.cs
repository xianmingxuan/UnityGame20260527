using QFramework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Color = UnityEngine.Color;


namespace UG20260527
{
    public class Player : MonoBehaviour, IController
    {
        /* ---------------------------------------------------------------- 属性 -------------------------------------------------------- */

        // Physics
        private Rigidbody2D Rig2D;
        private BoxCollider2D BoxCollider2D;

        // Move
        private float MoveSpeed = 3.0f;  // 移动速度
        private float AccSpeed = 0.7f;  // 加速度
        private float DecSpeed = 0.15f;  // 减速度
        private Vector2 MoveInputValue;
        private bool IsMoveInput = false;

        // Jump
        private bool IsJumpInput = false;
        private float JumpForce = 10f;


        /* ---------------------------------------------------------------- 生命周期 -------------------------------------------------------- */

        void Awake()
        {
            // 初始化自身引用
            Rig2D = GetComponent<Rigidbody2D>();
            BoxCollider2D = GetComponentInChildren<BoxCollider2D>();
        }

        private void Start()
        {
            // 摄像机系统
            this.GetSystem<ICameraSystem>().SetTarget(this.transform);

            // 输入系统
            this.GetSystem<IInputSystem>().SwitchActionMap("PlayerMap");
            this.RegisterEvent<InputActionEvent>(e =>
            {
                if (!string.IsNullOrEmpty(e.mapName) && e.mapName == "PlayerMap")
                {
                    switch (e.actionName)
                    {
                        case "Move":
                            if (e.context.performed) OnMovePerformed(e.context);
                            else if (e.context.canceled) OnMoveCanceled(e.context);
                            break;

                        case "Jump":
                            if (e.context.started) OnJumpStarted(e.context);
                            break;

                        case "Shoot":
                            if (e.context.started) OnShootStarted(e.context);
                            break;
                    }
                }
            }).UnRegisterWhenGameObjectDestroyed(gameObject);

            // 测试
            //await this.GetSystem<IUISystem>().PushPanel(new HUDPanel());
            //await Task.Delay(7000);
            //await this.GetSystem<IUISystem>().PushPanel(new TestPanel());
        }

        // 物理帧更新 缓存
        Vector2 LastFixedVelocity;  // 上一个物理帧速度

        void FixedUpdate()
        {
            // Move
            if (IsMoveInput)
            {
                // 加速度
                LastFixedVelocity.x = Mathf.Clamp(LastFixedVelocity.x + MoveInputValue.x * AccSpeed, -MoveSpeed, MoveSpeed);
                float x = Time.fixedDeltaTime * LastFixedVelocity.x;
                transform.position = new Vector3(transform.position.x + x, transform.position.y, transform.position.z);
            }
            else
            {
                // 减速度
                LastFixedVelocity.x = Mathf.MoveTowards(LastFixedVelocity.x, 0, DecSpeed);
                float x = Time.fixedDeltaTime * LastFixedVelocity.x;
                transform.position = new Vector3(transform.position.x + x, transform.position.y, transform.position.z);
            }

            // Jump
            if (IsJumpInput)
            {
                IsJumpInput = false;
                Rig2D.velocity = new Vector2(Rig2D.velocity.x, JumpForce);
            }
        }


        /* ---------------------------------------------------------------- 输入 -------------------------------------------------------- */

        // Move按下
        public void OnMovePerformed(InputAction.CallbackContext context)
        {
            IsMoveInput = true;
            MoveInputValue = context.ReadValue<Vector2>();
        }

        // Move抬起
        public void OnMoveCanceled(InputAction.CallbackContext context)
        {
            IsMoveInput = false;
            MoveInputValue = context.ReadValue<Vector2>();
        }

        // Jump按下
        void OnJumpStarted(InputAction.CallbackContext context)
        {
            // 检测是否在地面上
            Vector2 point = (Vector2)transform.position + Vector2.down * BoxCollider2D.size.y;
            if (Physics2D.OverlapBox(point, BoxCollider2D.size, 0f, LayerMask.GetMask("Ground")))
            {
                IsJumpInput = true;
            }
        }

        // 调试
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube((Vector2)transform.position + Vector2.down * BoxCollider2D.size.y, BoxCollider2D.size);
        }

        // Shoot按下
        void OnShootStarted(InputAction.CallbackContext context)
        {
            // 发送命令，增加分数
            //this.SendCommand<AddScoreCommand>();
        }



        /* ---------------------------------------------------------------- QFramework -------------------------------------------------------- */

        public IArchitecture GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}

