﻿using System.Collections.Generic;
using ScriptableSystem.GameEvent;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Voice;

namespace Architecture
{
    public class StepController : SerializedMonoBehaviour
    {
        [SerializeField] private List<Step> _steps;
        [SerializeField] private StepGameEvent _onStepLoaded;
        [SerializeField] private GameEvent _onStepChanged;
        [SerializeField] private GameEvent _onStepEnded;

#if UNITY_EDITOR

        [Button]
        private void CreateStepsFromFile(VoiceIntent intent)
        {
            CreateStepsFromFile("Assets/Resources/DialogMenuDictionary.txt");
            SetButtonActionSteps("Assets/Resources/AnnotationMenuDictionary.txt");
            SetStepsConditions(intent);
            SetListeners("Assets/Resources/VoiceListenerData.txt", intent);
            SetVoiceActor("Assets/Resources/Voice");
            AssetDatabase.SaveAssets();
        }

        private void CreateStepsFromFile(string filePath)
        {
            var builder = new StepBuilder();
            _steps = builder.CreateStepsFromFile(filePath);
        }


        private void SetButtonActionSteps(string filePath)
        {
            var builder = new StepBuilder();
            builder.SetButtonActionFromFile(filePath, _steps);
        }


        private void SetStepsConditions(GameEvent condition)
        {
            var builder = new StepBuilder();
            builder.SetStepTransitions(_steps, condition);
        }

        private void SetVoiceActor(string audioFilesFolderPath)
        {
            var builder = new StepBuilder();
            builder.SetVoiceActingFromFiles(_steps, audioFilesFolderPath);
        }

        private void SetListeners(string filePath, VoiceIntent intent)
        {
            var builder = new StepBuilder();
            builder.SetVoiceListenersFromFile(_steps, filePath, intent);
        }

        private void CreateStepsJson(string filePath)
        {
            var builder = new StepBuilder();
            builder.CreateStepsJSON(_steps, filePath);
        }

#endif


        private Step _currentStep;

        private void Start()
        {
            _currentStep = _steps[0];
            _onStepLoaded.Invoke(_currentStep);
            SubscribeTransitions(_currentStep);
            _onStepChanged.Invoke();
        }

        private void SubscribeTransitions(Step step)
        {
            foreach (var stepTransition in step.Transitions)
            {
                stepTransition.WaitForCondition();
                stepTransition.OnConditionComplete += LoadNextStep;
            }
        }

        private void UnsubscribeTransitions(Step step)
        {
            foreach (var stepTransition in step.Transitions)
            {
                stepTransition.StopWaitingForCondition();
                stepTransition.OnConditionComplete -= LoadNextStep;
            }
        }

        private void OnDisable() => UnsubscribeTransitions(_currentStep);

        private void LoadNextStep(Step step)
        {
            UnsubscribeTransitions(_currentStep);
            _currentStep = step;
            _onStepEnded.Invoke();
            _onStepLoaded.Invoke(_currentStep);
            _onStepChanged.Invoke();
            SubscribeTransitions(_currentStep);
        }
    }
}