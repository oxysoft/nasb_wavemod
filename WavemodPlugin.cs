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
	
//
	[BepInPlugin(ID, NAME, VERSION)]
	public class WavemodPlugin : BaseUnityPlugin
	{
		private const string ID      = "com.oxysoft.wavemod";
		private const string NAME    = "Wavemod";
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