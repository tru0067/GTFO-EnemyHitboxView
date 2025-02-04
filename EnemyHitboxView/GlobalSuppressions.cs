﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "UNT0026:GetComponent always allocates / Use TryGetComponent", Justification = "TryGetComponent is broken in GTFO Il2Cpp Environment")]
[assembly: SuppressMessage("Performance", "UNT0022:Inefficient position/rotation assignment", Justification = "Preferred Clarity", Scope = "member", Target = "~M:EnemyHitboxView.EnemyLimbHitboxes.Update")]
