﻿using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RollingBall.Common.Button;
using UniRx;
using UnityEngine;
using Zenject;

namespace RollingBall.Common.Transition
{
    /// <summary>
    /// シーン遷移を行うボタン
    /// </summary>
    [RequireComponent(typeof(ButtonActivator))]
    [RequireComponent(typeof(ButtonFader))]
    [RequireComponent(typeof(ButtonSpeaker))]
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public sealed class LoadButton : MonoBehaviour
    {
        [SerializeField] private LoadType loadType = default;
        [SerializeField] private int stageNumber = default;

        private ButtonActivator _buttonActivator;
        private ButtonFader _buttonFader;
        private SceneLoader _sceneLoader;
        private int _level;

        [Inject]
        private void Construct(SceneLoader sceneLoader, int level)
        {
            _buttonActivator = GetComponent<ButtonActivator>();
            _buttonFader = GetComponent<ButtonFader>();
            _sceneLoader = sceneLoader;
            _level = level;
        }

        private void Start()
        {
            GetComponent<UnityEngine.UI.Button>()
                .OnClickAsObservable()
                .Subscribe(_ => LoadScene())
                .AddTo(this);
        }

        private void LoadScene()
        {
            switch (loadType)
            {
                case LoadType.Direct:
                    _sceneLoader.FadeLoadScene(SceneName.Main, stageNumber, Const.FADE_TIME);
                    break;
                case LoadType.Reload:
                    _sceneLoader.FadeLoadScene(SceneName.Main, _level, Const.FADE_TIME);
                    break;
                case LoadType.Next:
                    LoadNext();
                    break;
                case LoadType.Title:
                    LoadTitle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(loadType), loadType, null);
            }
        }

        private void LoadNext()
        {
            var nextLevel = _level + 1;
            if (nextLevel <= Const.MAX_STAGE_COUNT)
            {
                _sceneLoader.FadeLoadScene(SceneName.Main, nextLevel, Const.FADE_TIME);
                return;
            }

            LoadTitle();
        }

        private void LoadTitle()
        {
            _sceneLoader.FadeLoadScene(SceneName.Title, 0, Const.FADE_TIME);
        }

        public async UniTask ShowAsync(CancellationToken token)
        {
            _buttonActivator.SetEnabled(false);
            _buttonActivator.SetInteractable(true);

            await _buttonFader.ShowAsync(token);

            _buttonActivator.SetEnabled(true);
        }
    }
}