using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MyNameSpace // Rename "MyNameSpace"
{
	[BepInPlugin(ID, NAME, VERSION)]
	public class AllStarMeleeMod : BaseUnityPlugin
	{
		private const string ID      = "com.author.project";
		private const string NAME    = "All Star Melee";
		private const string VERSION = "1.0";

		public static AllStarMeleeMod Instance { get; private set; }

		public ManualLogSource logger => base.Logger;

		public new static ManualLogSource Logger => Instance.logger;

		internal void Awake()
		{
			Instance = this;

			Logger.Log(LogLevel.Message, $"{NAME} {VERSION}");

			var harmony = new Harmony(NAME);
			harmony.PatchAll();
		}
	}
}