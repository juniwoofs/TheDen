// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;
using System.Numerics;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FishingRodComponent : Component
{
    /// <summary>
    /// Higher value will make every interact more productive.
    /// </summary>
    [DataField]
    public float Efficiency = 1f;

    /// <summary>
    /// At what progress fishing starts.
    /// </summary>
    [DataField]
    public float StartingProgress = 0.33f;

    /// <summary>
    /// How many seconds we wait until fish starts to fight with us
    /// </summary>
    [DataField]
    public float StartingStruggleTime = 0.3f;

    /// <summary>
    /// If lure moves bigger than this distance away from the rod,
    /// it will force it to reel instantly.
    /// </summary>
    [DataField]
    public float BreakOnDistance = 8f;

    [DataField]
    public EntProtoId FloatPrototype = "FishingLure";

    [DataField]
    public SpriteSpecifier RopeSprite =
        new SpriteSpecifier.Rsi(new ResPath("_Goobstation/Objects/Specific/Fishing/fishing_lure.rsi"), "rope");

    [DataField, ViewVariables]
    public Vector2 RopeUserOffset = new (0f, 0f);

    [DataField, ViewVariables]
    public Vector2 RopeLureOffset = new (0f, 0f);

    [DataField, AutoNetworkedField]
    public EntityUid? FishingLure;

    [DataField]
    public EntProtoId ThrowLureActionId = "ActionStartFishing";

    [DataField, AutoNetworkedField]
    public EntityUid? ThrowLureActionEntity;

    [DataField]
    public EntProtoId PullLureActionId = "ActionStopFishing";

    [DataField, AutoNetworkedField]
    public EntityUid? PullLureActionEntity;
}
