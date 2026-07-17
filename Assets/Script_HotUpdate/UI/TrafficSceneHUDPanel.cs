using Cysharp.Threading.Tasks;
using DG.Tweening;
using QFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace UG20260527
{
    public class TrafficSceneHUDPanel : PanelBase
    {
        // UI
        Button Btn_Setting;
        Button Btn_Mailbox;
        Button Btn_Friend;

        AudioClip audioClip;
        AudioSource audioSource;

        // 动画控制器
        private Animator _animator;
        private const string ANIMATOR_TOSHOWN = "ToShow";
        private const string ANIMATOR_TOHIDE = "ToHide";

        // 局内游戏数据
        private ITrafficGameModel _gameModel;

        // 监听句柄 数组
        private List<IUnRegister> unRegisters = new List<IUnRegister>();


        /* -------------------------------------------------- 生命周期 -------------------------------------------------- */

        public override async UniTask OnInit<T>(Action<T> onInit = null, object userData = null)
        {
            await base.OnInit(onInit, userData);

            // 动画控制器
            _animator = GetComponent<Animator>();
            Btn_Setting = GetComponentInChildren<Button>("Btn_Setting");
            Btn_Mailbox = GetComponentInChildren<Button>("Btn_Mailbox");
            Btn_Friend = GetComponentInChildren<Button>("Btn_Friend");
        }

        public override void OnOpen()
        {
            base.OnOpen();

            // 获取游戏数据
            _gameModel = this.GetModel<ITrafficGameModel>();

            // Level
            unRegisters.Add(_gameModel.Level.RegisterWithInitValue(value =>
            {
                var text_Level = GetComponentInChildren<Text>("Text_Level");
                if(text_Level != null) text_Level.text = value.ToString();
            }));

            // HP
            unRegisters.Add(_gameModel.HP.RegisterWithInitValue(value =>
            {
                var text_HP = GetComponentInChildren<Text>("Text_HP");
                if(text_HP != null) text_HP.text = value.ToString();
            }));

            unRegisters.Add(_gameModel.numberOfVehicles.RegisterWithInitValue(value =>
            {
                var text_NumberOfVehicles = GetComponentInChildren<Text>("Text_NumberOfVehicles");
                if (text_NumberOfVehicles != null) text_NumberOfVehicles.text = value.ToString();
            }));

            unRegisters.Add(_gameModel.numberOfRecycledVehicles.RegisterWithInitValue(value =>
            {
                var text_NumberOfRecycledVehicles = GetComponentInChildren<Text>("Text_NumberOfRecycledVehicles");
                if (text_NumberOfRecycledVehicles != null) text_NumberOfRecycledVehicles.text = value.ToString();
            }));

            // 设置
            Btn_Setting.onClick.AddListener(() =>
            {
                // 震动动画
                if (Btn_Setting != null)
                {
                    Btn_Setting.interactable = false;
                    Btn_Setting.transform.DOShakePosition(1, 10).OnComplete(() =>
                    {
                        // 退出场景
                        this.SendCommand<ExitLatestSceneCommand>();
                    });
                }
            });

            // 邮箱
            Btn_Mailbox.onClick.AddListener(async () =>
            {
                // 播放BGM
                if(audioClip == null) audioClip = await Addressables.LoadAssetAsync<AudioClip>("Assets/Res_HotUpdate/Audio/BGM1.wav");
                if(audioSource == null) audioSource = this.GetSystem<IAudioSystem>().Play2D(audioClip);
                else this.GetSystem<IAudioSystem>().Stop2D(ref audioSource);
            });

            // 好友
            Btn_Friend.onClick.AddListener(async () =>
            {
                var c = await Addressables.LoadAssetAsync<AudioClip>("Assets/Res_HotUpdate/Audio/UI.wav");
                this.GetSystem<IAudioSystem>().Play2DOneShot(c);
            });
        }

        public override void OnClose()
        {
            base.OnClose();

            foreach(var unRegister in unRegisters)
            {
                unRegister.UnRegister();
            }
            unRegisters.Clear();

            Btn_Setting.onClick.RemoveAllListeners();
            Btn_Setting.interactable = true;
            Btn_Mailbox.onClick.RemoveAllListeners();
            Btn_Friend.onClick.RemoveAllListeners();

            audioClip = null;
            if (audioSource != null)
            {
                this.GetSystem<IAudioSystem>().Stop2D(ref audioSource);
                audioSource = null;
            }
        }


        /* -------------------------------------------------- API_动画 -------------------------------------------------- */

        /// <summary>
        /// 播放显示动画
        /// </summary>
        public void Anim_ToShow()
        {
            if(_animator != null)
            {
                _animator.SetTrigger(ANIMATOR_TOSHOWN);
            }
        }

        /// <summary>
        /// 播放隐藏动画，返回动画时长
        /// </summary>
        public float Anim_ToHide()
        {
            if(_animator != null)
            {
                _animator.SetTrigger(ANIMATOR_TOHIDE);
                AnimationClip[] arr = _animator.runtimeAnimatorController.animationClips;
                if(arr.Length > 0)
                {
                    foreach(var clipInfo in arr)
                    {
                        if(clipInfo.name == "Hide") return clipInfo.length;
                    }
                }
            }
            return 0;
        }

    }
}