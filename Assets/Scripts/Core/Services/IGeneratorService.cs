using Core.GridSystem;
using UnityEngine;

namespace Core.Services
{
    public interface IGeneratorService
    {
        GeneratorResult TryGenerateItem(Vector2Int spawnerPos, IGridItem spawnerItem);
    }
}