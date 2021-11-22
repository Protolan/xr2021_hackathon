﻿using System.Collections;
using Architecture;
using ScriptableSystem.GameEvent;
using UnityEngine;

namespace Voice
{
    public class VoiceActing: MonoBehaviour
    {
        [SerializeField] private StepGameEvent _onStepLoaded;
        [SerializeField] private GameEvent _onStepEnded;
        [SerializeField] private GameEvent _onClipFinished;
        [SerializeField] private AudioSource _source;
        [SerializeField] private GameEvent _onNotUnderstand;

        private Coroutine _currentPlaying;
        private AudioClip _notUnderstandClip;
        
        private void OnEnable()
        {
            _onStepLoaded.AddAction(StartActingIfHave);
            _onStepEnded.AddAction(StopClip);
            _onNotUnderstand.AddAction(NotUnderstand);
        }

        private void NotUnderstand()
        {
            StopClip();
            StartActing(_notUnderstandClip);
        }

        private void OnDisable()
        {
            _onStepLoaded.RemoveAction(StartActingIfHave);
            _onStepEnded.RemoveAction(StopClip);
            _onNotUnderstand.RemoveAction(NotUnderstand);

        }

        private void StopClip()
        { 
            if(_currentPlaying != null) StopCoroutine(_currentPlaying); 
            if(_source.isPlaying) _source.Stop();
        }

        private void StartActingIfHave(Step step)
        {
            if (step.ContainsFeature(StepFeature.VoiceActing))
                StartActing(step.ActingData._clip);
            if (step.ContainsFeature(StepFeature.VoiceListener))
                _notUnderstandClip = step.ListenerData._notUnderStandClip;
        }

        private void StartActing(AudioClip clip)
        {
            _source.clip = clip;
            _source.Play();
            _currentPlaying =  StartCoroutine(WaitForEnding(clip.length));
        }

        private IEnumerator WaitForEnding(float clipDuration)
        {
            yield return new WaitForSeconds(clipDuration);
            _onClipFinished.Invoke();
        }
    }
}