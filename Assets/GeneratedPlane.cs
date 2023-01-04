using System;
using UnityEngine;
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
            if (go == null) return; // ? operator doesn't work on unity objects, explicitly test for null
            
            var meshFilter = go.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh) //not sure if mesh or sharedmesh is right here.. but think sharedmesh is safer mayber?
            {
                UnityEngine.Object.Destroy(meshFilter.sharedMesh); 
            }

            UnityEngine.Object.Destroy(go);
            go = null;
        }
        

        public void Dispose()
        {
            DestroyGameObject();
        }
    }
}