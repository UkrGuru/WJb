// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace UkrGuru.WJb.Data;

public class Action
{
    public int ActionId { get; set; }

    public string? ActionName { get; set; }

    public bool Disabled { get; set; }

    public string? ActionType { get; set; }

    public string? ActionMore { get; set; }
}