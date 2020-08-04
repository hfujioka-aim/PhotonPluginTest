// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CameraWork.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking Demos
// </copyright>
// <summary>
//  Used in PUN Basics Tutorial to deal with the Camera work to follow the player
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace Photon.Pun.Demo.PunBasics
{
	public static class VecEx
	{
		public static Vector2 ToXZ(this in Vector3 vec3)
		{
			return new Vector2(vec3.x, vec3.z);
		}

		public static Vector3 AddY(this in Vector2 vec2, in float y)
		{
			return new Vector3(vec2.x, y, vec2.y);
		}
	}

	/// <summary>
	/// Camera work. Follow a target
	/// </summary>
	public class CameraWork : MonoBehaviour
	{
		#region Private Fields

		[Tooltip("The distance in the local x-z plane to the target")]
		[SerializeField]
		private float distance = 7.0f;

		[Tooltip("The height we want the camera to be above the target")]
		[SerializeField]
		private float height = 3.0f;

		[Tooltip("Allow the camera to be offseted vertically from the target, for example giving more view of the sceneray and less ground.")]
		[SerializeField]
		private Vector3 centerOffset = Vector3.zero;

		[Tooltip("Set this as false if a component of a prefab being instanciated by Photon Network, and manually call OnStartFollowing() when and if needed.")]
		[SerializeField]
		private bool followOnStart = false;

		[Tooltip("The Smoothing for the camera to follow the target")]
		[SerializeField]
		private float smoothSpeed = 0.125f;

		// cached transform of the target
		Transform cameraTransform;

		// maintain a flag internally to reconnect if target is lost or camera is switched
		bool isFollowing;

		// Cache for camera offset
		Vector3 cameraOffset = Vector3.zero;
		#endregion

		#region MonoBehaviour Callbacks

		/// <summary>
		/// MonoBehaviour method called on GameObject by Unity during initialization phase
		/// </summary>
		void Start()
		{
			// Start following the target if wanted.
			if (followOnStart)
			{
				OnStartFollowing();
			}
		}

		void LateUpdate()
		{
			// The transform target may not destroy on level load, 
			// so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
			if (cameraTransform == null && isFollowing)
			{
				OnStartFollowing();
			}

			// only follow is explicitly declared
			if (isFollowing) {
				Follow ();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Raises the start following event. 
		/// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
		/// </summary>
		public void OnStartFollowing()
		{
			cameraTransform = Camera.main.transform;
			isFollowing = true;
			// we don't smooth anything, we go straight to the right camera shot
			Cut();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Follow the target smoothly
		/// </summary>
		void Follow()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;

			// cameraTransform.position = Vector3.Slerp(cameraTransform.position, this.transform.position +this.transform.TransformVector(cameraOffset), smoothSpeed*Time.deltaTime);
			var pos = this.transform.position;
			var cpos = cameraTransform.position;
			var dest = this.transform.TransformPoint(cameraOffset);
			var angle = -Vector2.SignedAngle(new Vector2(cpos.x, cpos.z) - new Vector2(pos.x, pos.z), new Vector2(dest.x, dest.z) - new Vector2(pos.x, pos.z));

			var dist = Vector2.Distance(cpos.ToXZ(), pos.ToXZ()) - cameraOffset.ToXZ().magnitude;
			cameraTransform.position = Vector2.MoveTowards(cpos.ToXZ(), pos.ToXZ(), dist).AddY(pos.y + height);
			if (Mathf.Abs(dist) < 0.00001) {
				cameraTransform.RotateAround(pos, Vector3.up, angle * smoothSpeed * Time.deltaTime);
			}
			cameraTransform.LookAt(pos + centerOffset);
		}

		void Cut()
		{
			cameraOffset.z = -distance;
			cameraOffset.y = height;

			cameraTransform.position = this.transform.position + this.transform.TransformVector(cameraOffset);

			cameraTransform.LookAt(this.transform.position + centerOffset);
		}
		#endregion
	}
}
