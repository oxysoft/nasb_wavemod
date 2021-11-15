using System;
using System.Reflection;
using System.Text;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Nick;
using UnityEngine;

namespace MyNameSpace // Rename "MyNameSpace"
{
	/// <summary>
	/// Wavemod Plugin
	/// 
	/// QoL mod adding various contextual actions or changing existing ones.
	///
	/// WAVEDASH NOTATION
	/// - PWD: refers to a perfect wavedash, holding left/right with the max distance possible.
	/// - AWD: refers to a shorter wavedash in diagonal, with down held.
	///
	/// ------------------------------------------------------------------------------------------
	///
	/// AIR MODS
	/// - Configurable auto-strafe (refer to STRAFE LOGIC)
	/// - Hold fall: hold down while falling to airdash down (delayed for ledge drops)
	/// - Tap fall: tap down to airdash down in midair (buffered)
	/// - C-stick airdash: airdash with cstick (off by default, feel free to try it)
	///
	/// GROUND MODS
	/// - Configurable auto-strafe (refer to STRAFE LOGIC)
	/// - Crouch Slide: left/right while crouching to PWD.
	/// - Run slide: down while running to PWD.
	/// - Buffered attacks: inputing a strong attack at any time during a wavedash will buffer the input and fire it as soon as landlag is over. TODO
	/// - Instant turnarounds FIX: the first turnaround is not instant, only consecutive dash dancing. This feels like crap, so this feature buffers inputs on the next 2 frames to do an instant dash dance. TODO
	/// - Platform tap drop: tap the joystick down to fall from a platform. TODO
	///
	/// LEDGE MODS
	/// - DOWN to let go of ledge
	///
	/// ------------------------------------------------------------------------------------------
	///
	/// SIDEWAVE BUTTON
	/// This mod adds 2 new buttons, I call them SIDEWAVE LEFT and SIDEWAVE RIGHT. (usually placed on triggers)
	/// They re-imagine wavedashing as a free sidestepping mechanic and decouple airdash from the joystick.
	///
	/// Functionality:
	/// - Grounded: PWD left/right. DOWN to AWD, UP to dash diagonally up.
	/// - Airborn: AWD left/right, or PWD if still within PWD frame window. UP to dash diagonally up. L/R to dash left/right.
	/// - Ledge: PWD getup onto stage.
//
	[BepInPlugin(ID, NAME, VERSION)]
	public class WavemodPlugin : BaseUnityPlugin
	{
		private const string ID      = "com.author.project";
		private const string NAME    = "All Star Melee";
		private const string VERSION = "1.0";

		public static WavemodPlugin Instance { get; private set; }

		public ManualLogSource logger => base.Logger;

		public new static ManualLogSource Logger => Instance.logger;

		internal void Awake()
		{
			Instance = this;

			Logger.Log(LogLevel.Message, $"{NAME} {VERSION}");

			var harmony = new Harmony(NAME);
			harmony.PatchAll();
		}

		private void OnGUI()
		{
			// if (GUI.Button(new Rect(10, 10, 150, 100), "I am a button"))
			// {
			//qprint("You clicked the button!");
			// }
		}
	}

	/// <summary>
	/// 
	/// </summary>
	public class WavemodUI
	{
		public static void ShowWavedash(WavedashStyle style) { }
		public static void ShowWavedash()                    { }
	}
}