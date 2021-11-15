using System;
using System.Reflection;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using MyNameSpace;
using Nick;

[HarmonyPatch(typeof(GameParse), "ReadSerialMovesetPreloaded")]
public class Patch_PrintStates
{
	private const int RIGHT_SIDE_SPACING = 40;

	private static readonly StringBuilder _sb     = new StringBuilder();
	private static          int           _indent = 0;

	private static void Indent(int i = 1)
	{
		_indent += i;
	}

	[UsedImplicitly]
	[HarmonyPostfix]
	private static void Postfix(GameAgentStateMachine.IdState[] __result)
	{
		for (var i = 0; i < __result.Length; i++)
		{
			GameAgentStateMachine.IdState idstate = __result[i];
			AgentState                    state   = idstate.state;

			_sb.AppendLine(i.ToString());

			Field("id", idstate.id);
			if (idstate.tags.Count > 0)
				Field("tags", string.Join(",", idstate.tags));
			if (!string.IsNullOrEmpty(state.customCall))
				Field("customCall", state.customCall);

			Field("timeline [");

			try
			{
				Indent();
				foreach (AgentState.TimedAction taction in state.timeline)
				{
					Field($"{taction.atFrame} | {taction.action.GetType().Name}");
					Indent();
					FieldRecursive(taction.action);
					Indent(-1);
				}
			}
			finally
			{
				Indent(-1);
			}

			_sb.AppendLine("]");
		}

		WavemodPlugin.Logger.Log(LogLevel.Info, _sb.ToString());
		_sb.Clear();
	}

	private static void FieldRecursive(object obj)
	{
		FieldInfo[] actionFields = obj
			.GetType()
			.GetFields(BindingFlags.Public | BindingFlags.Instance);

		foreach (FieldInfo objField in actionFields)
		{
			Type   fieldType = objField.FieldType;
			object fieldVal  = objField.GetValue(obj);

			if (fieldVal == null)
				Field(objField.Name, "<nil>");
			else if (fieldType == typeof(string))
				Field(objField.Name, fieldVal.ToString());
			else if (fieldType.IsArray)
			{
				try
				{
					var array = (Array)fieldVal;

					Field($"{objField.Name} (n={array.Length})");
					Indent();
					for (var i = 0; i < array.Length; i++)
					{
						object arrval = array.GetValue(i);
						Field($"- {i}. {arrval.GetType().Name}");
						Indent();
						FieldRecursive(arrval);
						Indent(-1);
					}
				}
				finally
				{
					Indent(-1);
				}
			}
			else if (fieldType.IsClass)
			{
				if (fieldType == typeof(FloatSourceContainer))
				{
					var         fsc = fieldVal as FloatSourceContainer;
					FloatSource fs  = fsc?.fs;
					Field(objField.Name, fs == null ? "<nil>" : $"{fs.GetType().Name}:{fs.val.ToString()}" );
					return;
				}

				Field(objField.Name);

				try
				{
					Indent();
					FieldRecursive(fieldVal);
				}
				finally
				{
					Indent(-1);
				}
			}
			else if (fieldType.IsPrimitive || fieldType.IsValueType)
				Field(objField.Name, fieldVal.ToString());
		}
	}

	private static void Field(string name)
	{
		for (var i = 0; i <= _indent; i++)
			_sb.Append("\t");
		_sb.AppendLine(name);
	}

	private static void Field(string name, string str)
	{
		for (var i = 0; i <= _indent; i++)
			_sb.Append("\t");

		_sb.Append(name.PadRight(RIGHT_SIDE_SPACING, ' '));
		_sb.Append(str);
		_sb.AppendLine();
	}
}