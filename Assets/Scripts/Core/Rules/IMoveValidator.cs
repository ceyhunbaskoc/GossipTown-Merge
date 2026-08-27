using Core.GridSystem;
using UnityEngine;

namespace Core.Rules
{
    public interface IMoveValidator
    {
        bool CanMoveForTutorial(Vector2Int toGridPosition);
        bool CanMoveBackpack();
    }
}