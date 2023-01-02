using System;
using System.Collections.Generic;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace DefaultNamespace
{
    [Serializable]
    public class SpacialAnchorHelper
    {
        private bool needAnchorEnumeration = false;
        
        private ulong[] anchorHandles = null;
        private Dictionary<ulong, AnchorPrefab> AnchorDictionary = new Dictionary<ulong, AnchorPrefab>();

        private ScenePerceptionManager scenePerceptionManager;
        private readonly AnchorPrefab anchorPrefab;

        public SpacialAnchorHelper(ScenePerceptionManager scenePerceptionManager,AnchorPrefab anchorPrefab)
        {
            this.scenePerceptionManager = scenePerceptionManager;
            this.anchorPrefab = anchorPrefab;
        }

        public void SetAnchorsShouldBeUpdated()
        {
            needAnchorEnumeration = true;
        }
        
        public void UpdateAnchorDictionary()
        {
            WVR_Result result;

            if (anchorHandles == null || needAnchorEnumeration)
            {
                result = scenePerceptionManager.GetSpatialAnchors(out anchorHandles);
                if (result != WVR_Result.WVR_Success)
                {
                    Debug.LogError("Failed to get spatial anchors");
                    return;
                }
                needAnchorEnumeration = false;
            }

            if (anchorHandles == null) return;
			
            foreach (ulong anchorHandle in anchorHandles)
            {
                WVR_SpatialAnchorState currentAnchorState = default(WVR_SpatialAnchorState);
                result = scenePerceptionManager.GetSpatialAnchorState(anchorHandle, ScenePerceptionManager.GetCurrentPoseOriginModel(), out currentAnchorState);
                if (result == WVR_Result.WVR_Success)
                {
                    Debug.Log("Anchor Tracking State: " + currentAnchorState.trackingState.ToString());
                    switch(currentAnchorState.trackingState)
                    {
                        case WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Tracking:
                        {
                            if (!AnchorDictionary.ContainsKey(anchorHandle)) //Create Anchor Object
                            {
                                AnchorPrefab newAnchorPrefabInstance = CreateNewAnchor(anchorHandle, currentAnchorState);

                                AnchorDictionary.Add(anchorHandle, newAnchorPrefabInstance);
                            }
                            else //Anchor is already in dictionary
                            {
                                CheckAnchorPose(anchorHandle,currentAnchorState);
                            }

                            break;
                        }
                        case WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Paused:
                        case WVR_SpatialAnchorTrackingState.WVR_SpatialAnchorTrackingState_Stopped:
                        default:
                        {
                            //Remove from dictionary if exists
                            if (AnchorDictionary.ContainsKey(anchorHandle))
                            {
                                UnityEngine.Object.Destroy(AnchorDictionary[anchorHandle]); //Destroy Anchor GO
                                AnchorDictionary.Remove(anchorHandle);
                            }
                            break;
                        }
                    }
                }
            }
        }
        public void HandleAnchorUpdateDestroy(RaycastHit raycastHit)
        {
            if (raycastHit.collider == null) return;
            if (raycastHit.collider.transform.GetComponent<AnchorPrefab>() == null) return;
            
            ulong targetAnchorHandle = raycastHit.collider.transform.GetComponent<AnchorPrefab>().anchorHandle;

            var result = scenePerceptionManager.DestroySpatialAnchor(targetAnchorHandle);
            if (result == WVR_Result.WVR_Success)
            {
                UnityEngine.Object.Destroy(AnchorDictionary[targetAnchorHandle]);
                AnchorDictionary.Remove(targetAnchorHandle);

                needAnchorEnumeration = true;

                UpdateAnchorDictionary();
            }
        }
        
        private AnchorPrefab CreateNewAnchor(ulong anchorHandle,WVR_SpatialAnchorState anchorState)
        {
            AnchorPrefab newAnchorPrefabInstance = UnityEngine.Object.Instantiate<AnchorPrefab>(anchorPrefab);
										
            newAnchorPrefabInstance.anchorHandle = anchorHandle;
            newAnchorPrefabInstance.currentAnchorState = anchorState;

            SetAnchorPoseInScene(anchorPrefab, anchorState);
            return newAnchorPrefabInstance;
        }

        private void CheckAnchorPose(ulong anchorHandle,WVR_SpatialAnchorState anchorState)
        {
            //Check anchor pose
            AnchorPrefab currentAnchorPrefabInstance = AnchorDictionary[anchorHandle];
            if (currentAnchorPrefabInstance == null)
            {
                Debug.LogError("Anchor prefab gameobject deleted but the internals are still hanging out");
                //FIXME: clean up instance i guess? could do this directly if code ran differently, but see if anyone else sets needAnchorEnumeration after first pass
                needAnchorEnumeration = true;
                return;
            }

            if (!ScenePerceptionManager.AnchorStatePoseEqual(currentAnchorPrefabInstance.currentAnchorState, anchorState)) //Pose different, update
            {
                SetAnchorPoseInScene(anchorPrefab, anchorState);
				
                currentAnchorPrefabInstance.currentAnchorState = anchorState;
            }
        }

        private void SetAnchorPoseInScene(AnchorPrefab anchorPrefab, WVR_SpatialAnchorState anchorState)
        {
            scenePerceptionManager.ApplyTrackingOriginCorrectionToAnchorPose(anchorState, out Vector3 currentAnchorPosition, out Quaternion currentAnchorRotation);
            anchorPrefab.transform.SetPositionAndRotation(currentAnchorPosition,currentAnchorRotation);
        }
		
        public void ClearAnchors()
        {
            foreach (KeyValuePair<ulong, AnchorPrefab> anchorPair in AnchorDictionary)
            {
                var anchor = anchorPair.Value;
                if (anchor == null)
                {
                    Debug.LogWarning("Anchor deleted without being cleaned up in anchor manager");
                    continue;
                }
                UnityEngine.Object.Destroy(anchor.gameObject);
            }

            AnchorDictionary.Clear();
        }
    }
}