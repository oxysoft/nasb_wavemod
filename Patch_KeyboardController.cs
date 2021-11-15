using HarmonyLib;
using JetBrains.Annotations;
using Nick;
using UnityEngine;

namespace MyNameSpace
{
	/// <summary>
	/// Map the numpad keys to the c-stick. (for use with xpadder, antimicro, etc.)
	/// 
	///   8
	/// 4   6
	///   2
	/// </summary>
	[HarmonyPatch(typeof(KeyboardController), "PollIntoState")]
	public class Patch_KeyboardController
	{
		[HarmonyPostfix]
		[UsedImplicitly]
		public static void Postfix(ref GameInputState sis)
		{
			Vector2 cstick = Vector2.zero;

			if (Input.GetKey(KeyCode.Keypad2)) cstick.y--;
			if (Input.GetKey(KeyCode.Keypad8)) cstick.y++;
			if (Input.GetKey(KeyCode.Keypad4)) cstick.x--;
			if (Input.GetKey(KeyCode.Keypad6)) cstick.x++;

			if (Input.GetKey(KeyCode.I)) cstick.y++;
			if (Input.GetKey(KeyCode.K)) cstick.y--;
			if (Input.GetKey(KeyCode.J)) cstick.x--;
			if (Input.GetKey(KeyCode.L)) cstick.x++;

			sis.ControlDir2nd = cstick;
		}
	}
}