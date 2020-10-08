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
    // The head steering provider allows the user to steer through the environment using the forward direction of the user's head (i.e., the main camera).
    [AddComponentMenu("XRST/Locomotion/HeadSteeringProvider")]
    public class HeadSteeringProvider : LocomotionProvider
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

        // A list of controllers that can activate/deactive head steering. If an XRController is not enabled, or does not have input actions enabled, head steering will not work.
        [SerializeField]
        [Tooltip("A list of controllers that can activate/deactive head steering. If an XRController is not enabled, or does not have input actions enabled, head steering will not work.")]
        List<XRController> m_Controllers = new List<XRController>();
        public List<XRController> Controllers { get { return m_Controllers; } set { m_Controllers = value; } }

        // The speed of the user's virtual travel.
        [SerializeField]
        [Tooltip("The speed of the user's virtual travel.")]
        float m_Speed = 1.0f;
        public float Speed { get { return m_Speed; } set { m_Speed = value; } }

        //// Whether to constrain virtual travel to the horizontal plane.
        //[SerializeField]
        //[Tooltip("Whether to constrain virtual travel to the horizontal plane.")]
        //bool m_HorizontalOnly = true;
        //public bool HorizontalOnly { get { return m_HorizontalOnly; } set { m_HorizontalOnly = value; } }

        // Reset function for initializing the walking provider.
        void Reset()
        {
            // Attempt to fetch the locomotion system.
            system = FindObjectOfType<LocomotionSystem>();
            // Did not find a locomotion system.
            if (system == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HeadSteeringProvider]: Did not find a LocomotionSystem in the scene.");
            }

            // Attempt to fetch the rig.
            Rig = FindObjectOfType<XRRig>();
            // Did not find a rig.
            if (Rig == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HeadSteeringProvider]: Did not find an XRRig in the scene.");
            }

            // Attempt to fetch the Camera.
            MainCamera = Camera.main;
            // Did not find the main camera.
            if (MainCamera == null)
            {
                Debug.LogWarning("[" + gameObject.name + "][HeadSteeringProvider]: Did not find a main Camera in the scene.");
            }

            // Select all controllers by default.
            Controllers = new List<XRController>(FindObjectsOfType<XRController>());
        }

        // Start is called before the first frame update.
        void Start()
        {
            // Warn if there are no controllers.
            if (Controllers.Count == 0)
            {
                Debug.LogWarning("[" + gameObject.name + "][HeadSteeringProvider]: No controllers are selected.");
            }

            // Check that each controller is valid.
            for (int i = 0; i < Controllers.Count; i++)
            {
                if (Controllers[i] == null)
                {
                    Debug.LogWarning("[" + gameObject.name + "][HeadSteeringProvider]: No controller selected at index " + i + ".");
                }
            }
        }

        // Update is called once per frame.
        void Update()
        {

            // If the rig and main camera are valid.
            if (Rig != null && MainCamera != null)
            {
                // Determine whether head steering is active and what the magnitude is.
                bool steering = false;
                float magnitude = 0.0f;

                // Get the primary 2D axis and button.
                InputFeatureUsage<Vector2> axisFeature = CommonUsages.primary2DAxis;
                InputFeatureUsage<bool> buttonFeature = CommonUsages.primary2DAxisClick;

                // For each controller.
                for (int i = 0; i < Controllers.Count; i++)
                {
                    // Fetch the controller.
                    XRController controller = Controllers[i];
                    // If the controller is valid and enabled.
                    if (controller != null && controller.enableInputActions)
                    {
                        // Fetch the controller's device.
                        InputDevice device = controller.inputDevice;

                        // Try to get the current state of the device's primary 2D axis and button.
                        Vector2 axis;
                        bool button;
                        if (device.TryGetFeatureValue(axisFeature, out axis) && device.TryGetFeatureValue(buttonFeature, out button))
                        {
                            // Activate steering and add the magnitude if the button is down.
                            if (button)
                            {
                                steering = true;
                                magnitude += axis.y;
                            }
                        }
                    }
                }

                // If steering is active, move the rig.
                if (steering)
                {
                    // Calculate the movement of the rig based on the user's head direction.
                    Vector3 movement = MainCamera.transform.forward;

                    movement.y = 0.0f;
                    movement.Normalize();

                    // Scale the travel by the magnitude and speed per second (which requires deltaTime).
                    movement *= magnitude * Speed * Time.deltaTime;

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
            }
        }
    }
}