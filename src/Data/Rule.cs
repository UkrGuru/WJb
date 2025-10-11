// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace UkrGuru.WJb.Data;

public class Rule: Action
{
    public int RuleId { get; set; }

    public string? RuleName { get; set; }

    public Priority RulePriority { get; set; } = Priority.Normal;

    public string? RuleMore { get; set; }
}
