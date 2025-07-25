// SPDX-FileCopyrightText: 2025 qwerltaz
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.SubFloor;

/// <summary>
/// For tile-like entities, such as catwalk and carpets, to reveal subfloor entities when on the same tile and when
/// using a t-ray scanner.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TrayScanRevealComponent : Component;
