using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityStandardAssets.Vehicles.Car;
using UnityEngine.SceneManagement;

using UnityStandardAssets.CrossPlatformInput;
using System.Collections.Generic;

public class UISystem : MonoSingleton<UISystem> {

    public CarController carController;
    public string GoodCarStatusMessage;
    public string BadSCartatusMessage;
    public Text MPH_Text;
    public Image MPH_Animation;
    public Text Angle_Text;
    public Text RecordStatus_Text;
	public Text DriveStatus_Text;
	public Text SaveStatus_Text;
	public Text Cruise_Text;
    public GameObject RecordingPause; 
	public GameObject RecordDisabled;
	public bool isTraining = false;

    private bool recording;
    private float topSpeed;
	private bool saveRecording;

	private IDictionary<string, bool> buttonDepressed;
	private IDictionary<string, int> dpadDepressed;
	private Vector3 saved_position;
	private Quaternion saved_rotation;

    // Use this for initialization
    void Start() {
		Debug.Log (isTraining);
        topSpeed = carController.MaxSpeed;
        recording = false;
        RecordingPause.SetActive(false);
		RecordStatus_Text.text = "RECORD";
		DriveStatus_Text.text = "";
		SaveStatus_Text.text = "";
		Cruise_Text.text = "";
		SetAngleValue(0);
        SetMPHValue(0);
		if (!isTraining) {
			DriveStatus_Text.text = "Mode: Autonomous";
			RecordDisabled.SetActive (true);
			RecordStatus_Text.text = "";
		} 

		buttonDepressed = new Dictionary<string, bool> ();
		buttonDepressed["A Button"] = false;
		buttonDepressed["B Button"] = false;
		buttonDepressed["X Button"] = false;
		buttonDepressed["Y Button"] = false;

		dpadDepressed = new Dictionary<string, int> ();
		dpadDepressed["Horizontal - DPad"] = 0;
		dpadDepressed["Vertical - DPad"] = 0;

		carController.GetTransform(out saved_position, out saved_rotation); // remember start location, initially
			
    }

    public void SetAngleValue(float value)
    {
        Angle_Text.text = value.ToString("N2") + "°";
    }

    public void SetMPHValue(float value)
    {
        MPH_Text.text = value.ToString("N2");
        //Do something with value for fill amounts
        MPH_Animation.fillAmount = value/topSpeed;
    }

    public void ToggleRecording()
    {
		// Don't record in autonomous mode
		if (!isTraining) {
			return;
		}

        if (!recording)
        {
			if (carController.checkSaveLocation()) 
			{
				recording = true;
				RecordingPause.SetActive (true);
				RecordStatus_Text.text = "RECORDING";
				carController.IsRecording = true;
			}
        }
        else
        {
			saveRecording = true;
			carController.IsRecording = false;
        }
    }

	public void ToggleCruise() {
		carController.Cruising = !carController.Cruising;
	}
	
    void UpdateCarValues()
    {
        SetMPHValue(carController.CurrentSpeed);
        SetAngleValue(carController.CurrentSteerAngle);
    }

	bool GamepadButtonReleased(string button) {
		var value = CrossPlatformInputManager.GetAxis(button);

		if (value != 0 && !buttonDepressed[button] ) { // set flag for button being depressed, first time in series
			buttonDepressed[button] = true;
		} else if(value == 0 && buttonDepressed[button]) { // handles the release
			buttonDepressed[button] = false;
			return true;
		}  
		return false; // either wasn't pressed or remains pressed
	}

	int GamepadDpadReleased(string axisName) {
		// axisName = Horizontal - DPad or Vertical - DPad
		// returns 0 for no release, or value (-1,1) upon release
		var value = Mathf.RoundToInt(CrossPlatformInputManager.GetAxis(axisName));

		int result = 0;
		if (value != 0 && dpadDepressed[axisName] == 0) { // set flag for dpad being depressed, first time in series
			result = 0;
			dpadDepressed[axisName] = value;
		} else if (value == 0 && dpadDepressed[axisName] != 0) { // handles the release
			result = dpadDepressed [axisName];
			dpadDepressed[axisName] = 0;
			Debug.Log ("dpad was clicked, axis = " + axisName + " result is " + result);
		}  
		return result; // either wasn't pressed or remains pressed
	}


	// Update is called once per frame
	void Update () {

        // Easier than pressing the actual button :-)
        // Should make recording training data more pleasant.

		if (carController.getSaveStatus ()) {
			SaveStatus_Text.text = "Capturing Data: " + (int)(100 * carController.getSavePercent ()) + "%";
			//Debug.Log ("save percent is: " + carController.getSavePercent ());
		} 
		else if(saveRecording) 
		{
			SaveStatus_Text.text = "";
			recording = false;
			RecordingPause.SetActive(false);
			RecordStatus_Text.text = "RECORD";
			saveRecording = false;
		}

		if (Input.GetKeyDown(KeyCode.R)|| GamepadButtonReleased("B Button")) { // B Button is Red - Record
            ToggleRecording();
        }

		if (Input.GetKeyDown (KeyCode.Space) || GamepadButtonReleased("A Button")) { // A Button is Green - Cruise Control
			ToggleCruise ();
		}

		// save car's current location
		if (GamepadButtonReleased("X Button")) { // X Button is Blue
			carController.GetTransform(out saved_position, out saved_rotation);
			Debug.Log("Saving Location at: " + saved_position + " | " + saved_rotation);

		}

		// restore car's previous location
		if (GamepadButtonReleased("Y Button")) { // Y Button is Yellow -- Caution!
			carController.JumpTo(saved_position, saved_rotation);
			Debug.Log ("Jumping to : " + saved_position + " | " + saved_rotation);
		}

		// bump the max speed up and down
		int dPadStatus = GamepadDpadReleased("Vertical - DPad");
		if (dPadStatus != 0) {
			float newSpeed = carController.incrementSpeed (dPadStatus > 0 ? 1.0f : -1.0f);
			Debug.Log ("Maxspeed set to : " + newSpeed);
		}

		Cruise_Text.text = carController.Cruising ? "Cruise (" + carController.MaxSpeed + " MPH)" : ""; 

		//  nudge the car left and right
		dPadStatus = GamepadDpadReleased("Horizontal - DPad");
		float bump = 0.5f;
		if (dPadStatus != 0) {
			if (dPadStatus > 0) 
				carController.BumpLeft (bump);
			else
				carController.BumpRight (bump);
			Debug.Log ("Car bumped by : " + bump);
		}

		if (!isTraining) 
		{
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) 
			{
				DriveStatus_Text.color = Color.red;
				DriveStatus_Text.text = "Mode: Manual";
			} 
			else 
			{
				DriveStatus_Text.color = Color.white;
				DriveStatus_Text.text = "Mode: Autonomous";
			}
		}
			
	    if(Input.GetKeyDown(KeyCode.Escape))
        {
            //Do Menu Here
            SceneManager.LoadScene("MenuScene");
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            //Do Console Here
        }

        UpdateCarValues();
    }
}
