using UnityEngine;
using Wave.Essence;
using Wave.Essence.ScenePerception.Sample;
using Wave.Native;

namespace DefaultNamespace
{
    public class ScenePerceptionDemo2 : MonoBehaviour
    {
	    [SerializeField] private PassThroughHelper passThroughHelper;
	    [SerializeField] private AnchorPrefab anchorPrefab;
	    [SerializeField] private GameObject anchorDisplayPrefab;

        private ScenePerceptionHelper _scenePerceptionHelper;
        private SpacialAnchorHelper _spacialAnchorHelper;
        private ScenePerceptionMeshFacade _scenePerceptionMeshFacade;
        private bool hidePlanesAndAnchors = false;
        
        //physics stuff?
        private RaycastHit leftControllerRaycastHitInfo = new RaycastHit(), rightControllerRaycastHitInfo = new RaycastHit();
        private GameObject AnchorDisplayRight = null; 
        
        //config for scene perception mesh... maybe handle this some other way either through seperate init call and serializing or a serialed dto?
        [SerializeField] private Material GeneratedMeshMaterialTranslucent;
        [SerializeField] private GameObject leftController = null, rightController = null;

        private void OnEnable()
        {
            if (_scenePerceptionHelper == null)
            {
                _scenePerceptionHelper = new ScenePerceptionHelper();
                _spacialAnchorHelper = new SpacialAnchorHelper(_scenePerceptionHelper.scenePerceptionManager, anchorPrefab);
                if (_scenePerceptionHelper.isRunning)
                {
	                _spacialAnchorHelper.SetAnchorsShouldBeUpdated();
                }
	            _scenePerceptionMeshFacade = new ScenePerceptionMeshFacade(_scenePerceptionHelper, anchorDisplayPrefab,GeneratedMeshMaterialTranslucent);
            }
        }
        private void OnDisable()
        {
            if (_scenePerceptionHelper != null)
            {
                _scenePerceptionHelper.OnDisable();
            }
        }
        private void OnApplicationPause(bool pause)
        {
            if (!pause)
            {
                _spacialAnchorHelper.SetAnchorsShouldBeUpdated(); //Anchors will have moved since the program was previously running - re-update during On Resume in case of a tracking map change
            }
        }

        private void Update()
        {
	        if (_scenePerceptionHelper.isSceneCMPStarted && !hidePlanesAndAnchors)
	        {
		        //Log.d(LOG_TAG, "Update Plane and Anchors");
		        //Handle Scene Perception
		        _scenePerceptionHelper.ScenePerceptionGetState(); //Update state of scene perception every frame
		        _scenePerceptionMeshFacade.UpdateScenePerceptionMesh();

		        //Handle Spatial Anchor
		        _spacialAnchorHelper.UpdateAnchorDictionary();
	        }

	        if (_scenePerceptionHelper.isSceneCMPStarted)
	        {
		        if (ButtonFacade.XButtonPressed)
		        {
			        _spacialAnchorHelper.HandleAnchorUpdateDestroy(leftControllerRaycastHitInfo);
		        }
		        if (ButtonFacade.AButtonPressed)
		        {
			        _spacialAnchorHelper.HandleAnchorUpdateDestroy(rightControllerRaycastHitInfo);
		        }
	        }
	        if (ButtonFacade.YButtonPressed)
	        {
		        passThroughHelper.ShowPassthroughUnderlay(!Interop.WVR_IsPassthroughOverlayVisible());
	        }
	        if (ButtonFacade.BButtonPressed)
	        {
		        hidePlanesAndAnchors = !hidePlanesAndAnchors;
		        Debug.Log("hidePlanesAndAnchors: " + hidePlanesAndAnchors);
		        if (hidePlanesAndAnchors)
		        {
			        _scenePerceptionMeshFacade.DestroyGeneratedMeshes();
			        _spacialAnchorHelper.ClearAnchors();
		        }
	        }

        }
        private void FixedUpdate()
        {
	        if (AnchorDisplayRight == null)
	        {
		        AnchorDisplayRight = UnityEngine.GameObject.Instantiate(anchorDisplayPrefab);
	        }

	        Physics.Raycast(leftController.transform.position, leftController.transform.forward, out leftControllerRaycastHitInfo);

	        Physics.Raycast(rightController.transform.position, rightController.transform.forward, out rightControllerRaycastHitInfo);
	        if (rightControllerRaycastHitInfo.collider != null && rightControllerRaycastHitInfo.collider.transform.GetComponent<AnchorPrefab>() == null) //Not hitting an anchor
	        {
		        AnchorDisplayRight.gameObject.SetActive(true);
		        AnchorDisplayRight.transform.SetPositionAndRotation(rightControllerRaycastHitInfo.point,rightController.transform.rotation);
	        }
	        else
	        {
		        AnchorDisplayRight.gameObject.SetActive(false);
	        }
        }

        private static class ButtonFacade
        {
	        public static bool AButtonPressed =>
		        WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right,WVR_InputId.WVR_InputId_Alias1_A);
	        public static bool BButtonPressed => 
		        WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Right,WVR_InputId.WVR_InputId_Alias1_B); 
	        public static bool XButtonPressed =>
		        WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left,WVR_InputId.WVR_InputId_Alias1_X);
	        public static bool YButtonPressed => 
		        WXRDevice.ButtonPress(WVR_DeviceType.WVR_DeviceType_Controller_Left,WVR_InputId.WVR_InputId_Alias1_Y);
        }
	}
}