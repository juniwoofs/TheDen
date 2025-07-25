// SPDX-FileCopyrightText: 2024 Remuchi <72476615+Remuchi@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared.ListViewSelector;

[Serializable, NetSerializable]
public record ListViewSelectorEntry(string Id, string Name = "", string Description = "");

[Serializable, NetSerializable]
public enum ListViewSelectorUiKey
{
    Key
}

[Serializable, NetSerializable]
public sealed class ListViewSelectorState(
    List<ListViewSelectorEntry> items,
    Dictionary<string, object>? metaData = null) : BoundUserInterfaceState
{
    public List<ListViewSelectorEntry> Items { get; } = items;
    public Dictionary<string, object> MetaData = metaData ?? new();
}

[Serializable, NetSerializable]
public sealed class ListViewItemSelectedMessage(
    ListViewSelectorEntry selectedItem,
    int index,
    Dictionary<string, object> metaData = default!)
    : BoundUserInterfaceMessage
{
    public ListViewSelectorEntry SelectedItem { get; private set; } = selectedItem;
    public int Index { get; private set; } = index;
    public Dictionary<string, object> MetaData = metaData;
}
