using System;
using System.Collections.Generic;
using Core.SaveSystem;
using UnityEngine;

namespace Data.Tutorial
{
    [Serializable]
    public class StepText
    {
        public TutorialStep StepCategory;
        public string Text;
    }
    [CreateAssetMenu(fileName = "TutorialHelperTexts", menuName = "Tutorial/Tutorial Helper Texts")]
    public class TutorialHelperTextsSO : ScriptableObject
    {
        public List<StepText> StepTexts;

        public StepText GetStepTextByCategory(TutorialStep step)
        {
            return StepTexts.Find(s => s.StepCategory == step);
        }
    }
}