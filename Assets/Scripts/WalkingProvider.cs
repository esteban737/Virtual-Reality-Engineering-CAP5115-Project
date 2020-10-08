/*
*   Copyright (C) 2020 University of Central Florida, created by Dr. Ryan P. McMahan.
*
*   This program is free software: you can redistribute it and/or modify
*   it under the terms of the GNU General Public License as published by
*   the Free Software Foundation, either version 3 of the License, or
*   (at your option) any later version.
*
*   This program is distributed in the hope that it will be useful,
*   but WITHOUT ANY WARRANTY; without even the implied warranty of
*   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*   GNU General Public License for more details.
*
*   You should have received a copy of the GNU General Public License
*   along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*   Primary Author Contact:  Dr. Ryan P. McMahan <rpm@ucf.edu>
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Interaction.Toolkit
{
    // The walking provider is a locomotion provider that keeps the user from walking through objects by moving the rig relative to the environment.
    [AddComponentMenu("XRST/Locomotion/WalkingProvider")]
    public class WalkingProvider : LocomotionProvider
    {
        // The XRRig that represents the user's rig.
        [SerializeField]
        [Tooltip("The XRRig that represents the user's rig.")]
        XRRig m_Rig;
        public XRRig Rig { get { return m_Rig; } set { m_Rig = value; } }

        // The Camera that represents the main camera or user's head.
        [SerializeField]
        [Tooltip("The Camera that represents the main camera or user's head.")]
        Camera m_MainCamera;
        public Camera MainCamera { get { return m_MainCamera; } set { m_MainCamera = value; } }

        // A GameObject that represents the user's virtual body.
        [SerializeField]
        [Tooltip("A GameObject that represents the user's virtual body.")]
        GameObject m_VirtualBody;
        public GameObject VirtualBody { get { return m_VirtualBody; } set { m_VirtualBody = value; } }

        // The CharacterController that controls the user's virtual body.
        [SerializeField]
        [Tooltip("The CharacterController that controls the user's virtual body.")]
        CapsuleCollider m_Character;
        public CapsuleCollider Character { get { return m_Character; } set { m_Character = value; } }

        // Whether to apply gravity to the user's virtual body.
        [SerializeField]
        [Tooltip("Whether to apply gravity to the user's virtual body.")]
        bool m_UseGravity = true;
        public bool UseGravity { get { return m_UseGravity; } set { m_UseGravity = value; } }

        // The rigidbody of the virtual body 
        [SerializeField]
        [Tooltip("The rigidbody of the virtual body.")]
        Rigidbody m_RigidBodyCharacter;
        public Rigidbody RigidBodyCharacter { get { return m_RigidBodyCharacter; } set { m_RigidBodyCharacter = value; } }

        // The class of the virtual body that provides behavioral information such as collision detection
        [SerializeField]
        [Tooltip("The class of the virtual body that provides behavioral information such as collision detection.")]
        VirtualBody m_VirtualBodyScript;
        public VirtualBody VirtualBodyScript { get { return m_VirtualBodyScript; } set { m_VirtualBodyScript = value; } }

        private float offset = .2f;

        // Reset function for initializing the walking provider.
        void Reset()
        {
            // Attempt to fetch the locomotion system.
            system = FindObjectOfType<LocomotionSystem>();
            // Did not find a locomotion system.
            if (system == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][WalkingProvider]: Did not find a LocomotionSystem in the scene.");
            }

            // Attempt to fetch the rig.
            Rig = FindObjectOfType<XRRig>();
            // Did not find a rig.
            if (Rig == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][WalkingProvider]: Did not find an XRRig in the scene.");
            }

            // Attempt to fetch the Camera.
            MainCamera = Camera.main;
            // Did not find the main camera.
            if (MainCamera == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][WalkingProvider]: Did not find a main Camera in the scene.");
            }

            // Attempt to find the virtual body gameobject.
            if (GameObject.Find("Virtual Body") != null)
            {
                // Set the virtual body.
                VirtualBody = GameObject.Find("Virtual Body").gameObject;
            }
            // Did not find the virtual body gameobject.
            else
            {
                // Create a virtual body gameobject.
                VirtualBody = new GameObject("Virtual Body");
                // Notify the developer.
                Debug.Log("[" + gameObject.name + "][WalkingProvider]: Added a new GameObject 'Virtual Body' to the scene.");
            }

            // If the virtual body exists.
            if (VirtualBody != null)
            {
                // Attempt to fetch the character controller.
                Character = VirtualBody.GetComponent<CapsuleCollider>();

                // Did not find a character controller.
                if (Character == null)
                {
                    // Add a character controller. 
                    Character = VirtualBody.AddComponent<CapsuleCollider>();
                    // Notify the developer.
                    Debug.Log("[" + gameObject.name + "][WalkingProvider]: Added a CharacterController to the 'Virtual Body' gameobject.");
                }
            }
        }

        // Start is called before the first frame update.
        void Start()
        {
            // Move the virtual body to match the main camera's position.
            if (VirtualBody != null)
            {
                VirtualBody.transform.position = MainCamera.transform.position;
            }
        }

        // Update is called once per frame.
        void Update()
        {
            // If the rig, main camera, and character controller are valid.
            if (Rig != null && MainCamera != null && Character != null)
            {
                // Update the character controller's height.
                Character.height = MainCamera.transform.position.y - Rig.transform.position.y + offset;

                // Update the character controller's center.
                Vector3 center = Character.center;
                center.y = -Character.height / 2.0f;
                Character.center = center;

                Vector3 newPos = MainCamera.transform.position;

                // Maintain Old RigidBody Y Position
                newPos.y = RigidBodyCharacter.position.y;

                RigidBodyCharacter.MovePosition(newPos);

                // Begin locomotion.
                if (CanBeginLocomotion() && BeginLocomotion())
                {
                    // Determine the character controller's world location.
                    Vector3 characterLocation = Character.transform.position;

                    characterLocation.y -= offset;

                    if (!VirtualBodyScript.CollisionFlag)
                    {
                        characterLocation.x = MainCamera.transform.position.x;
                        characterLocation.z = MainCamera.transform.position.z;
                    }

                    // Move the rig and camera to the character controller's world location.
                    Rig.MoveCameraToWorldLocation(characterLocation);
                    // End locomotion.
                    EndLocomotion();
                }
            }
        }
    }
}
