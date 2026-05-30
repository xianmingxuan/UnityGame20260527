using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using QFramework;


namespace UG20260527
{
    public class Player : MonoBehaviour, IController
    {
        Rigidbody2D rig2D;
        Vector2 moveValue;
        bool isMoveTrigger = false;

        [Tooltip("移动速度")]
        public float moveSpeed = 3.0f;


        void Awake()
        {
            // 刚体引用
            rig2D = GetComponent<Rigidbody2D>();
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

        }

        void FixedUpdate()
        {
            // 移动输入
            if (isMoveTrigger) rig2D.velocity = new Vector2(moveValue.x * moveSpeed, rig2D.velocity.y);
        }


        /* ---------------------------------------------------------------- 输入 -------------------------------------------------------- */

        // Move按下
        public void OnMovePerformed(InputAction.CallbackContext context)
        {
            isMoveTrigger = true;
            moveValue = context.ReadValue<Vector2>();
        }

        // Move抬起
        public void OnMoveCanceled(InputAction.CallbackContext context)
        {
            isMoveTrigger = false;
            moveValue = context.ReadValue<Vector2>();
        }

        // Jump按下
        void OnJumpStarted(InputAction.CallbackContext context)
        {
            rig2D.velocity = new Vector2(rig2D.velocity.x, 10f);
        }

        // Shoot按下
        void OnShootStarted(InputAction.CallbackContext context)
        {
            // 发送命令，增加分数
            //this.SendCommand<AddScoreCommand>();
        }

        public IArchitecture GetArchitecture()
        {
            return UnityGame20260527.Interface;
        }
    }
}

