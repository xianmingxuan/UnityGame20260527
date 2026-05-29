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

        GameControls gameControls;
        Vector2 moveValue;
        bool isMoveTrigger = false;
        [Tooltip("移动速度")]
        public float moveSpeed = 3.0f;

        void Awake()
        {
            // 刚体引用
            rig2D = GetComponent<Rigidbody2D>();
            // 输入控制引用（全局唯一）
            gameControls = new GameControls();
            gameControls.GamePlayMap.Enable();
        }

        private void Start()
        {
            this.GetSystem<ICameraSystem>().SetTarget(this.transform);
        }

        void OnEnable()
        {
            // 绑定输入
            gameControls.GamePlayMap.Move.performed += OnMovePerformed;
            gameControls.GamePlayMap.Move.canceled += OnMoveCanceled;
            gameControls.GamePlayMap.Jump.started += OnJumpStarted;
            gameControls.GamePlayMap.Shoot.started += OnShootStarted;
        }

        void FixedUpdate()
        {
            // 移动输入
            if (isMoveTrigger) rig2D.velocity = new Vector2(moveValue.x * moveSpeed, rig2D.velocity.y);
        }

        void OnDisable()
        {
            // 解绑输入
            gameControls.GamePlayMap.Move.performed -= OnMovePerformed;
            gameControls.GamePlayMap.Move.canceled -= OnMoveCanceled;
            gameControls.GamePlayMap.Jump.started -= OnJumpStarted;
            gameControls.GamePlayMap.Shoot.started -= OnShootStarted;
        }


        /* ---------------------------------------------------------------- 输入 ------------------------------------------------------ */

        // 移动输入执行时（按下？）
        public void OnMovePerformed(InputAction.CallbackContext context)
        {
            isMoveTrigger = true;
            moveValue = context.ReadValue<Vector2>();

        }

        // 移动输入取消时（抬起？）
        public void OnMoveCanceled(InputAction.CallbackContext context)
        {
            isMoveTrigger = false;
            moveValue = context.ReadValue<Vector2>();
        }

        // 跳跃输入
        void OnJumpStarted(InputAction.CallbackContext context)
        {
            rig2D.velocity = new Vector2(rig2D.velocity.x, 10f);
        }

        // 射击输入
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

