using System;
using Core.Services;
using UI.Story;
using Data.Roadmap;
using Core.SaveSystem;

namespace Core.Controllers
{
    public class StoryTriggerController : IDisposable
    {
        private readonly RoadmapProgressionService _roadmapService;
        private readonly StoryPanelPresenter _storyPresenter;

        public StoryTriggerController(RoadmapProgressionService roadmapService, StoryPanelPresenter storyPresenter)
        {
            _roadmapService = roadmapService;
            _storyPresenter = storyPresenter;

            _roadmapService.OnNodeUpgraded += HandleNodeUpgraded;
        }

        private void HandleNodeUpgraded(MapNodeDefinitionSO nodeDef, BuildingLevelData nextLevelData)
        {
            _storyPresenter.OpenPanel();
        }

        public void Dispose()
        {
            if (_roadmapService != null)
            {
                _roadmapService.OnNodeUpgraded -= HandleNodeUpgraded;
            }
        }
    }
}