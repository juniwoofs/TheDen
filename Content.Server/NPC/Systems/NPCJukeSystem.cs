// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2024 sleepyyapril <flyingkarii@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Content.Server.NPC.Components;
using Content.Server.NPC.Events;
using Content.Server.NPC.HTN.PrimitiveTasks.Operators.Combat;
using Content.Server.Weapons.Melee;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.NPC;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Collections;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.NPC.Systems;

public sealed class NPCJukeSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MeleeWeaponSystem _melee = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private EntityQuery<NPCMeleeCombatComponent> _npcMeleeQuery;
    private EntityQuery<NPCRangedCombatComponent> _npcRangedQuery;
    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();
        _npcMeleeQuery = GetEntityQuery<NPCMeleeCombatComponent>();
        _npcRangedQuery = GetEntityQuery<NPCRangedCombatComponent>();
        _physicsQuery = GetEntityQuery<PhysicsComponent>();

        SubscribeLocalEvent<NPCJukeComponent, NPCSteeringEvent>(OnJukeSteering);
    }

    private void OnJukeSteering(EntityUid uid, NPCJukeComponent component, ref NPCSteeringEvent args)
    {
        if (_timing.CurTime < component.NextJuke)
        {
            component.TargetTile = null;
            return;
        }

        component.NextJuke = _timing.CurTime + TimeSpan.FromSeconds(component.JukeCooldown);

        if (component.JukeType == JukeType.AdjacentTile)
        {
            if (args.Transform.GridUid == null)
                return;

            if (_npcRangedQuery.TryGetComponent(uid, out var ranged)
                && ranged.Status is CombatStatus.NotInSight
                || !TryComp<MapGridComponent>(args.Transform.GridUid, out var grid))
            {
                component.TargetTile = null;
                return;
            }

            var currentTile = _map.CoordinatesToTile((EntityUid) args.Transform.GridUid, grid, args.Transform.Coordinates);

            if (component.TargetTile == null)
            {
                var targetTile = currentTile;
                var startIndex = _random.Next(8);
                _physicsQuery.TryGetComponent(uid, out var ownerPhysics);
                var collisionLayer = ownerPhysics?.CollisionLayer ?? 0;
                var collisionMask = ownerPhysics?.CollisionMask ?? 0;

                for (var i = 0; i < 8; i++)
                {
                    var index = (startIndex + i) % 8;
                    var neighbor = ((Direction) index).ToIntVec() + currentTile;
                    var valid = true;

                    // TODO: Probably make this a helper on engine maybe
                    var tileBounds = new Box2(neighbor, neighbor + grid.TileSize);
                    tileBounds = tileBounds.Enlarged(-0.1f);

                    foreach (var ent in _lookup.GetEntitiesIntersecting(args.Transform.GridUid.Value, tileBounds))
                    {
                        if (ent == uid ||
                            !_physicsQuery.TryGetComponent(ent, out var physics) ||
                            !physics.CanCollide ||
                            !physics.Hard ||
                            ((physics.CollisionMask & collisionLayer) == 0x0 &&
                            (physics.CollisionLayer & collisionMask) == 0x0))
                        {
                            continue;
                        }

                        valid = false;
                        break;
                    }

                    if (!valid)
                        continue;

                    targetTile = neighbor;
                    break;
                }

                component.TargetTile ??= targetTile;
            }

            var elapsed = _timing.CurTime - component.NextJuke;

            // Finished juke.
            if (elapsed.TotalSeconds > component.JukeDuration
                || currentTile == component.TargetTile)
            {
                component.TargetTile = null;
                return;
            }

            var targetCoords = _map.GridTileToWorld((EntityUid) args.Transform.GridUid, grid, component.TargetTile.Value);
            var targetDir = targetCoords.Position - args.WorldPosition;
            targetDir = args.OffsetRotation.RotateVec(targetDir);
            const float weight = 1f;
            var norm = targetDir.Normalized();

            for (var i = 0; i < SharedNPCSteeringSystem.InterestDirections; i++)
            {
                var result = -Vector2.Dot(norm, NPCSteeringSystem.Directions[i]) * weight;

                if (result < 0f)
                    continue;

                args.Steering.Interest[i] = MathF.Max(args.Steering.Interest[i], result);
            }

            args.Steering.CanSeek = false;
        }

        if (component.JukeType == JukeType.Away)
        {
            // TODO: Ranged away juking
            if (_npcMeleeQuery.TryGetComponent(uid, out var melee))
            {
                if (!_melee.TryGetWeapon(uid, out var weaponUid, out var weapon))
                    return;

                var cdRemaining = weapon.NextAttack - _timing.CurTime;
                var attackCooldown = TimeSpan.FromSeconds(1f / _melee.GetAttackRate(weaponUid, uid, weapon));

                // Might as well get in range.
                if (cdRemaining < attackCooldown * 0.45f)
                    return;

                // If we get whacky boss mobs might need nearestpos that's more of a PITA
                // so will just use this for now.
                var obstacleDirection = _transform.GetWorldPosition(melee.Target) - args.WorldPosition;

                if (obstacleDirection == Vector2.Zero)
                    obstacleDirection = _random.NextVector2();

                // If they're moving away then pursue anyway.
                // If just hit then always back up a bit.
                if (cdRemaining < attackCooldown * 0.90f &&
                    _physicsQuery.TryGetComponent(melee.Target, out var targetPhysics) &&
                    Vector2.Dot(targetPhysics.LinearVelocity, obstacleDirection) > 0f)
                {
                    return;
                }

                if (cdRemaining < TimeSpan.FromSeconds(1f / _melee.GetAttackRate(weaponUid, uid, weapon)) * 0.45f)
                    return;

                // TODO: Probably add in our bounds and target bounds for ideal distance.
                var idealDistance = weapon.Range * 4f;
                var obstacleDistance = obstacleDirection.Length();

                if (obstacleDistance > idealDistance || obstacleDistance == 0f)
                {
                    // Don't want to get too far.
                    return;
                }

                obstacleDirection = args.OffsetRotation.RotateVec(obstacleDirection);
                var norm = obstacleDirection.Normalized();

                var weight = obstacleDistance <= args.Steering.Radius
                    ? 1f
                    : (idealDistance - obstacleDistance) / idealDistance;

                for (var i = 0; i < SharedNPCSteeringSystem.InterestDirections; i++)
                {
                    var result = -Vector2.Dot(norm, NPCSteeringSystem.Directions[i]) * weight;

                    if (result < 0f)
                        continue;

                    args.Steering.Interest[i] = MathF.Max(args.Steering.Interest[i], result);
                }
            }

            args.Steering.CanSeek = false;
        }
    }
}
