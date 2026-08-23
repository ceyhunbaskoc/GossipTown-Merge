using System;
using Core.Services;
using UI.Story;
using Data.Roadmap;
using Core.SaveSystem;
using Core.Story;
using UI.Map;

namespace Core.Controllers
{
    public class StoryTriggerController : IDisposable
    {
        private readonly RoadmapUIController _roadmapUIController;
        private readonly StoryPanelPresenter _storyPresenter;
        
        private readonly StoryService _storyService; 

        public StoryTriggerController(
            RoadmapUIController roadmapUIController, 
            StoryPanelPresenter storyPresenter,
            StoryService storyService)
        {
            _roadmapUIController = roadmapUIController;
            _storyPresenter = storyPresenter;
            _storyService = storyService;

            _roadmapUIController.OnNodeUpgradeVisualCompleted += HandleNodeUpgraded;
        }

        private void HandleNodeUpgraded(string id)
        {
            if (!_storyService.IsStoryCompleted())
            {
                _storyPresenter.OpenPanel();
            }
            else
            {
                UnityEngine.Debug.Log("[StoryTriggerController] Hikaye tamamlandı, panel açılmıyor.");
            }
        }

        public void Dispose()
        {
            if (_roadmapUIController != null)
            {
                _roadmapUIController.OnNodeUpgradeVisualCompleted -= HandleNodeUpgraded;
            }
        }
    }
}