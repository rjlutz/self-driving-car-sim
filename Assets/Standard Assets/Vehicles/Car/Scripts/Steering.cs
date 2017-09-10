using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

//using UnityEditor;


namespace UnityStandardAssets.Vehicles.Car
{
    public class Steering
    {

        public float H { get; private set; }
        public float V { get; private set; }
        public bool mouse_hold;
		public float mouse_start;
		//private SerializedProperty axisArray;
		private CarController m_Car;

		//public enum InputType { KeyOrMouseButton, MouseMovement, JoystickAxis};

		// logitech F310, bottom switch set to 'D' setting
		// 
		// 0 horizontal
		// 1 vertical
		// 2 right stick x axis 
		// 3 right stick y axis
		// 4 DPad x
		// 5 DPad y 

		//X joystick button 0
		//Y joystick button 3
		//A joystick button 1
		//B joystick button 2

		public Steering(CarController car) {
			m_Car = car;
		}


        // Use this for initialization
        public void Start() {
            H = 0f;
            V = 0f;
            mouse_hold = false;

			// need to interrogate axes, can be removed later 
			//var inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
			//SerializedObject obj = new SerializedObject(inputManager);
			//axisArray = obj.FindProperty("m_Axes");

        }

        // Update is called once per frame
		public void UpdateValues() {
			if (m_Car.Cruising) 
                V = 0.4f; // gets to max speed at a gradual pace
            else
                V = CrossPlatformInputManager.GetAxis("Vertical");

			if (Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.LeftArrow)) {
				if (H > -1.0)
					H -= 0.05f;
			} else if (Input.GetKey (KeyCode.D) || Input.GetKey (KeyCode.RightArrow)) {
				if (H < 1.0)
					H += 0.05f;
			} else if (Input.GetMouseButton(0)) {
				// get the mouse position
				float mousePosition = Input.mousePosition.x;

				// check if its the first time pressing down on mouse button
				if (!mouse_hold) {
					// we are now holding down the mouse
					mouse_hold = true;
					// set the start reference position for position tracking
					mouse_start = mousePosition;
				}
			
				// This way h is [-1, -1]
				// it's quite hard to get a max or close to max
				// steering angle unless it's actually wanted.
				H = Mathf.Clamp ( (mousePosition - mouse_start)/(Screen.width/6), -1, 1);
            } else {
				mouse_hold = false;    // reset
				H = CrossPlatformInputManager.GetAxis ("Horizontal - Right Joystick");
				//LogNonZero();  // DEBUG
            }
        }

//		private void LogNonZero() {
//			for (int i = 0; i < axisArray.arraySize; ++i) {
//				var axis = axisArray.GetArrayElementAtIndex (i);
//				var axisName = axis.FindPropertyRelative ("m_Name").stringValue;
//				if (CrossPlatformInputManager.GetAxis (axisName) != 0) {
//					var axisValue = axis.FindPropertyRelative ("axis").intValue;
//					var axisType = (InputType)axis.FindPropertyRelative ("type").intValue;
//					Debug.Log ("name = " + axisName + " axisVal = " + axisValue + " inputType = " + axisType + 
//						" " + CrossPlatformInputManager.GetAxis (axisName));
//				}
//			}
//		}
    }
}