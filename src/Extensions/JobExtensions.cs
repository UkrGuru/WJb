// Copyright (c) Oleksandr Viktor (UkrGuru). All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using UkrGuru.WJb.Data;

namespace UkrGuru.WJb.Extensions;

public static class JobExtensions
{
    public static dynamic CreateAction(this Job job)
    {
        ArgumentNullException.ThrowIfNull(job.ActionType);
    
        var type = Type.GetType($"UkrGuru.WJb.Actions.{job.ActionType}") ?? Type.GetType(job.ActionType);
        ArgumentNullException.ThrowIfNull(type);

        dynamic? action = Activator.CreateInstance(type);
        ArgumentNullException.ThrowIfNull(action);

        action!.Init(job);

        return action;
    }
}