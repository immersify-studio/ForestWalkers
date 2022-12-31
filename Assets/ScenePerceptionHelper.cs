using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

namespace DefaultNamespace
{
    [Serializable]
    public class ScenePerceptionHelper
    {
        public ScenePerceptionManager scenePerceptionManager { get; private set; }
        public bool isSceneCMPStarted { get; private set; } //what is CMP?
        public bool isRunning { get; private set; } //should this be used instead of is SceneCMPStarted?
        
        private Dictionary<WVR_ScenePerceptionTarget, WVR_ScenePerceptionState> perceptionStateDictionary = new Dictionary<WVR_ScenePerceptionTarget, WVR_ScenePerceptionState>();
        public WVR_ScenePerceptionTarget currentPerceptionTarget { get; private set; } = WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane;
        
        public void OnEnable()
        {
            if(IsScenePerceptionSupported())
            {
                Debug.LogError("ScenePerception Not Supported"); //Pass in logger?
                throw new Exception("Scene Perception is not supported on this device"); 

            }
            WVR_Result result = scenePerceptionManager.StartScene();
            if (result == WVR_Result.WVR_Success)
            {
                isSceneCMPStarted = true; //FIXME: should this only be set if the start scene perception is true??
                result = scenePerceptionManager.StartScenePerception(WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane);

                if (result == WVR_Result.WVR_Success)
                {
                    ScenePerceptionGetState();
                    isRunning = true;
                }
            }
        }

        public void OnDisable()
        {
            if (!isSceneCMPStarted) return;
            isSceneCMPStarted = false;

            if (isRunning)
            {
                scenePerceptionManager.StopScene();
                isRunning = false;
            }
        }
        public void ScenePerceptionGetState()
        {
            WVR_ScenePerceptionState latestPerceptionState = WVR_ScenePerceptionState.WVR_ScenePerceptionState_Empty;
            WVR_Result result = scenePerceptionManager.GetScenePerceptionState(currentPerceptionTarget, ref latestPerceptionState);
            if (result == WVR_Result.WVR_Success)
            {
                perceptionStateDictionary[currentPerceptionTarget] = latestPerceptionState; //Update perception state for the perception target
            }
        }
        
        private bool IsScenePerceptionSupported()
        {
            return (Interop.WVR_GetSupportedFeatures() &
                    (ulong) WVR_SupportedFeature.WVR_SupportedFeature_ScenePerception) != 0;
        }

        public bool CurrentPerceptionTargetIsCompleted()
        {
            return perceptionStateDictionary[currentPerceptionTarget] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed;
        }
    }
}