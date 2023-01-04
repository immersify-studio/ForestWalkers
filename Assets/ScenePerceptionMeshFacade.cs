using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace DefaultNamespace
{
    public class ScenePerceptionMeshFacade
    {
        private readonly ScenePerceptionHelper scenePerceptionHelper;
        private readonly GeneratedPlaneContainer generatedPlaneContainer;

        public ScenePerceptionMeshFacade(ScenePerceptionHelper scenePerceptionHelper,GameObject anchorDisplayPrefab,Material generatedMeshMaterialTranslucent)
        {
            this.scenePerceptionHelper = scenePerceptionHelper ?? throw new ArgumentNullException(nameof(scenePerceptionHelper));
            if(generatedMeshMaterialTranslucent == null) throw new ArgumentNullException(nameof(generatedMeshMaterialTranslucent)); 
            generatedPlaneContainer = new GeneratedPlaneContainer(scenePerceptionHelper.scenePerceptionManager,generatedMeshMaterialTranslucent,anchorDisplayPrefab);
        }
	    
        public void UpdateScenePerceptionMesh()
        {
            if (!scenePerceptionHelper.CurrentPerceptionTargetIsCompleted())
            {
                Debug.LogError( "OnClickGenerateMesh: Perception not complete, cannot generate mesh.");
                return;
            }

            switch(scenePerceptionHelper.currentPerceptionTarget)
            {
                case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_2dPlane:
                {
                    generatedPlaneContainer.UpdateAssumingThePerceptionTargetIsCompleted();
                    break;
                }

                case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_3dObject:
                case WVR_ScenePerceptionTarget.WVR_ScenePerceptionTarget_SceneMesh:
                default:
                    break;
            }
        }

        public void DestroyGeneratedMeshes()
        {
            generatedPlaneContainer.Dispose();
        }
    }
}