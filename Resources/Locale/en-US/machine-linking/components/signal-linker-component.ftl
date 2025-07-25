# SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
# SPDX-FileCopyrightText: 2022 Jack Fox <35575261+DubiousDoggo@users.noreply.github.com>
# SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
# SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
# SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
#
# SPDX-License-Identifier: MIT

signal-linker-component-saved = Successfully saved link to {$machine}!
signal-linker-component-linked-port = Successfully linked {$machine1}:{$port1} to {$machine2}:{$port2}!
signal-linker-component-unlinked-port = Successfully unlinked {$machine1}:{$port1} from {$machine2}:{$port2}!
signal-linker-component-connection-refused = {$machine} refused the connection!
signal-linker-component-max-connections-receiver = Maximum connections reached on the receiver!
signal-linker-component-max-connections-transmitter = Maximum connections reached on the transmitter!

signal-linker-component-type-mismatch = The port's type does not match the type of the saved port!

signal-linker-component-out-of-range = Connection is out of range!

# Verbs
signal-linking-verb-text-link-default = Link default ports
signal-linking-verb-success = Connected all default {$machine} links.
signal-linking-verb-fail = Failed to connect all default {$machine} links.
signal-linking-verb-disabled-no-transmitter = First interact with a transmitter, then link default ports.
signal-linking-verb-disabled-no-receiver = First interact with a receiver, then link default ports.