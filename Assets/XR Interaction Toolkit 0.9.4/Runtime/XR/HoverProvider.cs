using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.XR.Interaction.Toolkit
{
    public class HoverProvider : MonoBehaviour
    {
        // A list of controllers that can activate/deactive head steering. If an XRController is not enabled, or does not have input actions enabled, head steering will not work.
        [SerializeField]
        [Tooltip("A list of controllers that can activate/deactive hovering. If an XRController is not enabled, or does not have input actions enabled, hovering will not work.")]
        List<XRController> m_Controllers = new List<XRController>();
        public List<XRController> Controllers { get { return m_Controllers; } set { m_Controllers = value; } }

        // Contols the amount of jetpack thrust
        [SerializeField]
        [Tooltip("Contols the amount of jetpack thrust.")]
        float m_Thrust = 50;
        public float Thrust { get { return m_Thrust; } set { m_Thrust = value; } }


        Rigidbody RigidBodyCharacter;
        CapsuleCollider CapsuleColliderCharacter;

        [HideInInspector]
        bool m_Hovering = false;
        public bool Hovering { get { return m_Hovering; } }


        void Start()
        {
            RigidBodyCharacter = GetComponent<Rigidbody>();
            CapsuleColliderCharacter = GetComponent<CapsuleCollider>();
        }

        // Update is called once per frame
        void Update()
        {
            RaycastHit hit;

            Physics.Raycast(transform.position, Vector3.up, out hit);

            InputFeatureUsage<bool> triggerFeature = CommonUsages.triggerButton;

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

                    // Try to get the current state of the device's trigger button.
                    bool button;

                    if (device.TryGetFeatureValue(triggerFeature, out button))
                    {
                        AudioSource audioSource = GetComponent<AudioSource>();

                        if (button && hit.distance > .3f)
                        {
                            GetComponent<VirtualBody>().OnFloor = false;
                            if (!audioSource.isPlaying)
                            {
                                audioSource.Play();
                            }
                            RigidBodyCharacter.AddForce(new Vector3(0, Thrust * Time.deltaTime, 0), ForceMode.Force);
                            m_Hovering = true;
                        }
                        else
                        {
                            m_Hovering = false;
                            if (audioSource.isPlaying) audioSource.Pause();
                        }
                    }
                }
            }
        }
    }
}