using Godot;

/* Superclass */
public partial abstract class MenuOutput : Control 
{
    public abstract void Push(MenuNodeBlueprint blueprint);
    public abstract void Pop();
    public abstract void Clear();
}

