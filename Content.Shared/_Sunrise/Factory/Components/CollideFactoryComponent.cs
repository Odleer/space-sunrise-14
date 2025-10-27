using Robust.Shared.GameStates;

namespace Content.Shared._Sunrise.Factory.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CollideFactoryComponent : Component
{
    /// <summary>
    /// Whether this component is currently enabled and can detect collisions
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;
}