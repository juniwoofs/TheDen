// SPDX-FileCopyrightText: 2024 Mnemotechnican <69920617+Mnemotechnician@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared.InteractionVerbs.Events;

/// <summary>
///     Raised directly on the performer of the interaction verb and on its target to determine if it should be allowed.
///     Note that this is raised if and only if verb's own CanPerform check returns true.
/// </summary>
[ByRefEvent]
public sealed class InteractionVerbAttemptEvent(InteractionVerbPrototype proto, InteractionArgs args) : CancellableEntityEventArgs
{
    public bool Handled { get; set; } = false;

    public InteractionVerbPrototype Proto => proto;
    public InteractionArgs Args => args;
}
