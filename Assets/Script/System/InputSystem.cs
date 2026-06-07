using QFramework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UG20260527
{
    public interface IInputSystem : ISystem
    {
        void Enable();
        void Disable();
        string SwitchActionMap(string mapName);
    }

    public struct InputActionEvent
    {
        /// <summary>
        /// 输入Map名
        /// </summary>
        public string mapName;
        /// <summary>
        /// Map中Action名
        /// </summary>
        public string actionName;
        /// <summary>
        /// Action触发时，传入的输入上下文
        /// </summary>
        public InputAction.CallbackContext context;

        public InputActionEvent(string map, string action, InputAction.CallbackContext con)
        {
            mapName = map;
            actionName = action;
            context = con;
        }
    }

    public class InputSystem : AbstractSystem, IInputSystem
    {
        // GameControl
        private GameControls GameControls = new GameControls();
        private string CurActionMapName = "";


        protected override void OnInit()
        {
            // 注册所有的Map
            RegisterAllActionMap();
        }

        private void RegisterAllActionMap()
        {
            foreach(var map in GameControls.asset.actionMaps)
            {
                foreach(var action in map.actions)
                {
                    action.started += ctx => _SendEvent(new InputActionEvent(map.name, action.name, ctx));
                    action.performed += ctx => _SendEvent(new InputActionEvent(map.name, action.name, ctx));
                    action.canceled += ctx => _SendEvent(new InputActionEvent(map.name, action.name, ctx));
                }
            }
        }

        private void _SendEvent(InputActionEvent e)
        {
            if(CurActionMapName == e.mapName)
            {
                this.SendEvent(e);
            }
        }

        

        void IInputSystem.Enable()
        {
            if(string.IsNullOrEmpty(CurActionMapName))
            {
                Debug.LogWarning("当前ActionMap为空");
                return;
            }
            GameControls.asset.FindActionMap(CurActionMapName)?.Enable();
        }

        void IInputSystem.Disable()
        {
            // 停用所有 ActionMap
            GameControls.asset.Disable();
        }

        // 切换到目标ActionMap
        string IInputSystem.SwitchActionMap(string mapName)
        {

            // 查找Asset中是否存在mapName
            InputActionMap targetMap = GameControls.asset.FindActionMap(mapName);
            if(targetMap == null)
            {
                Debug.LogWarning($"{mapName} 不存在 InputSystem配置中");
                return CurActionMapName;
            }

            // 停用当前 ActionMap
            if(!string.IsNullOrEmpty(CurActionMapName))
            {
                GameControls.asset.FindActionMap(CurActionMapName)?.Disable();
            }

            // 开启当前mapName
            targetMap.Enable();
            string lastActionMapName = CurActionMapName;
            CurActionMapName = mapName;
            Debug.Log($"切换到 ActionMap：{mapName}");

            return lastActionMapName;
        }
    }
}
