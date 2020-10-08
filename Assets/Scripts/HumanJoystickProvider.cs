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
    // The human joystick provider allows the user to steer through the environment by stepping away from the center of the tracking space.
    [AddComponentMenu("XRST/Locomotion/HumanJoystickProvider")]
    public class HumanJoystickProvider : LocomotionProvider
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

        // The GameObject that represents the no-travel zone.
        [SerializeField]
        [Tooltip("The GameObject that represents the no-travel zone.")]
        GameObject m_NoTravelZone;
        public GameObject NoTravelZone { get { return m_NoTravelZone; } set { m_NoTravelZone = value; } }

        // The radius of the no-travel zone.
        [SerializeField]
        [Tooltip("The radius of the no-travel zone.")]
        float m_Radius = 0.5f;
        public float Radius { get { return m_Radius; } set { m_Radius = value; } }

        // The speed of the user's virtual travel at the radius.
        [SerializeField]
        [Tooltip("The speed of the user's virtual travel at the radius.")]
        float m_Speed = 1.0f;
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }

        // Reset function for initializing the walking provider.
        void Reset()
        {
            // Attempt to fetch the locomotion system.
            system = FindObjectOfType<LocomotionSystem>();
            // Did not find a locomotion system.
            if (system == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HumanJoystickProvider]: Did not find a LocomotionSystem in the scene.");
            }

            // Attempt to fetch the rig.
            Rig = FindObjectOfType<XRRig>();
            // Did not find a rig.
            if (Rig == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HumanJoystickProvider]: Did not find an XRRig in the scene.");
            }

            // Attempt to fetch the Camera.
            MainCamera = Camera.main;
            // Did not find the main camera.
            if (MainCamera == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HumanJoystickProvider]: Did not find a main Camera in the scene.");
            }
        }

        // Start is called before the first frame update.
        void Start()
        {
            // Warn if there is not a no travel zone gameobject.
            if (NoTravelZone == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HumanJoystickProvider]: A no-travel zone gameobject is not provided.");
            }
        }

        // Update is called once per frame.
        void Update()
        {
            // If the rig and main camera are valid.
            if (Rig != null && MainCamera != null)
            {
                // Calculate the travel vector based on the user's head and the rig.
                Vector3 travelVector = MainCamera.transform.position - Rig.transform.position;

                // Ignore any height value.
                travelVector.y = 0.0f;

                // Activate human joystick if the travel vector is larger than the radius.
                if (travelVector.magnitude >= Radius)
                {
                    // Calculate the ratio of the travel vector compared to the radius.
                    float ratio = travelVector.magnitude / Radius;

                    // Normalize the travel vector for multiplication.
                    travelVector.Normalize();

                    // Scale the travel vector by the ratio and speed per second (which requires deltaTime).
                    Vector3 movement = travelVector * ratio * Speed * Time.deltaTime;

                    // Begin locomotion.
                    if (CanBeginLocomotion() && BeginLocomotion())
                    {
                        // Determine the camera's new world location.
                        Vector3 newLocation = MainCamera.transform.position + movement;
                        // Move the rig and camera to the character controller's world location.
                        Rig.MoveCameraToWorldLocation(newLocation);
                        // End locomotion.
                        EndLocomotion();
                    }
                }

                // If the no-travel zone is valid.
                if (NoTravelZone != null)
                {
                    // Move the no-travel zone to match the rig.
                    NoTravelZone.transform.position = Rig.transform.position;
                    // Scale the NoTravelZone to match the radius.
                    float originalHeight = NoTravelZone.transform.localScale.y;
                    NoTravelZone.transform.localScale = new Vector3(Radius, originalHeight, Radius);
                }
            }
        }
    }
}