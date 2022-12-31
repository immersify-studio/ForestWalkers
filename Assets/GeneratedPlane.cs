using System;
using UnityEngine;
using Wave.Essence.ScenePerception;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace DefaultNamespace
{
    public class GeneratedPlane : IDisposable
    {
        public WVR_Uuid uuid; //not sure if we can replace with accessor
        public WVR_ScenePlane plane;
        public GameObject go;

        public void DestroyGameObject()
        {
            if (go != null) // ? operator doesn't work on unity objects, explicitly test for null
            {
                var meshFilter = go.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh) //not sure if mesh or sharedmesh is right here.. but think sharedmesh is safer mayber?
                {
                    UnityEngine.Object.Destroy(meshFilter.sharedMesh); 
                }

                UnityEngine.Object.Destroy(go);
                go = null;
            }
        }
        public static GeneratedPlane NewPlane(ScenePerceptionManager scenePerceptionManager,Material generatedMeshMaterialTranslucent,AnchorPrefab anchorDisplayPrefab, WVR_Uuid uuid, WVR_ScenePlane plane)
        {    
            //Log.d(LOG_TAG, "Add new plane");
            GameObject newPlaneMeshGO = scenePerceptionManager.GenerateScenePlaneMesh(plane, generatedMeshMaterialTranslucent, true);

            AnchorPrefab axisDisplay = UnityEngine.Object.Instantiate(anchorDisplayPrefab, newPlaneMeshGO.transform, true);
            axisDisplay.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            return new GeneratedPlane(){uuid = uuid,plane= plane,go = GeneratedPlane.GenerateNewGameObject(plane, scenePerceptionManager, generatedMeshMaterialTranslucent, axisDisplay)};
            
        }
        public static GameObject GenerateNewGameObject(WVR_ScenePlane plane, ScenePerceptionManager scenePerceptionManager,Material generatedMeshMaterialTranslucent,AnchorPrefab anchorDisplayPrefab)
        {
            //Log.d(LOG_TAG, "Add new plane");
            GameObject newPlaneMeshGO = scenePerceptionManager.GenerateScenePlaneMesh(plane, generatedMeshMaterialTranslucent, true);

            AnchorPrefab axisDisplay = UnityEngine.Object.Instantiate(anchorDisplayPrefab, newPlaneMeshGO.transform, true);
            axisDisplay.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            return newPlaneMeshGO;
        }
        /*
         * foreach (KeyValuePair<WVR_Uuid, GameObject> uuidGOPair in generatedPlaneGOList)
                {
                    MeshFilter generatedPlaneMeshFilter = uuidGOPair.Value.GetComponent<MeshFilter>();
                    UnityEngine.Object.Destroy(generatedPlaneMeshFilter.mesh);
                    UnityEngine.Object.Destroy(uuidGOPair.Value);
                }
         */
        public void Dispose()
        {
            DestroyGameObject();
        }
    }
}