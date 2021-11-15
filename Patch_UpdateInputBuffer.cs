using System.CodeDom;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using MyNameSpace;
using Nick;
using Steamworks;
using UnityEngine;


public struct Settings
{
	public Air      air;
	public Ground   ground;
	public Ledge    ledge;
	public Sidewave sidewave;
		
	public struct Air { }

	public struct Ground { }

	public struct Ledge { }

	public struct Sidewave { }
}

/// ------------------------------------------------------------------------------------------
///
/// STRAFE MODES
/// - Free: like in base game, changes direction direction with L/R.
/// - Locked (SSBM physics)
/// - Origin-based (changes around jump point)
/// - Edge Guard: defend based on ledge
/// - Agent Guard: guard against nearest agent
/// - Weighted map: choose between any of the above based on best scoring, with configurable weights for each
/// 
/// ------------------------------------------------------------------------------------------
/// 
/// All wavsedashing macros
///
/// NOTES
/// - Some of it may be considered cheating.
/// My goal is to add more contextual input interactions
/// to make the game more comfortable and less clunky to manoeuver.
/// I think mods like this can raise the bar in the competitive scene.
/// I can provide good arguments I consider each of these modifications to
/// be in good design for this game.
/// 
/// </summary>
[HarmonyPatch(typeof(GameAgentControls), "UpdateInputBuffer")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class Patch_UpdateInputBuffer
{
	public static int wavedashWindowPerf   = 4;
	public static int wavedashWindowAngled = 8;

#region Agent
	
	public static           GameAgentStateMachine states;
	private static          GameAgentMovement     movement;
	private static readonly Inputs                inputs  = new Inputs();
	private static readonly Inputs                linputs = new Inputs();

	public static int h, lh;
	public static int v, lv;

#endregion

#region Frames

	public static  int  groundFrames;
	public static  int  aerialFrames;
	public static  bool airborn, grounded;
	public static  bool jumped;
	public static  int  jumpDir;
	public static  bool wasAirborn;
	private static int  cstate;

#endregion

	public static int _wavedashMacroDelay      = 0;
	public static int wavedashMacroDelayNormal = 0;
	public static int wavedashMacroDelayLedge  = 4;

	private static bool _releasedJumpdir;
	private static bool _justReleasedJumpdir;
	// wavedash
	private static bool          _wavedashMacro;
	private static int           _wavedashMacroDir;
	private static int           _wavedashMacroFrames;
	private static WavedashStyle _wavedashMacroStyle = WavedashStyle.JoystickOrAngled;
	// bufering
	private static bool _bufferedFastfall;
	private static bool _bufferedWavedashMacro;
	private static bool _bufferedWavedashJump;
	private static int  _bufferedWavedashMacroDir;
	private static bool _bufferedJump;

	private static GameAgent _agent;

	private static int state_Land;
	private static int state_LandAirdash;
	private static int state_Jump;
	// private static int state_Ledge;

#region State IDs

	// Use these to test the agent's current state
	// -------------------------------------------

	private static int state_AttackDown;
	private static int state_AttackMid;
	private static int state_AttackMid2;
	private static int state_AttackUp;
	private static int state_AttackRun;
	private static int state_StrongAttackDown;
	private static int state_StrongAttackMid;
	private static int state_StrongAttackUp;
	private static int state_StrongAttackRun;
	private static int state_SpecialDown;
	private static int state_SpecialMid;
	private static int state_SpecialUp;

	private static int state_AirAtkDown;
	private static int state_AirAtkMid;
	private static int state_AirAtkUp;
	private static int state_AirStrDown;
	private static int state_AirStrMid;
	private static int state_AirStrUp;
	private static int state_EdgeGrab;

#endregion

	private static GameInputState.btnState BTN_UP       = GameInputState.btnState.NOT;
	private static GameInputState.btnState BTN_DOWN     = GameInputState.btnState.HOLD;
	private static GameInputState.btnState BTN_PRESSED  = GameInputState.btnState.DOWN;
	private static GameInputState.btnState BTN_RELEASED = GameInputState.btnState.UP;

	private static void Reset()
	{
		groundFrames         = 0;
		aerialFrames         = 0;
		jumped               = false;
		_wavedashMacro       = false;
		_wavedashMacroDir    = 0;
		_wavedashMacroFrames = 0;
	}


	/// <summary>
	/// Skip the original function, we are rewriting it completely 
	/// because traanspiler is too difficult. This should be exeactly
	/// the code from the dll, plus a call to my own function.
	/// </summary>
	/// <returns></returns>Kz
	[UsedImplicitly]
	[HarmonyPrefix]
	private static bool Prefix() => false;

	/// <summary>
	/// Main function that we are rewriting.
	/// </summary>
	[UsedImplicitly]
	[HarmonyPostfix]
	private static void OriginalFunction(
		[NotNull]     GameAgentControls       __instance,
		ref           GameAgent               ___agent,
		[NotNull] ref GameInstance            ___context,
		ref           bool                    ___gotBuffer,
		ref           BufferHelp.SlicedBuffer ___lastBuffer
	)
	{
		GameInput input = __instance.input;

		// AllStarMeleeMod.Logger.Log(LogLevel.Info, "POSTFIX");
		if (___context.allowControl)
		{
			input.PollFrame();

			InjectedCode(___agent, input);
		}
		else
		{
			if (input.controller.type == GameController.ctrlType.Replay)
				((ReplayController)input.controller).nextFrame++;
			if (___gotBuffer && ___context.gameEnded)
				input.TakeState(___lastBuffer[0].state);
			else
				input.TakeState(GameInputState.empty);
		}

		___lastBuffer = input.GetBuffer();
		___gotBuffer  = true;
		if (!__instance.recordReplay)
			return;

		___context.replay.RecordInput(___agent.playerIndex, ___context.NextFrameIndex, ___lastBuffer[0].state.GetP2P());
	}

	/// <summary>
	///	Processes inputs
	/// </summary>
	private static void InjectedCode(GameAgent ___agent, GameInput input)
	{
		// Fetch components
		// ----------------------------------------

		bool hasMovement = ___agent.TryGetMovement(out movement);   // could be cached
		bool hasStates   = ___agent.TryGetStateMachine(out states); // could be cached
		bool hasGif      = input.TryGetFrame(0, out var gif);
		bool hasLgif     = input.TryGetFrame(1, out var lgif);
		bool hasValidController = (input.controller.type == GameController.ctrlType.Keyboard
		                           || input.controller.type == GameController.ctrlType.PlayStation4
		                           || input.controller.type == GameController.ctrlType.GameCube
		                           || input.controller.type == GameController.ctrlType.SwitchPro
		                           || input.controller.type == GameController.ctrlType.GenericJoyPad
			);

		WavemodPlugin.Logger.Log(LogLevel.Info, $"{hasMovement} | {hasStates} | {hasGif} | {hasLgif} | {hasValidController}");
		if (hasMovement && hasStates && hasGif && hasLgif && hasValidController)
		{
			inputs.Copy(gif.state);
			linputs.Copy(lgif.state);

			UpdateInputs(___agent, gif, lgif);

			inputs.Set(gif.state);
		}
	}

	private static void UpdateInputs(GameAgent ___agent, GameInputFrame gif, GameInputFrame lgif)
	{
		WavemodPlugin.Logger.Log(LogLevel.Info, "Applying mods");

		// On changed
		if (___agent != _agent)
		{
			_agent = ___agent;

			Reset();
			FetchStates();
		}

		// Properties
		v        = inputs.v;
		lv       = linputs.v;
		h        = inputs.h;
		lh       = linputs.v;
		cstate   = states.CurrentStateId;
		grounded = movement.MovementState.grounded;
		airborn  = !movement.MovementState.grounded;


		if (cstate == state_EdgeGrab)
		{
			grounded = false;
			airborn  = false;
		}

		// Update state
		// ----------------------------------------
		if (airborn)
		{
			groundFrames = 0;
			aerialFrames++;
		}
		else
		{
			groundFrames++;
			aerialFrames = 0;
		}

		if (cstate == state_Land)
		{
			jumped = false;
		}
		else if (cstate == state_Jump)
		{
			jumped = true;
		}

		_justReleasedJumpdir = false;

		if (!_releasedJumpdir && jumpDir != 0 && h != jumpDir)
		{
			_releasedJumpdir     = true;
			_justReleasedJumpdir = true;
		}

		if (wasAirborn != airborn)
		{
			if (grounded)
			{
				OnGrounded();
			}
			else
			{
				OnLifted();
			}
		}


		// Wavedash macros
		// ----------------------------------------

		// DoJumpWave();

		if (grounded)
		{
			DoSlideWave();
			DoWavedashMacroConfig();
			DoWavedashMacroGround();
		}
		else if (airborn)
		{
			DoWavedashMacroAerial();
		}


		// Aerial Mods
		// ----------------------------------------
		if (airborn)
		{
			DoAerialStrafe();
			DoTapFall();
			DoFastFall(movement);

			TickWavedashMacro();
		}

		// Ledge Mods
		// ----------------------------------------
		DoLedgedash();

		// Jump buffering
		// ----------------------------------------
		UpdateInputBuffer(ref inputs.jump);

		// Airdash with C-stick (experimental)
		// if (inputs.ControlDir2nd.magnitude > 0.25f)
		// {
		// 	inputs.defend     = GameInputState.btnState.DOWN;
		// 	inputs.ControlDir = inputs.ControlDir2nd;
		// }
	}

	private static void FetchStates()
	{
		state_Land        = states.GetStateId("Land");
		state_LandAirdash = states.GetStateId("landairdash");
		state_Jump        = states.GetStateId("Jump");

		state_AttackDown = states.GetStateId("AttackDown");
		state_AttackMid  = states.GetStateId("AttackMid");
		state_AttackRun  = states.GetStateId("AttackRun");
		state_AttackMid2 = states.GetStateId("AttackMid2");
		state_AttackUp   = states.GetStateId("AttackUp");

		state_StrongAttackDown = states.GetStateId("StrongDown");
		state_StrongAttackMid  = states.GetStateId("StrongMid");
		state_StrongAttackRun  = states.GetStateId("StrongRun");
		state_StrongAttackUp   = states.GetStateId("StrongUp");

		state_AirAtkDown = states.GetStateId("AirAtkDown");
		state_AirAtkMid  = states.GetStateId("AirAtkMid");
		state_AirAtkUp   = states.GetStateId("AirAtkUp");
		state_AirStrDown = states.GetStateId("AirStrDown");
		state_AirStrMid  = states.GetStateId("AirStrMid");
		state_AirStrUp   = states.GetStateId("AirStrUp");


		state_SpecialDown = states.GetStateId("SpecialDown");
		state_SpecialMid  = states.GetStateId("SpecialMid");
		state_SpecialUp   = states.GetStateId("SpecialUp");
		state_EdgeGrab    = states.GetStateId("edge-grab");
	}


	private static void UpdateInputBuffer(ref GameInputState.btnState stateJump)
	{
		if (cstate == state_LandAirdash)
		{
			if (stateJump.Pressed())
				_bufferedJump = true;
		}
		else if (_bufferedJump)
		{
			_bufferedJump = false;
			inputs.jump   = BTN_DOWN;
		}
	}

	private static void DoLedgedash()
	{
		if (states.CurrentStateId == state_EdgeGrab)
		{
			if (Input.GetKeyDown(KeyCode.LeftShift))
			{
				StartWavedash(-1, wavedashMacroDelayLedge, WavedashStyle.Perfect);
			}
			else if (Input.GetKeyDown(KeyCode.RightShift))
			{
				StartWavedash(1, wavedashMacroDelayLedge, WavedashStyle.Perfect);
			}
		}
	}


	/// <summary>
	/// Transitioned to ground
	/// </summary>
	private static void OnGrounded() { }

	/// <summary>
	/// Transitioned to air
	/// </summary>
	private static void OnLifted()
	{
		if (jumped)
		{
			jumpDir              = h;
			_releasedJumpdir     = jumpDir == 0;
			_justReleasedJumpdir = false;
		}
	}


#region Mechanic Mods

	private static void DoAerialStrafe()
	{
		// Auto-strafe in midair (simulated SSBM physics, keep direction)
		if (inputs.attack == GameInputState.btnState.NOT &&
		    inputs.strong == GameInputState.btnState.NOT &&
		    inputs.special == GameInputState.btnState.NOT)
			inputs.fun = GameInputState.btnState.HOLD;
	}

#endregion

#region Aerial Dashing

	private static void DoFastFall(GameAgentMovement movement)
	{
		if (movement.MovementState.moveDir.y < 0 && v == -1)
		{
			inputs.defend = GameInputState.btnState.DOWN;
		}
	}

	private static void DoTapFall()
	{
		if (lh == 0 && lv >= 0 && v == -1)
		{
			inputs.defend = GameInputState.btnState.DOWN;

			if (!CanAirdash)
				_bufferedFastfall = true;
		}

		if (_bufferedFastfall && CanAirdash)
		{
			WavedashWindow(h);
			// ApplyFastfall();
			_bufferedFastfall = false;
		}
	}

#endregion

#region Ground Dashing

	private static void DoSlideWave()
	{
		WavedashStyle style = movement.direction == GameAgentMovement.Direction.Left && h == -1
			? WavedashStyle.Perfect
			: WavedashStyle.Angled;

		if (v == -1 && lh == 0 && h != 0) StartWavedash(h, null, style);            // Crouching slidedash
		if (lh != 0 && h != 0 && lv != v && v == -1) StartWavedash(h, null, style); // Walking slidedash
	}

	/// <summary>
	/// Tap left and right within a short time after jumping to wavedash.
	/// </summary>
	private static void DoJumpWave()
	{
		if (airborn
		    && jumped
		    && _releasedJumpdir // We must release our jump dir before we can tap it, otherwise we'll detect release from the running jump
		    && _justReleasedJumpdir
		    && lh != h && h == 0) // BUG this will still trigger
		{
			// Pressed left/right
			WavedashWindow(lh);
		}
	}


	private static void DoWavedashMacroConfig()
	{
		if (Input.GetKeyDown(KeyCode.LeftBracket))
		{
			wavedashMacroDelayLedge--;
			wavedashMacroDelayLedge = Mathf.Max(wavedashMacroDelayLedge, 0);
		}
		else if (Input.GetKeyDown(KeyCode.RightBracket))
		{
			wavedashMacroDelayLedge++;
		}
	}

	private static void DoWavedashMacroAerial()
	{
		if (Input.GetKeyDown(KeyCode.LeftShift))
		{
			WavedashAuto(-1, WavedashStyle.JoystickOrAngled);
			if (!CanAirdash)
			{
				_bufferedWavedashMacro    = true;
				_bufferedWavedashMacroDir = -1;
			}
		}
		else if (Input.GetKeyDown(KeyCode.RightShift))
		{
			WavedashAuto(1, WavedashStyle.JoystickOrAngled);
			if (!CanAirdash)
			{
				_bufferedWavedashMacro    = true;
				_bufferedWavedashMacroDir = 1;
			}
		}

		if (_bufferedWavedashMacro && CanAirdash)
		{
			WavedashAuto(_bufferedWavedashMacroDir, WavedashStyle.JoystickOrAngled);
			_bufferedWavedashMacro    = false;
			_bufferedWavedashMacroDir = 0;
		}
	}

	private static void DoWavedashMacroGround()
	{
		if (Input.GetKeyDown(KeyCode.LeftShift))
			StartWavedash(-1, wavedashMacroDelayNormal);
		else if (Input.GetKeyDown(KeyCode.RightShift))
			StartWavedash(1, wavedashMacroDelayNormal);
	}

	private static void TickWavedashMacro()
	{
		if (_wavedashMacro)
		{
			if (_wavedashMacroFrames >= _wavedashMacroDelay)
			{
				Wavedash(_wavedashMacroDir, _wavedashMacroStyle);

				_wavedashMacro       = false;
				_wavedashMacroFrames = 0;
				_wavedashMacroDir    = 0;
			}
			else
			{
				_wavedashMacroFrames++;
			}
		}
	}

#endregion


	// private enum Buttons
	// {
	// 	defend,
	// 	attack,
	// 	strong,
	// 	special,
	// 	fun
	// }
	//
	// private static bool IsRecent(int frames)
	// {
	// false;
	// }

	private static bool CanAirdash =>
		!(
			states.CurrentStateId == state_AttackDown
			|| states.CurrentStateId == state_AttackMid
			|| states.CurrentStateId == state_AttackMid2
			|| states.CurrentStateId == state_AttackUp
			|| states.CurrentStateId == state_AttackRun
			|| states.CurrentStateId == state_StrongAttackDown
			|| states.CurrentStateId == state_StrongAttackMid
			|| states.CurrentStateId == state_StrongAttackRun
			|| states.CurrentStateId == state_StrongAttackUp
			|| states.CurrentStateId == state_SpecialDown
			|| states.CurrentStateId == state_SpecialMid
			|| states.CurrentStateId == state_SpecialUp
			|| states.CurrentStateId == state_AirAtkDown
			|| states.CurrentStateId == state_AirAtkMid
			|| states.CurrentStateId == state_AirAtkUp
			|| states.CurrentStateId == state_AirStrDown
			|| states.CurrentStateId == state_AirStrMid
			|| states.CurrentStateId == state_AirStrUp
		);

	private static void StartWavedash(int dir, int? delay = null, WavedashStyle wavedashStyle = WavedashStyle.JoystickOrAngled)
	{
		WavemodPlugin.Logger.Log(LogLevel.Info, $"StartWavedash {dir} {delay}");
		_wavedashMacroDelay = delay ?? wavedashMacroDelayNormal;

		inputs.jump          = GameInputState.btnState.DOWN;
		inputs.defend        = GameInputState.btnState.NOT;
		_wavedashMacro       = true;
		_wavedashMacroDir    = dir;
		_wavedashMacroStyle  = wavedashStyle;
		_wavedashMacroFrames = 0;
	}

	private static void WavedashAuto(int dir, WavedashStyle style = WavedashStyle.Angled)
	{
		// AllStarMeleeMod.Logger.Log(LogLevel.Info, $"WavedashAuto {dir} {style}");
		Wavedash(dir, GetSmartDash(style, true));
	}

	private static void WavedashWindow(int dir, WavedashStyle fallback = WavedashStyle.Angled)
	{
		// AllStarMeleeMod.Logger.Log(LogLevel.Info, $"WavedashAuto {dir} {style}");
		Wavedash(dir, GetSmartDash(fallback, false));
	}

	private static WavedashStyle GetSmartDash(WavedashStyle fallback, bool unbounded)
	{
		WavedashStyle style = WavedashStyle.None;

		if (jumped && aerialFrames < wavedashWindowPerf)
			style = WavedashStyle.Perfect;
		else if (unbounded || aerialFrames < wavedashWindowAngled)
			style = fallback;

		return style;
	}

	private static void Wavedash(int dir, WavedashStyle style)
	{
		WavemodPlugin.Logger.Log(LogLevel.Info, $"Wavedash {dir} {style}");

		if (style == WavedashStyle.None) return;

		inputs.defend = GameInputState.btnState.DOWN;
		inputs.h      = dir;
		switch (style)
		{
			case WavedashStyle.Perfect:
				inputs.v = 0;
				break;
			case WavedashStyle.Angled:
				inputs.v = -1;
				break;
			case WavedashStyle.JoystickOrAngled:
				if (inputs.v == 0)
				{
					// Try to perfect dash
					style = GetSmartDash(WavedashStyle.None, false);
					if (style != WavedashStyle.None)
					{
						Wavedash(dir, style);
						return;
					}

					// Otherwise default to down
					inputs.v = -1;
				}

				break;
		}
	}

	private class Inputs
	{
		public int                     h, v;
		public GameInputState.btnState attack;
		public GameInputState.btnState strong;
		public GameInputState.btnState special;
		public GameInputState.btnState jump;
		public GameInputState.btnState defend;
		public GameInputState.btnState fun;
		public GameInputState.btnState pause;
		public GameInputState.btnState taunt;
		public GameInputState.btnState grabmacro;

		public void Copy([NotNull] GameInputState frame)
		{
			h         = frame.Horizontal;
			v         = frame.Vertical;
			attack    = frame.attack;
			strong    = frame.strong;
			special   = frame.special;
			jump      = frame.jump;
			defend    = frame.defend;
			fun       = frame.fun;
			pause     = frame.pause;
			taunt     = frame.taunt;
			grabmacro = frame.grabmacro;
		}

		public void Set([NotNull] GameInputState state)
		{
			state.Horizontal = h;
			Debug.Log($"{state.Horizontal} | {h}");
			state.Vertical = v;
			state.attack   = attack;
			state.strong   = strong;
			state.special  = special;
			state.jump     = jump;
			state.defend   = defend;
			state.fun      = fun;
			state.pause    = pause;
			state.taunt    = taunt;
		}
	}
}

public enum WavedashStyle
{
	None,
	Perfect,
	Angled,
	JoystickOrAngled,
}