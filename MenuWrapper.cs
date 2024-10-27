using System.Linq;
using Godot;
using System.Collections.Generic;

public partial class MenuWrapper : MenuOutput
{
    public const string StartGameScene = "res://Path/to/Scene.tscn";
    
    public static readonly PackedScene InitialGameScene = ResourceLoader.Load<PackedScene>(StartGameScene);
    private static bool InGame = false; // set as soon EnterGame() is called
    private static bool HasDied = false; // prevent popping the death screen
    
    private static Node CurrentScene;
    private static object InstanceLock = new object();
    private static MenuWrapper _Instance = null;

    // Do not change the order - it'll OpenButton's export since behind the scenes its just an integer
    public enum BlueprintKeys {
        DeathScreen,
        MainMenu,
        PauseMenu,
        QuitConfirm,
        SettingsMenu,
        WinScreen, 
    }
    
    // I guess its smart enough to know what each of these should cast to.
    public static readonly Dictionary<BlueprintKeys, MenuNodeBlueprint> Blueprints = new Dictionary<BlueprintKeys, MenuNodeBlueprint>()
    {
        [BlueprintKeys.DeathScreen] = new MenuNodeBlueprint
        (
            foregound: "res://Path/to/Scene.tscn",
            background: "res://Path/to/Scene.tscn"
        ),
        [BlueprintKeys.MainMenu] = new MenuNodeBlueprint
        (
            foregound: "res://Path/to/Scene.tscn",
            background: "res://Path/to/Scene.tscn"
        ),
        [BlueprintKeys.PauseMenu] = new MenuNodeBlueprint
        (
            foregound: "res://Path/to/Scene.tscn",
            background: "res://Path/to/Scene.tscn"
        ),
        [BlueprintKeys.QuitConfirm] = new MenuNodeBlueprint
        (
            foregound: "res://Path/to/Scene.tscn",
            background: "res://Path/to/Scene.tscn"
        ),
        [BlueprintKeys.SettingsMenu] = new MenuNodeBlueprint
        (
            foregound: "res://Path/to/Scene.tscn",
            background: "res://Path/to/Scene.tscn"
        ),
        [BlueprintKeys.WinScreen] = new MenuNodeBlueprint
        (
            foregound: "res://Path/to/Scene.tscn",
            background: "res://Path/to/Scene.tscn"
        ),
    }; 

    [Signal]
    public delegate void OnPauseEventHandler();
    
    [Signal]
    public delegate void OnResumeEventHandler();
    
    [Signal]
    public delegate void OnReturnToMainMenuEventHandler();

    // The only "state" that MenuWrapper has
    private CanvasLayer Menu; // contains the stack
    private MenuOutput Output; 
    
    public static MenuWrapper Instance()
    {
        if (_Instance == null)
        {
            throw new System.Exception("MenuWrappper._Instance is null");
        }
        lock (InstanceLock)
        {
            return _Instance;
        }
    }

    private MenuWrapper() { }
    
	public override void _Ready()
	{
        lock (InstanceLock)
        {
            if (_Instance != null)
            {
                throw new System.Exception("MenuWrapper._Instance is not null");
            }
            
            base._Ready();
            
            Output = new MenuStack();

            Output.Name = "Output";
            Output.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            Output.ProcessMode = ProcessModeEnum.Always;

            Menu = new CanvasLayer();
            Menu.Name = "MenuCanvasLayer";
            Menu.ProcessMode = ProcessModeEnum.Always;
            
            Menu.AddChild(Output);
            
            CurrentScene = null;

            ProcessMode = ProcessModeEnum.Always;
            
            AddChild(Menu);     

            _Instance = this; 
        }
		  
    }

	public override void _Process(double _delta)
	{
		if (Input.IsActionJustPressed("open_menu"))
		{
            if (GetTree().Paused || !InGame) 
            {
                Pop();
            }
			else 
			{
				PauseGame();
			}
		}
    }

    public void ReturnToMainMenu()
    {
        if (CurrentScene != null)
        {
            CurrentScene.QueueFree();
            CurrentScene = null;
        }
        
        Clear();
        Push(Blueprints[BlueprintKeys.MainMenu]);

        GetTree().Paused = false;
        InGame = false;
        HasDied = false;

        EmitSignal(SignalName.OnReturnToMainMenu);
    }

	public void EnterGame()
	{
		if (CurrentScene != null)
		{
			CurrentScene.QueueFree();
			GetTree().Root.RemoveChild(CurrentScene);
			CurrentScene = null;
		}

		GetTree().Paused = false;
		InGame = true;
		HasDied = false;

        Output.Clear();

        Node NewScene = InitialGameScene.Instantiate();
        
        GetTree().Root.AddChild(NewScene);
        CurrentScene = NewScene;
    }

	public void QuitGame() 
	{
		if (InGame)
		{
			ReturnToMainMenu();
		}
		else 
		{
            GetTree().Quit(); 
		}
    }

    public void PauseGame() 
    {
        if (!GetTree().Paused) 
        {
            GetTree().Paused = true;
            Push(Blueprints[BlueprintKeys.PauseMenu]);
            EmitSignal(SignalName.OnPause);
        } 
    }

    public void ResumeGame()
    {
        if (GetTree().Paused)
        {
            GetTree().Paused = false;
            EmitSignal(SignalName.OnResume);
        }   
    }

    public void OnPlayerDeath()
    {
        if (!HasDied)
        {
            GetTree().Paused = true;
            HasDied = true;
            Push(Blueprints[BlueprintKeys.DeathScreen]);
        }
    }

    public void OnGameWin()
    {
        if (!HasDied) {
            GetTree().Paused = true;
            Push(Blueprints[BlueprintKeys.WinScreen]);
        } 
    }

    public override void Push(MenuNodeBlueprint blueprint)
    {
        Output.Push(blueprint);
    }

    public override void Pop()
    {
        Output.Pop();
    }

    public override void Clear()
    {
        Output.Clear();
    }
}
