using Content.Shared.Lathe;
using Content.Shared.Verbs;
using Content.Shared._Sunrise.Factory.Components;
using Content.Shared.Interaction;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Client._Sunrise.Factory;
using Robust.Client.Player;
using Robust.Client.UserInterface;

namespace Content.Client._Sunrise.Factory;

public sealed class FactorySystem : EntitySystem
{
    [Dependency] private readonly SharedLatheSystem _lathe = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

    private FactoryMaterialStorageWindow? _window;

    public override void Initialize()
    {
        SubscribeLocalEvent<FactoryComponent, GetVerbsEvent<Verb>>(OnGetVerb);
        SubscribeLocalEvent<FactoryComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnGetVerb(EntityUid uid, FactoryComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!_proto.TryIndex(component.BorgRecipePack, out var recipePack))
            return;

        foreach (var v in from type in recipePack.Recipes
                 let proto = _proto.Index(type)
                 select new Verb
                 {
                     Category = VerbCategory.SelectType,
                     Text = _lathe.GetRecipeName(proto),
                     Disabled = type == component.Recipe,
                     DoContactInteraction = true,
                     Icon = proto.Icon,
                 })
        {
            args.Verbs.Add(v);
        }
    }

    private void OnAfterInteract(EntityUid uid, FactoryComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || !IsClientSide(args.User))
            return;

        OpenFactoryStorageWindow(uid);
    }

    private void OpenFactoryStorageWindow(EntityUid factoryUid)
    {
        _window?.Dispose();
        _window = _uiManager.CreateWindow<FactoryMaterialStorageWindow>();
        _window.SetOwner(factoryUid);
        _window.OpenCentered();
    }
}