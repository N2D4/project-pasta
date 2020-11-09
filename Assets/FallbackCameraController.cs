//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Controls for the non-VR debug camera
//
//=============================================================================

/**
 * Original license as below:

	Copyright (c) Valve Corporation
	All rights reserved.

	Redistribution and use in source and binary forms, with or without modification,
	are permitted provided that the following conditions are met

	1. Redistributions of source code must retain the above copyright notice, this
	list of conditions and the following disclaimer.

	2. Redistributions in binary form must reproduce the above copyright notice,
	this list of conditions and the following disclaimer in the documentation andor
	other materials provided with the distribution.

	3. Neither the name of the copyright holder nor the names of its contributors
	may be used to endorse or promote products derived from this software without
	specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS AS IS AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR
	ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
	ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using UnityEngine;
using System.Collections;

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	[RequireComponent( typeof( Camera ) )]
	public class FallbackCameraController : MonoBehaviour
	{
		public float speed = 4.0f;
		public float shiftSpeed = 16.0f;
		public bool showInstructions = true;

		private Vector3 startEulerAngles;
		private Vector3 startMousePosition;
		private float realTime;

		//-------------------------------------------------
		void OnEnable()
		{
			realTime = Time.realtimeSinceStartup;
		}


		//-------------------------------------------------
		void Update()
		{
			float forward = 0.0f;
			if ( Input.GetKey( KeyCode.W ) || Input.GetKey( KeyCode.UpArrow ) )
			{
				forward += 1.0f;
			}
			if ( Input.GetKey( KeyCode.S ) || Input.GetKey( KeyCode.DownArrow ) )
			{
				forward -= 1.0f;
			}

			float right = 0.0f;
			if ( Input.GetKey( KeyCode.D ) || Input.GetKey( KeyCode.RightArrow ) )
			{
				right += 1.0f;
			}
			if ( Input.GetKey( KeyCode.A ) || Input.GetKey( KeyCode.LeftArrow ) )
			{
				right -= 1.0f;
			}

            float up = 0.0f;
            if (Input.GetKey(KeyCode.Space))
            {
                up += 1.0f;
            }
            if (Input.GetKey(KeyCode.LeftControl))
            {
                up -= 1.0f;
            }

            float currentSpeed = speed;
			if ( Input.GetKey( KeyCode.LeftShift ) || Input.GetKey( KeyCode.RightShift ) )
			{
				currentSpeed = shiftSpeed;
			}

			float realTimeNow = Time.realtimeSinceStartup;
			float deltaRealTime = realTimeNow - realTime;
			realTime = realTimeNow;

			Vector3 delta = new Vector3( right, up, forward ) * currentSpeed * deltaRealTime;

			transform.position += transform.TransformDirection( delta );

			Vector3 mousePosition = Input.mousePosition;

			if ( Input.GetMouseButtonDown( 1 ) /* right mouse */)
			{
				startMousePosition = mousePosition;
				startEulerAngles = transform.localEulerAngles;
			}

			if ( Input.GetMouseButton( 1 ) /* right mouse */)
			{
				Vector3 offset = mousePosition - startMousePosition;
				transform.localEulerAngles = startEulerAngles + new Vector3( -offset.y * 360.0f / Screen.height, offset.x * 360.0f / Screen.width, 0.0f );
			}
		}


		//-------------------------------------------------
		void OnGUI()
		{
			if ( showInstructions )
			{
				GUI.Label( new Rect( 10.0f, 10.0f, 600.0f, 400.0f ),
					"WASD/Arrow Keys to translate the camera\n" +
					"Right mouse click to rotate the camera\n" +
					"Left mouse click for standard interactions.\n" );
			}
		}
	}
}
