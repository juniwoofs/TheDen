// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server.WhiteDream.BloodCult.RendingRunePlacement;

[RegisterComponent]
public sealed partial class RendingRunePlacementMarkerComponent : Component
{
    [DataField]
    public bool IsActive;

    [DataField]
    public float DrawingRange = 10;
}
