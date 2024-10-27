using Godot;

public partial class PauseMenu : MenuNode
{
    public override void OnPop()
    {
        MenuWrapper.Instance().ResumeGame();
		base.OnPop();
    }
}
