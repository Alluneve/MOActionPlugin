﻿using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace MOAction.Configuration;

[Serializable]
public class ConfigurationEntry
{
    public uint BaseId;
    public List<ConfigurationActionStack> ConfigurationActionStacks;
    public VirtualKey Modifier;
    public uint JobIdx;


    public ConfigurationEntry(uint baseId, List<ConfigurationActionStack> configurationActionStacks, VirtualKey modifier, uint job)
    {
        BaseId = baseId;
        ConfigurationActionStacks = configurationActionStacks;
        Modifier = modifier;
        JobIdx = job;
    }

    [JsonConstructor]
    [Obsolete("This constructor is a one-time migration from the old job string to jobIdx uint and/or from the old stack to the new stack Added 16/06/2025")]
    public ConfigurationEntry(uint baseId, List<ConfigurationActionStack> configurationActionStacks, VirtualKey modifier, uint jobIdx,
        string job = null, List<(string, uint)> stack = null)
    {
        BaseId = baseId;
        Modifier = modifier;
        if (job == null)
        {
            if (jobIdx == 0 && Plugin.PlayerState.IsLoaded)
            {
                JobIdx = Plugin.PlayerState.ClassJob.RowId;
            }
            else
            {
                JobIdx = jobIdx;
            }
        }
        else
        {
            JobIdx = uint.TryParse(job, out var num) ? num : 0;
        }
        if (stack == null)
        {
            ConfigurationActionStacks = configurationActionStacks;
        }
        else
        {
            ConfigurationActionStacks = new();
            foreach (var tuple in stack)
            {
                ConfigurationActionStacks.Add(new ConfigurationActionStack(tuple.Item1, tuple.Item2));
            }
        }
    }

    public override string ToString()
    {
        return
            $"BaseId: {BaseId}, Modifier: {Modifier}, JobIdx: {JobIdx}, Stacks: {ConfigurationActionStacks.Count}";
    }

    [Serializable]
    public class ConfigurationActionStack(string target, uint actionId)
    {
        public string Target { get; set; } = target;
        public uint ActionId { get; set; } = actionId;

        public override string ToString()
        {
            return $"Target: {Target}, ActionId: {ActionId}";
        }
    }
}