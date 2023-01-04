using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Native;

namespace DefaultNamespace
{
    public class GeneratedPlaneContainer : IDisposable
    {
        private readonly List<GeneratedPlane> generatedPlanes = new List<GeneratedPlane>();
        
        private readonly ScenePerceptionManager scenePerceptionManager;
        private readonly Material generatedMeshMaterialTranslucent;
        private readonly GameObject anchorDisplayPrefab;

        public GeneratedPlaneContainer(ScenePerceptionManager scenePerceptionManager,Material generatedMeshMaterialTranslucent,GameObject anchorDisplayPrefab)
        {
            this.scenePerceptionManager = scenePerceptionManager ?? throw new ArgumentNullException(nameof(scenePerceptionManager));
            this.generatedMeshMaterialTranslucent = generatedMeshMaterialTranslucent?? throw new ArgumentNullException(nameof(generatedMeshMaterialTranslucent));
            this.anchorDisplayPrefab = anchorDisplayPrefab ?? throw new ArgumentNullException(nameof(anchorDisplayPrefab));
        }
        public void Dispose()
        {
            foreach (var plane in generatedPlanes)
            {
                plane.Dispose();
            }
            generatedPlanes.Clear();
        }
        
        private GeneratedPlane FindGeneratedPlane(WVR_Uuid uuid) //TOOD: see if we can just use a list with IEqualityComparer<T> instead of using the IsUUIDEqual to simplify normal operations
        {
            foreach (var plane in generatedPlanes)
            {
                if (ScenePerceptionManager.IsUUIDEqual(plane.uuid, uuid))
                {
                    return plane;
                }
            }
            return null;
        }

        enum PlaneAction{NONE,ADD,REMOVE,UPDATE_EXTENTS,UPDATE_POSE}
        IEnumerable<Tuple<PlaneAction,WVR_ScenePlane, GeneratedPlane>> PlaneActionEnumerator()
        {
            WVR_Result result = scenePerceptionManager.GetScenePlanes(ScenePerceptionManager.GetCurrentPoseOriginModel(), out WVR_ScenePlane[] currentScenePlanes);
            if(result != WVR_Result.WVR_Success)
            {
                Debug.LogError("Failed to get scene planes");
                //todo: handle error better
                yield break;
            }
            
            //removing is more tricky - we side step lack of hashing on structs by adding all and removing any that we find by id
            List<GeneratedPlane> toRemoveGeneratedPlanes = new List<GeneratedPlane>(generatedPlanes);
            for (var index = 0; index < currentScenePlanes.Length; index++)
            {
                WVR_ScenePlane currentScenePlane = currentScenePlanes[index];
                GeneratedPlane generatedPlane = FindGeneratedPlane(currentScenePlane.uuid);
                if (generatedPlane == null)
                {
                    yield return new Tuple<PlaneAction, WVR_ScenePlane, GeneratedPlane>(PlaneAction.ADD,currentScenePlane,null);
                }
                else
                {
                    if (!ScenePerceptionManager.ScenePlaneExtent2DEqual(generatedPlane.plane, currentScenePlane))
                    {
                        yield return new Tuple<PlaneAction, WVR_ScenePlane, GeneratedPlane>(PlaneAction.UPDATE_EXTENTS,currentScenePlane,generatedPlane);
                    }
                    else
                    {
                        if (!ScenePerceptionManager.ScenePlanePoseEqual(generatedPlane.plane, currentScenePlane))
                        {
                            yield return new Tuple<PlaneAction, WVR_ScenePlane, GeneratedPlane>(PlaneAction.UPDATE_POSE,currentScenePlane,generatedPlane);
                        }
                        else
                        {
                            yield return new Tuple<PlaneAction, WVR_ScenePlane, GeneratedPlane>(PlaneAction.NONE,currentScenePlane,generatedPlane);
                        }
                    }
                }
            }

            foreach (var removeGeneratedPlane in toRemoveGeneratedPlanes)
            {
                yield return new Tuple<PlaneAction, WVR_ScenePlane, GeneratedPlane>(PlaneAction.REMOVE,default,removeGeneratedPlane);
            }
        }

        //only call if scenePerceptionHelper.CurrentPerceptionTargetIsCompleted -- which was perceptionStateDictionary[currentPerceptionTarget] == WVR_ScenePerceptionState.WVR_ScenePerceptionState_Completed or
        public void UpdateAssumingThePerceptionTargetIsCompleted()  
        {
            foreach (Tuple<PlaneAction, WVR_ScenePlane, GeneratedPlane> planeAction in PlaneActionEnumerator())
            {
                PlaneAction action = planeAction.Item1;
                WVR_ScenePlane currentScenePlane = planeAction.Item2;
                GeneratedPlane generatedPlane = planeAction.Item3;
                
                switch (action)
                {
                    case PlaneAction.ADD:
                        GeneratedPlane newGeneratedPlane = NewGeneratedPlanePlane(currentScenePlane.uuid,currentScenePlane);
                        generatedPlanes.Add(newGeneratedPlane);
                        break;
                    case PlaneAction.REMOVE:
                        generatedPlanes.Remove(generatedPlane);
                        generatedPlane.Dispose();
                        break;
                    case PlaneAction.UPDATE_EXTENTS:
                        generatedPlane.plane = currentScenePlane;
                        generatedPlane.DestroyGameObject();
                        generatedPlane.go = GenerateNewGameObject(currentScenePlane);
                        break;
                    case PlaneAction.UPDATE_POSE:
                        scenePerceptionManager.ApplyTrackingOriginCorrectionToPlanePose(currentScenePlane, out var planePositionUnity, out var planeRotationUnity);
                        generatedPlane.go.transform.SetPositionAndRotation(planePositionUnity,planeRotationUnity);
                        break;
                    case PlaneAction.NONE:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private GeneratedPlane NewGeneratedPlanePlane(WVR_Uuid uuid, WVR_ScenePlane plane)
        {    
            //Log.d(LOG_TAG, "Add new plane");
            return new GeneratedPlane(){uuid = uuid,plane= plane,go = GenerateNewGameObject(plane)};
            
        }
        private GameObject GenerateNewGameObject(WVR_ScenePlane plane)
        {
            //Log.d(LOG_TAG, "Add new plane");
            GameObject newPlaneMeshGO = scenePerceptionManager.GenerateScenePlaneMesh(plane, generatedMeshMaterialTranslucent, true);

            GameObject axisDisplay = UnityEngine.Object.Instantiate(anchorDisplayPrefab, newPlaneMeshGO.transform, true);
            axisDisplay.transform.localPosition = Vector3.zero;
            axisDisplay.transform.localRotation = Quaternion.identity;

            return newPlaneMeshGO;
        }
    }
}