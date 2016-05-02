
/* SineWave
 * v1.1
 * 
 * Written by Nick Yonge
 * 
 * Copyright 2016 Nick Yonge
 * 
 * Completely free for any use (private, public, commercial, educational)
 * 
 * */

/* 
	
 HOW TO USE:
	
 1) Drop me on a GameObject
	
 2) Select whether I affect the Position, Rotation, or Scale (default Position)
    (If you want to affect more than one of the above, you CAN, but it's highly recommended you use two different instances of this script.)
	
 3) Set the Amplitude (intensity) and Omega (speed) of the sine wave, along the axes that you want to affect
    (You can optionally set an offset too. For example, use a slight offset (0.1, 0.2, 0.3, 0.4...) along several non-randomized sines to create a "wave" movement effect.)

 4) You're good to go! Play around with other properties or read the summaries below for more details.

 NOTE: For IgnoreGameplayPause to work, you need to un-comment the line it appears in (line 162), and add your own check to see if the game's paused.

*/


using UnityEngine;
using System.Collections;

//disable "assigned but unused variable" warning in case you don't use IgnoreGameplayPaused
#pragma warning disable 0414

public class SineWave : MonoBehaviour {

	#region VariableDefinition

	//VARIABLE DEFINITION

	/// <summary>
	/// Will this sine wave affect the Position of this transform?
	/// </summary>
	public bool Position = true;

	/// <summary>
	/// Will this sine wave affect the Rotation (euler) of this transform?
	/// </summary>
	public bool Rotation = false;
	/// <summary>
	/// Will this sine wave affect the Scale of this transform?
	/// </summary>
	public bool Scale = false;


	/// <summary>
	/// The amplitude (range/intensity) of this sine wave, split along 3 axes
	/// </summary>
	public Vector3 Amplitude = Vector3.zero;
	/// <summary>
	/// The omega (speed/frequency) of this sine wave, split along 3 axes
	/// </summary>
	public Vector3 Omega = Vector3.zero;
	/// <summary>
	/// The offset of this sine wave. 
	/// </summary>
	public Vector3 Offset = Vector3.zero;


	/// <summary>
	/// If true, Bounces (i.e. always positive or negative) on the X axis. If false, smoothly swings beyond positive and negative ranges.
	/// </summary>
	public bool BounceX = false;
	/// <summary>
	/// If true, Bounces (i.e. always positive or negative) on the Y axis. If false, smoothly swings beyond positive and negative ranges.
	/// </summary>
	public bool BounceY = false;
	/// <summary>
	/// If true, Bounces (i.e. always positive or negative) on the Z axis. If false, smoothly swings beyond positive and negative ranges.
	/// </summary>
	public bool BounceZ = false;


	/// <summary>
	/// Uniformly scale? If true, all axes will read from the X axis. This is useful for scaling.
	/// </summary>
	public bool Uniform = false;
	/// <summary>
	/// If true, this sine wave affects this transform in local space. If false, affects it in global space. NOTE: Scale is always local.
	/// </summary>
	public bool Local = true;
	/// <summary>
	/// Randomize this sine wave on start?
	/// </summary>
	public bool RandomizeOnStart = false;
	/// <summary>
	/// Randomize this sine wave whenever the amplitude or omega is changed?
	/// </summary>
	public bool RandomizeOnChange = false;


	/// <summary>
	/// Does it use the initial position/rotation/scale of the gameObject as the "zero" from which the sine moves?
	/// NOTE: If it affects more than one transform aspect (Position/Rotation/Scale), it will first read this from Position, then the Rotation, then the Scale.
	/// This is highly discouraged, you should only affect one of those on a given SineWave component.
	/// Add two SineWaves to the same object if you want to affect more than one.
	/// </summary>
	public bool UseStartAsBasePosition = true;
	/// <summary>
	/// The "zero" from which the sine moves back and forth from
	/// </summary>
	public Vector3 BasePosition = Vector3.zero;


	/// <summary>
	/// Ignore global gameplay pause function? Good for UI sine waves, leave unchecked to freeze this sine when the game pauses. Incompatible with UseGlobalIndex
	/// </summary>
	public bool IgnoreGameplayPause = false;
	/// <summary>
	/// Use Global timing rather than individual timing. Different sines that have this checked will be synchronized. Overrides any randomization.
	/// </summary>
	public bool UseGlobalIndex = false;//overrides randomize on start


	/// <summary>
	/// OPTIONALLY override the transform, so this SineWave doesn't affect its linked gameobject's transform
	/// </summary>
	public Transform OverrideTransform = null;


	//private local variables
	private Vector3 index = Vector3.zero;//the stored index of this sine wave
	private Vector3 LastAmplitude = Vector3.zero;//last referenced amplitude (used in randomization updating)
	private Vector3 LastOmega = Vector3.zero;//last referenced omega (used in randomization updating)

	#endregion

	#region Functions

	// FUNCTIONS

	/// <summary>
	/// Randomizes the indexes of this sine wave (UNLESS it uses the global index)
	/// </summary>
	public void Randomize() {
		if (!UseGlobalIndex) {
			index.x = Random.Range (0f, Mathf.Abs ((Omega.x * 1000f) * 2f));
			index.y = Random.Range (0f, Mathf.Abs ((Omega.y * 1000f) * 2f));
			index.z = Random.Range (0f, Mathf.Abs ((Omega.z * 1000f) * 2f));
		}
	}

	private void LateUpdate () {
		//check for pause
		//		NOTE: 
		//		Uncomment and replace "GameData.GamePaused()" with whatever bool/function/reference/etc
		//		you use to check if the game is paused
		//		(assuming you don't just use timeScale = 0, which would affect all sine waves)
		//if (!IgnoreGameplayPause && GameData.GamePaused ()) { return; }
		
		//update the index
		if (UseGlobalIndex) {
			//set index to global time value
			index.x = Time.timeSinceLevelLoad;
			index.y = Time.timeSinceLevelLoad;
			index.z = Time.timeSinceLevelLoad;
		} else {
			//check for re-randomization
			if (RandomizeOnChange)
				UpdateRandomization();
			//time increment the index
			index.x += Time.smoothDeltaTime;
			index.y += Time.smoothDeltaTime;
			index.z += Time.smoothDeltaTime;
		}
		//update the sine wave
		Vector3 pos = BasePosition;
		//x axis update (cosine)
		//first check that this axis is used
		if (Amplitude.x != 0f && Omega.x != 0f) {
			//check if it bounces or not
			if (BounceX) {
				//it bounces, abs lock the result so it's always on the positive/negative side of BasePosition
				if (Amplitude.x >= 0) {
					//positive, add the result (always above BasePosition)
					pos.x += Mathf.Abs (Amplitude.x * Mathf.Cos (Omega.x * (index.x + Offset.x)));
				} else {
					//negative, subtract the result (always below BasePosition)
					pos.x -= Mathf.Abs (Amplitude.x * Mathf.Cos (Omega.x * (index.x + Offset.x)));
				}
			} else {
				//doesn't bounce, add the non-Abs-locked sine wave to the position
				pos.x += Amplitude.x * Mathf.Cos (Omega.x * (index.x + Offset.x));
			}
		}
		//y axis update (sine)
		if (Uniform) {
			//uniformly scale all axes to follow the X axis
			pos.y = pos.x;
		} else {
			if (Amplitude.y != 0f && Omega.y != 0f) {
				if (BounceY) {
					if (Amplitude.y >= 0)
						pos.y += Mathf.Abs (Amplitude.y * Mathf.Sin (Omega.y * (index.y + Offset.y)));
					else
						pos.y -= Mathf.Abs (Amplitude.y * Mathf.Sin (Omega.y * (index.y + Offset.y)));
				} else
					pos.y += Amplitude.y * Mathf.Sin (Omega.y * (index.y + Offset.y));
			}
		}
		//z axis update (cosine)
		if (Uniform) {
			pos.z = pos.x;
		} else {
			if (Amplitude.z != 0f && Omega.z != 0f) {
				if (BounceZ) {
					if (Amplitude.z >= 0)
						pos.z += Mathf.Abs (Amplitude.z * Mathf.Cos (Omega.z * (index.z + Offset.z)));
					else
						pos.z -= Mathf.Abs (Amplitude.z * Mathf.Cos (Omega.z * (index.z + Offset.z)));
				} else
					pos.z += Amplitude.z * Mathf.Cos (Omega.z * (index.z + Offset.z));
			}
		}
		//affect the transform itself
		//check if it's affecting the given transform or an override transform
		if (OverrideTransform != null) {
			//check if affecting local or global position (scale is always local)
			if (Local) {
				if (Position)
					OverrideTransform.localPosition = pos;
				if (Rotation)
					OverrideTransform.localEulerAngles = pos;
			} else {
				if (Position)
					OverrideTransform.position = pos;
				if (Rotation)
					OverrideTransform.eulerAngles = pos;
			}
			if (Scale)
				OverrideTransform.localScale = pos;
		} else {
			if (Local) {
				if (Position)
					transform.localPosition = pos;
				if (Rotation)
					transform.localEulerAngles = pos;
			} else {
				if (Position)
					transform.position = pos;
				if (Rotation)
					transform.eulerAngles = pos;
			}
			if (Scale)
				transform.localScale = pos;
		}
	}

	//start the sinewave!
	private void Start() {
		//assign starting base position, if applicable
		if (UseStartAsBasePosition) {
			//check override transform
			if (OverrideTransform != null) {
				//check local or global (scale is always local)
				if (Local) {
					//check which transform dimension to use as base (recommended to not use more than one of these on the same component)
					if (Position)
						BasePosition = OverrideTransform.localPosition;
					else if (Rotation)
						BasePosition = OverrideTransform.localEulerAngles;
					else if (Scale)
						BasePosition = OverrideTransform.localScale;
				} else {
					if (Position)
						BasePosition = OverrideTransform.position;
					else if (Rotation)
						BasePosition = OverrideTransform.eulerAngles;
					else if (Scale)
						BasePosition = OverrideTransform.localScale;
				}
			} else {
				if (Local) {
					if (Position)
						BasePosition = transform.localPosition;
					else if (Rotation)
						BasePosition = transform.localEulerAngles;
					else if (Scale)
						BasePosition = transform.localScale;
				} else {
					if (Position)
						BasePosition = transform.position;
					else if (Rotation)
						BasePosition = transform.eulerAngles;
					else if (Scale)
						BasePosition = transform.localScale;
				}
			}
		}
		//force update the randomization
		if (RandomizeOnStart)
			UpdateRandomization(true);
	}
	private void UpdateRandomization(bool Force = false) {
		//checks if lastAmplitude or Omega have changed (Force updates the randomization regardless of whether Amplitude or Omega have changed)
		if (LastAmplitude != Amplitude || LastOmega != Omega || Force) {
			Randomize ();
		}
		LastAmplitude = Amplitude;
		LastOmega = Omega;
	}

	#endregion
}