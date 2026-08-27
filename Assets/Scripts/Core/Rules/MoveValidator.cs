using Core.GridSystem;
using Core.Tutorial;
using Data;
using UnityEngine;

namespace Core.Rules
{
    public class MoveValidator : IMoveValidator
    {
        private readonly TutorialOrchestrator _tutorialOrchestrator;
        private readonly StartingBoardSetupSO _startingBoardSetup;

        public MoveValidator(TutorialOrchestrator tutorialOrchestrator, StartingBoardSetupSO startingBoardSetup)
        {
            _tutorialOrchestrator = tutorialOrchestrator;
            _startingBoardSetup = startingBoardSetup;
        }
        
        public bool CanMoveForTutorial(Vector2Int toGridPosition)
        {
            if (_tutorialOrchestrator.IsCompleted) return true;

            if (toGridPosition == _startingBoardSetup.TutorialItem1Position ||
                toGridPosition == _startingBoardSetup.TutorialItem2Position)
            {
                return true;
            }

            return false;
        }

        public bool CanMoveBackpack()
        {
            return _tutorialOrchestrator.IsCompleted;
        }
    }
}