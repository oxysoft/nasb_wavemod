using Nick;

public static class BtnStateExtensions
{
	public static bool Up(this GameInputState.btnState btn)
	{
		return btn == GameInputState.btnState.NOT;
	}

	public static bool Down(this GameInputState.btnState btn)
	{
		return btn == GameInputState.btnState.HOLD;
	}

	public static bool Released(this GameInputState.btnState btn)
	{
		return btn == GameInputState.btnState.UP;
	}

	public static bool Pressed(this GameInputState.btnState btn)
	{
		return btn == GameInputState.btnState.DOWN;
	}

	public static bool SetPressed(this ref GameInputState.btnState btn)
	{
		return btn == GameInputState.btnState.DOWN;
	}
}