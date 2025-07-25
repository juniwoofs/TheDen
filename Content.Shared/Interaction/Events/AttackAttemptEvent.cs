// SPDX-FileCopyrightText: 2021 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2021 Vera Aguilera Puerto <gradientvera@outlook.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Weapons.Melee;

namespace Content.Shared.Interaction.Events
{
    /// <summary>
    ///     Raised Directed at a user to check whether they are allowed to attack a target.
    /// </summary>
    /// <remarks>
    ///     Combat will also check the general interaction blockers, so this event should only be used for combat-specific
    ///     action blocking.
    /// </remarks>
    public sealed class AttackAttemptEvent : CancellableEntityEventArgs
    {
        public EntityUid Uid { get; }
        public EntityUid? Target { get; }

        public Entity<MeleeWeaponComponent>? Weapon { get; }

        /// <summary>
        ///     If this attempt is a disarm as opposed to an actual attack, for things that care about the difference.
        /// </summary>
        public bool Disarm { get; }

        public AttackAttemptEvent(EntityUid uid, EntityUid? target = null, Entity<MeleeWeaponComponent>? weapon = null, bool disarm = false)
        {
            Uid = uid;
            Target = target;
            Weapon = weapon;
            Disarm = disarm;
        }
    }

    /// <summary>
    /// Raised directed at an entity to check if they can attack while inside of a container.
    /// </summary>
    public sealed class CanAttackFromContainerEvent : EntityEventArgs
    {
        public EntityUid Uid;
        public EntityUid? Target;
        public bool CanAttack = false;

        public CanAttackFromContainerEvent(EntityUid uid, EntityUid? target = null)
        {
            Uid = uid;
            Target = target;
        }
    }
}
