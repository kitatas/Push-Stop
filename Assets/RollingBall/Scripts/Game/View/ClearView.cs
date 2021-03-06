﻿using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RollingBall.Common;
using RollingBall.Common.Sound.SE;
using RollingBall.Common.Transition;
using RollingBall.Common.Utility;
using RollingBall.Title;
using TMPro;
using UnityEngine;
using Zenject;

namespace RollingBall.Game.View
{
    /// <summary>
    /// ステージクリア時の演出
    /// </summary>
    public sealed class ClearView : MonoBehaviour
    {
        [SerializeField] private RankView[] rankViews = default;
        [SerializeField] private TextMeshProUGUI clearText = default;
        [SerializeField] private LoadButton nextButton = default;
        [SerializeField] private LoadButton reloadButton = default;
        [SerializeField] private LoadButton homeButton = default;

        private ISeController _seController;
        private int _level;

        [Inject]
        private void Construct(ISeController seController, int level)
        {
            _seController = seController;
            _level = level;
        }

        public void Show(float clearRate)
        {
            var clearRank = RankLoader.SaveClearData(_level - 1, clearRate);

            var token = this.GetCancellationTokenOnDestroy();
            TweenClearAsync(token, clearRank).Forget();
        }

        private async UniTaskVoid TweenClearAsync(CancellationToken token, int clearRank)
        {
            await TweenClearTextAsync(token);

            await TweenRankImagesAsync(clearRank, token);

            await UniTask.WhenAll(
                nextButton.ShowAsync(token),
                reloadButton.ShowAsync(token),
                homeButton.ShowAsync(token)
            );
        }

        private async UniTask TweenClearTextAsync(CancellationToken token)
        {
            var tasks = new List<UniTask>();
            var textAnimation = new DOTweenTMPAnimator(clearText);
            var offset = Vector3.up * 40.0f;
            var charCount = textAnimation.textInfo.characterCount;
            for (int i = 0; i < charCount; i++)
            {
                tasks.Add(DOTween.Sequence()
                    .Append(textAnimation
                        .DOOffsetChar(i, textAnimation.GetCharOffset(i) + offset, Const.CLEAR_TEXT_ANIMATION_TIME)
                        .SetEase(Ease.OutFlash, 2))
                    .Join(textAnimation
                        .DOFadeChar(i, 1, Const.CLEAR_TEXT_ANIMATION_TIME))
                    .SetDelay(i * 0.04f)
                    .WithCancellation(token));
            }

            _seController.PlaySe(SeType.Clear);

            await UniTask.WhenAll(tasks);

            var highlightColor = new Color(1f, 1f, 0.8f);
            for (int i = 0; i < charCount; i++)
            {
                var interval = i * 0.05f;
                DOTween.Sequence()
                    .AppendInterval(0.5f)
                    .Append(textAnimation
                        .DOColorChar(i, highlightColor, 0.15f)
                        .SetLoops(2, LoopType.Yoyo)
                        .SetDelay(interval))
                    .AppendInterval(3.0f - interval)
                    .SetLoops(-1)
                    .DisableKill(this);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.25f), cancellationToken: token);

            _seController.PlaySe(SeType.Flash);
        }

        private async UniTask TweenRankImagesAsync(int clearRank, CancellationToken token)
        {
            _seController.PlaySe(SeType.Star);

            if (clearRank == 1)
            {
                await rankViews[0].TweenStarAsync(Side.Center, token);
            }
            else if (clearRank == 2)
            {
                await (
                    rankViews[0].TweenStarAsync(Side.Left2, token),
                    rankViews[1].TweenStarAsync(Side.Right2, token)
                );
            }
            else if (clearRank == 3)
            {
                await (
                    rankViews[0].TweenStarAsync(Side.Left3, token),
                    rankViews[1].TweenStarAsync(Side.Center, token),
                    rankViews[2].TweenStarAsync(Side.Right3, token)
                );
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f), cancellationToken: token);
        }
    }
}