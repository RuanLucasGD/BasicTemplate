using Flax.Build;

public class BasicTemplateTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add("BasicTemplate");
    }
}
