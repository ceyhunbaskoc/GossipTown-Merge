using Data.Quests;
using UnityEngine;

namespace UI.FlightSystem
{
    public interface IHUDTargetProvider
    {
        Vector3 GetTargetScreenPosition(RewardCategory category);
    }
}