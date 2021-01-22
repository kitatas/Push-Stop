﻿using System;
using RollingBall.Game.Memento;
using RollingBall.Game.Player;
using RollingBall.Game.StageObject;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RollingBall.Game.StageData
{
    /// <summary>
    /// ステージの読み込み
    /// </summary>
    public sealed class StageLoader
    {
        private enum SquareType
        {
            None = 0,
            Player = 1,
            Goal = 2,
            Block = 3,
            MoveBlock = 4,
            BallBlock = 5,
        }

        private readonly StageObjectTable _stageObjectTable;
        private readonly IStageObject _player;
        private readonly IStageObject _goal;

        public StageLoader(StageObjectTable stageObjectTable, PlayerController playerController, Goal goal,
            StageLevelLoader stageLevelLoader, Caretaker caretaker)
        {
            _stageObjectTable = stageObjectTable;
            _player = playerController;
            _goal = goal;

            LoadStageData(stageLevelLoader.GetStageData().stageFile);
            caretaker.Initialize();
        }

        private void LoadStageData(TextAsset stageFile)
        {
            var lines = stageFile.text.Split(new[] {'\n', '\r'}, StringSplitOptions.RemoveEmptyEntries);

            var col = lines[0].Split(',').Length;
            var row = lines.Length;

            for (int y = 0; y < row; y++)
            {
                var value = lines[y].Split(',');
                for (int x = 0; x < col; x++)
                {
                    var type = (SquareType) int.Parse(value[x]);
                    var pos = new Vector2(x - 2, 2 - y);
                    Create(type, pos);
                }
            }
        }

        private void Create(SquareType squareType, Vector2 position)
        {
            IStageObject stageObject;
            switch (squareType)
            {
                case SquareType.None:
                    return;
                case SquareType.Player:
                    stageObject = _player;
                    break;
                case SquareType.Goal:
                    stageObject = _goal;
                    break;
                case SquareType.Block:
                    stageObject = Object.Instantiate(_stageObjectTable.normalBlock);
                    break;
                case SquareType.MoveBlock:
                    stageObject = Object.Instantiate(_stageObjectTable.moveBlock);
                    break;
                case SquareType.BallBlock:
                    stageObject = Object.Instantiate(_stageObjectTable.ballBlock);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(squareType), squareType, null);
            }

            stageObject.SetPosition(position);
        }
    }
}