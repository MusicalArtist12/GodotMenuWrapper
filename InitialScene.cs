using Godot;

public partial class InitialScene : Control
{
	public override void _Ready()
	{
		MenuWrapper.Instance().ReturnToMainMenu();
		QueueFree();
	}
}
