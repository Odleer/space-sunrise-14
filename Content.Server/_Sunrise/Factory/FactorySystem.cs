using Content.Server.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Interaction.Events;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared._Sunrise.Factory.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Physics.Events;
using System.Linq;

namespace Content.Server._Sunrise.Factory;

public sealed class FactorySystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedLatheSystem _lathe = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FactoryComponent, StartCollideEvent>(OnCollide);
        SubscribeLocalEvent<FactoryComponent, GetVerbsEvent<Verb>>(OnGetVerb);
    }

    private void OnCollide(EntityUid uid, FactoryComponent component, ref StartCollideEvent args)
    {
        TryProcessItem(uid, args.OtherEntity, component);
    }

    private void TryProcessItem(EntityUid uid, EntityUid item, FactoryComponent component)
    {
        // Проверяем наличие необходимых ресурсов
        if (!CanProduce(uid, component))
        {
            _popup.PopupEntity(Loc.GetString("factory-not-enough-materials"), uid);
            return;
        }

        // Получаем рецепт
        if (!_proto.TryIndex(component.Recipe, out var recipe))
            return;

        if (recipe.Result == null)
            return;

        // Админ-логи
        _adminLogger.Add(LogType.Construction, LogImpact.Medium, 
            $"Объект {ToPrettyString(item)} был переработан фабрикой {ToPrettyString(uid)} в {recipe.Result}");

        // Списываем материалы
        foreach (var (material, needed) in recipe.Materials)
        {
            _materialStorage.TryChangeMaterialAmount(uid, material, -needed);
        }

        // Удаляем старый объект
        Del(item);

        // Создаем новый объект на том же месте
        var coords = Transform(uid).Coordinates;
        Spawn(recipe.Result, coords);
    }

    // Проверка наличия необходимых материалов
    private bool CanProduce(EntityUid factory, FactoryComponent component)
    {
        if (!_proto.TryIndex(component.Recipe, out var recipe))
            return false;

        foreach (var (material, needed) in recipe.Materials)
        {
            if (_materialStorage.GetMaterialAmount(factory, material) < needed)
                return false;
        }

        return true;
    }

    // Отрисовка меню выбора типа продукции
    private void OnGetVerb(EntityUid uid, FactoryComponent component, GetVerbsEvent<Verb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!_proto.TryIndex(component.BorgRecipePack, out var recipePack))
            return;

        foreach (var verbInfo in from type in recipePack.Recipes
                    let proto = _proto.Index(type)
                    select new Verb
                    {
                        Category = VerbCategory.SelectType,
                        Text = _lathe.GetRecipeName(proto),
                        Disabled = type == component.Recipe,
                        DoContactInteraction = true,
                        Icon = proto.Icon,
                        Act = () =>
                        {
                            component.Recipe = type;
                            _popup.PopupEntity(Loc.GetString("emitter-component-type-set",
                                    ("type", _lathe.GetRecipeName(proto))),
                                uid);
                            Dirty(uid, component);
                        },
                    })
        {
            args.Verbs.Add(verbInfo);
        }
    }
}