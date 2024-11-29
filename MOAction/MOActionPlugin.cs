﻿using Dalamud.Game.Command;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using MOAction.Target;
using MOAction.Configuration;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Keys;
using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using Dalamud.Plugin.Services;
using Dalamud.Game;
using Dalamud.IoC;

namespace MOAction
{
    // This class handles the visual GUI frontend and organizing of action stacks.
    // Listen.
    // I appreciate you wanting to read my code.
    // This class is nothing but ugly GUI code.
    // It works, and that's what I care about.
    // ImGui is a pain to work with, but I managed to write something that gets the job done. That's all I care about.
    // Proceed with this warning in mind.
    internal class MOActionPlugin : IDalamudPlugin
    {
        [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] internal static IClientState ClientState { get; private set; } = null!;
        [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
        [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
        [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
        [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
        [PluginService] internal static IPluginLog PluginLog { get; private set; } = null!;
        [PluginService] internal static IGameInteropProvider HookProvider { get; private set; } = null!;
        [PluginService] internal static IObjectTable Objects { get; private set; } = null!;
        [PluginService] internal static IGameGui GameGui { get; private set; } = null!;
        [PluginService] internal static IKeyState KeyState { get; private set; } = null!;
        public string Name => "Mouseover Action Plugin";
        public MOActionConfiguration Configuration;

        private MOAction moAction;
        private List<Lumina.Excel.Sheets.Action> applicableActions;
        private List<TargetType> TargetTypes;
        private List<TargetType> GroundTargetTypes;
        private List<MoActionStack> NewStacks;
        private Dictionary<string, HashSet<MoActionStack>> SavedStacks;
        private Dictionary<string, List<MoActionStack>> SortedStacks;
        private bool firstTimeUpgrade = false;
        private bool rangeCheck;
        private bool mouseClamp;
        private bool otherGroundClamp;
        private readonly Lumina.Excel.Sheets.ClassJob[] Jobs;
        private readonly List<Lumina.Excel.Sheets.ClassJob> JobAbbreviations;
        private Dictionary<string, List<Lumina.Excel.Sheets.Action>> JobActions;
        private bool isImguiMoSetupOpen = false;
        private readonly int CURRENT_CONFIG_VERSION = 6;



        unsafe public MOActionPlugin()
        {
            CommandManager.AddHandler("/pmoaction", new CommandInfo(OnCommandDebugMouseover)
            {
                HelpMessage = "Open a window to edit mouseover action settings.",
                ShowInHelp = true
            });

            applicableActions = new List<Lumina.Excel.Sheets.Action>();

            Jobs = DataManager.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>().Where(x => x.JobIndex > 0).ToArray();
            JobAbbreviations = Jobs.ToList();
            JobAbbreviations.Sort((x, y) => x.Abbreviation.ExtractText().CompareTo(y.Abbreviation.ExtractText()));
            NewStacks = new();
            SavedStacks = new();
            SortedStacks = new();
            var x = DataManager.GetExcelSheet<Lumina.Excel.Sheets.Action>().Where(row => row.IsPlayerAction && !row.IsPvP && row.ClassJobLevel > 0).ToList();
            foreach (var a in x)
            {
                // random old ability still marked as usable?
                if (a.RowId == 212)
                    continue;
                applicableActions.Add(a);

            }
            JobActions = new();
            SortActions();
            moAction = new MOAction(SigScanner,
                                    ClientState,
                                    DataManager,
                                    TargetManager,
                                    Objects,
                                    KeyState,
                                    GameGui,
                                    HookProvider,
                                    PluginLog,
                                    Jobs.ToDictionary(item => item.RowId));

            foreach (var jobname in JobAbbreviations)
            {
                JobActions.Add(jobname.Abbreviation.ExtractText(), applicableActions.Where(action => action.ClassJobCategory.Value.Name.ExtractText().Contains(jobname.Name.ExtractText()) ||
                action.ClassJobCategory.Value.Name.ExtractText().Contains(jobname.Abbreviation.ExtractText())).ToList());
            }

            TargetTypes = new List<TargetType>
            {
                new EntityTarget(moAction.GetGuiMoPtr, "UI Mouseover"),
                new EntityTarget(moAction.getFieldMo, "Field Mouseover"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<t>"), "Target"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<f>"), "Focus Target"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<tt>"), "Target of Target"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<me>"), "Self"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<2>"), "<2>"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<3>"), "<3>"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<4>"), "<4>"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<5>"), "<5>"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<6>"), "<6>"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<7>"), "<7>"),
                new EntityTarget(() => moAction.GetActorFromPlaceholder("<8>"), "<8>")
            };

            GroundTargetTypes = new List<TargetType>
            {
                new EntityTarget(() => null, "Mouse Location", false),
            };

            var config = PluginInterface.GetPluginConfig() as MOActionConfiguration ?? new MOActionConfiguration();

            // big upgrade for old moaction config
            if (config.Version < 6)
            {
                config = new MOActionConfiguration();
                firstTimeUpgrade = true;
            }
            else
            {
                for (int i = 0; i < config.Stacks.Count; i++)
                {
                    var y = config.Stacks[i];
                    int tmp;
                    if (!int.TryParse(y.Job, out tmp))
                    {
                        var q = DataManager.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>().FirstOrDefault(z => z.Abbreviation == y.Job);
                        if (q.RowId != default)
                            y.Job = DataManager.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>().First(z => z.Abbreviation == y.Job).RowId.ToString();
                        else
                        {
                            config.Stacks.Remove(y);
                            i--;
                        }
                    }
                }
            }
            Configuration = config;
            {
                var tmpstacks = RebuildStacks(Configuration.Stacks);
                SavedStacks = SortStacks(tmpstacks);
                foreach (var (k, v) in SavedStacks)
                {
                    var tmp = v.ToList();
                    tmp.Sort();
                    SortedStacks[k] = tmp;
                }
            }
            rangeCheck = Configuration.RangeCheck;
            mouseClamp = Configuration.MouseClamp;
            otherGroundClamp = Configuration.OtherGroundClamp;

            moAction.SetConfig(Configuration);

            moAction.Enable();
            foreach (var entry in SavedStacks)
            {
                moAction.Stacks.AddRange(entry.Value);
            }
            PluginInterface.UiBuilder.OpenConfigUi += () => isImguiMoSetupOpen = true;
            PluginInterface.UiBuilder.Draw += UiBuilder_OnBuildUi;
            PluginInterface.UiBuilder.OpenMainUi += () => isImguiMoSetupOpen = true;

        }

        private void UiBuilder_OnBuildUi()
        {
            if (firstTimeUpgrade)
                DrawUpgradeNotice();
            else if (isImguiMoSetupOpen)
                DrawConfig();
            else if (NewStacks.Count != 0)
                SortStacks();
        }

        private void DrawUpgradeNotice()
        {
            ImGui.SetNextWindowSize(new Vector2(800, 400), ImGuiCond.Appearing);
            ImGui.Begin("MOAction 4.0 update Information");
            ImGui.Text("MOAction 4.0 is a BREAKING update. This means that the entirety of your config has been wiped.");
            ImGui.Text("This is an unfortunate side effect of me being not the best developer. I apologize but it's the only sane way forward. Here are the patch notes:");
            ImGui.Text("- Removed \"old config\". It's never coming back. You will set up your stacks and you will like it.");
            ImGui.Text("- Added support for every PvE-usable action in the game.");
            ImGui.Text("-- Ground targeted actions WILL queue properly, and DO work for all placeholders. There is a special mouse placement option as well for them.");
            if (ImGui.Button("ok"))
                firstTimeUpgrade = false;
            ImGui.End();
        }

        private void DrawConfigForList(ICollection<MoActionStack> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                ImGui.PushID(i);
                var entry = list.ElementAt(i);
                if (ImGui.CollapsingHeader(entry.BaseAction.RowId == 0 ? "Unset Action###" : entry.BaseAction.Name.ExtractText() + "###"))
                {
                    ImGui.SetNextItemWidth(100);
                    // Require user to select a job, filtering actions down.
                    if (ImGui.BeginCombo("Job", entry.GetJob(DataManager)))
                    {
                        foreach (var x in JobAbbreviations)
                        {
                            string job = x.Abbreviation.ExtractText();
                            if (ImGui.Selectable(job))
                            {
                                if (entry.GetJob(DataManager) != null && entry.GetJob(DataManager) != job)
                                {
                                    entry.BaseAction = default;
                                    foreach (var stackentry in entry.Entries)
                                        stackentry.Action = default;
                                }
                                entry.Job = x.RowId.ToString();
                            }
                        }
                        ImGui.EndCombo();
                    }
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    if (ImGui.BeginCombo("Held Modifier Key", entry.Modifier.ToString()))
                    {
                        foreach (VirtualKey vk in MoActionStack.AllKeys)
                        {
                            if (ImGui.Selectable(vk.ToString()))
                            {
                                entry.Modifier = vk;
                            }
                        }
                        ImGui.EndCombo();
                    }
                    if (entry.GetJob(DataManager) != "Unset Job" || entry.GetJob(DataManager) != "ADV")
                    {
                        ImGui.Indent();
                        // Select base action.
                        ImGui.SetNextItemWidth(200);
                        if (ImGui.BeginCombo("Base Action", entry.BaseAction.RowId == default ? "" : entry.BaseAction.Name.ExtractText()))
                        {
                            foreach (var actionEntry in JobActions[entry.GetJob(DataManager)])
                            {
                                if (ImGui.Selectable(actionEntry.Name.ExtractText()))
                                {
                                    entry.BaseAction = actionEntry;
                                    // By default, add UI mouseover as the first TargetType
                                    if (entry.Entries.Count == 0)
                                    {
                                        entry.Entries.Add(new(actionEntry, TargetTypes[0]));
                                    }
                                    else
                                    {
                                        entry.Entries[0].Action = actionEntry;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }
                        if (entry.BaseAction.RowId != default)
                        {
                            ImGui.Indent();
                            for (int j = 0; j < entry.Entries.Count; j++)
                            {
                                var stackEntry = entry.Entries[j];

                                ImGui.PushID(j); // push stack entry number

                                ImGui.Text($"Ability #{entry.Entries.IndexOf(stackEntry) + 1}");
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.BeginCombo("Target", stackEntry.Target == null ? "" : stackEntry.Target.TargetName))
                                {
                                    foreach (var target in TargetTypes)
                                    {
                                        if (ImGui.Selectable(target.TargetName))
                                        {
                                            stackEntry.Target = target;
                                        }
                                    }
                                    if (stackEntry.Action.TargetArea)
                                    {
                                        foreach (var target in GroundTargetTypes)
                                            if (ImGui.Selectable(target.TargetName))
                                            {
                                                stackEntry.Target = target;
                                            }
                                    }

                                    ImGui.EndCombo();
                                }
                                ImGui.SameLine();
                                ImGui.SetNextItemWidth(200);
                                if (ImGui.BeginCombo("Ability", stackEntry.Action.Name.ExtractText()))
                                {
                                    foreach (var ability in JobActions[entry.GetJob(DataManager)])
                                    {
                                        if (ImGui.Selectable(ability.Name.ExtractText()))
                                        {
                                            stackEntry.Action = ability;
                                            if (ability.TargetArea && GroundTargetTypes.Contains(stackEntry.Target))
                                            {
                                                stackEntry.Target = null;
                                            }
                                        }
                                    }
                                    ImGui.EndCombo();
                                }

                                // TODO: foreach makes lists immutable, use for-loop
                                if (entry.Entries.Count() > 1)
                                {
                                    ImGui.SameLine();
                                    if (ImGui.Button("Delete Entry"))
                                    {
                                        entry.Entries.Remove(stackEntry);
                                        j--;
                                    }
                                }
                                ImGui.PopID(); //pop stack entry number
                            }

                            ImGui.Unindent();
                            // Add new entry to bottom of stack.
                            if (ImGui.Button("Add new stack entry"))
                            {
                                entry.Entries.Add(new(entry.BaseAction, null));
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Copy stack to clipboard"))
                            {
                                CopyToClipboard(new() { entry });
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Delete Stack"))
                            {
                                list.Remove(entry);
                                SavedStacks[entry.GetJob(DataManager)].Remove(entry);
                                i--;
                            }
                        }
                        ImGui.Unindent();
                    }
                }
                ImGui.PopID(); //pop i
            }

        }

        private void CopyToClipboard(List<MoActionStack> list)
        {
            List<ConfigurationEntry> entries = new();
            foreach (var elem in list)
            {
                var x = Configuration.Stacks.FirstOrDefault(e => elem.Equals(e));
                if (x == default) continue;
                entries.Add(x);
            }
            var json = JsonConvert.SerializeObject(entries);
            ImGui.SetClipboardText(Convert.ToBase64String(Encoding.UTF8.GetBytes(json.ToString())));
        }

        private Dictionary<string, HashSet<MoActionStack>> SortStacks(List<MoActionStack> list)
        {
            Dictionary<string, HashSet<MoActionStack>> toReturn = new();
            foreach (var x in JobAbbreviations)
            {
                var name = x.Abbreviation.ExtractText();
                var jobstack = list.Where(x => x.GetJob(DataManager) == name).ToList();
                if (jobstack.Count > 0)
                    toReturn[name] = new(jobstack);
                else
                    toReturn[name] = new();
            }
            return toReturn;
        }

        private void SaveStacks()
        {
            SortStacks();
            moAction.Stacks.Clear();
            foreach (var x in SavedStacks)
            {
                foreach (var entry in x.Value)
                {
                    moAction.Stacks.Add(entry);
                }
            }
            Configuration.Stacks.Clear();
            foreach (var x in moAction.Stacks)
                Configuration.Stacks.Add(new ConfigurationEntry(x.BaseAction.RowId, x.Entries.Select(y => (y.Target.TargetName, y.Action.RowId)).ToList(), x.Modifier, x.Job));
            UpdateConfig();
        }

        private void DrawConfig()
        {
            ImGui.SetNextWindowSize(new Vector2(800, 800) * ImGui.GetIO().FontGlobalScale, ImGuiCond.Once);
            ImGui.Begin("Action stack setup", ref isImguiMoSetupOpen,
                ImGuiWindowFlags.NoCollapse);
            ImGui.Text("This window allows you to set up your action stacks.");
            ImGui.Text("What is an action stack? ");
            ImGui.SameLine();
            if (ImGui.Button("Click me to learn!"))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://youtu.be/pm4eCxD90gs",
                    UseShellExecute = true
                });
            }

            ImGui.Checkbox("Stack entry fails if target is out of range.", ref rangeCheck);
            if (ImGui.Button("Copy all stacks to clipboard"))
            {
                CopyToClipboard(moAction.Stacks);
            }
            ImGui.SameLine();
            if (ImGui.Button("Import stacks from clipboard"))
            {
                // TODO: don't wipe all existing stacks
                try
                {
                    var tempStacks = SortStacks(
                        RebuildStacks(
                            JsonConvert.DeserializeObject<List<ConfigurationEntry>>(
                                 Encoding.UTF8.GetString(
                                    Convert.FromBase64String(
                                        ImGui.GetClipboardText())))));
                    foreach (var (k, v) in tempStacks)
                    {
                        if (SavedStacks.ContainsKey(k))
                            SavedStacks[k].UnionWith(v);
                        else
                            SavedStacks[k] = v;
                    }
                }
                catch (Exception)
                {
                    ImGui.BeginPopup("oh no, cringe!");
                    ImGui.EndPopup();
                }
            }
            ImGui.BeginChild("scrolling", new Vector2(0, -(25 + ImGui.GetStyle().ItemSpacing.Y) * ImGui.GetIO().FontGlobalScale), true);

            // sorted stacks are grouped by job.
            ImGui.PushID("Sorted Stacks");
            foreach (var x in JobAbbreviations)
            {
                var jobName = x.Abbreviation.ExtractText();
                var entries = SavedStacks[jobName];
                if (entries.Count == 0) continue;
                var key = jobName;

                ImGui.PushID(key); //push job

                ImGui.SetNextItemWidth(300);
                if (ImGui.CollapsingHeader(key))
                {
                    if (ImGui.Button("Copy All to Clipboard"))
                    {
                        CopyToClipboard(entries.ToList());
                    }
                    ImGui.Indent();
                    DrawConfigForList(SortedStacks[jobName]);

                    ImGui.Unindent();
                }
                ImGui.PopID(); //pop job
            }
            ImGui.PopID();
            ImGui.PushID("Unsorted Stacks");
            // Unsorted stacks are created when "Add stack" is clicked.
            DrawConfigForList(NewStacks);
            ImGui.PopID();
            ImGui.EndChild();
            if (ImGui.Button("Save"))
            {
                SaveStacks();
            }
            ImGui.SameLine();
            if (ImGui.Button("Save and Close"))
            {
                isImguiMoSetupOpen = false;
                SaveStacks();
            }
            ImGui.SameLine();
            if (ImGui.Button("New Stack"))
            {
                if (ClientState.LocalPlayer != null)
                {
                    MoActionStack stack = new(default, null);
                    var job = ClientState.LocalPlayer.ClassJob.RowId.ToString();
                    stack.Job = job;
                    NewStacks.Add(stack);
                    PluginLog.Debug("localplayer job was {1}",job);
                }
                else
                {
                    NewStacks.Add(new(default, new()));
                }
            }
            ImGui.End();
        }

        private void SortStacks()
        {
            foreach (var x in NewStacks)
            {
                if (x.Job != "Unset Job" && x.Entries.Count > 0)
                    SavedStacks[x.GetJob(DataManager)].Add(x);
            }
            NewStacks.Clear();
            foreach (var (k, v) in SavedStacks)
            {
                var tmp = v.ToList();
                tmp.Sort();
                SortedStacks[k] = tmp;
            }

        }

        private void UpdateConfig()
        {
            Configuration.Version = CURRENT_CONFIG_VERSION;
            Configuration.RangeCheck = rangeCheck;
            Configuration.MouseClamp = mouseClamp;
            Configuration.OtherGroundClamp = otherGroundClamp;

            PluginInterface.SavePluginConfig(Configuration);
        }


        public List<MoActionStack> RebuildStacks(List<ConfigurationEntry> configurationEntries)
        {
            if (configurationEntries == null) return new();
            var toReturn = new List<MoActionStack>();
            foreach (var entry in configurationEntries)
            {
                var action = applicableActions.FirstOrDefault(x => x.RowId == entry.BaseId);
                if (action.RowId == default) continue;
                string job = entry.Job;
                List<StackEntry> entries = new();
                foreach (var stackEntry in entry.Stack)
                {
                    TargetType targ = TargetTypes.FirstOrDefault(x => x.TargetName == stackEntry.Item1);
                    if (targ == default) targ = GroundTargetTypes[0];
                    var action1 = applicableActions.FirstOrDefault(x => x.RowId == stackEntry.Item2);
                    if (action1.RowId == default)
                        continue;
                    entries.Add(new(action1, targ));
                }
                MoActionStack tmp = new(action, entries);
                tmp.Job = job;
                tmp.Modifier = entry.Modifier;
                toReturn.Add(tmp);
            }

            return toReturn;
        }
        public void Dispose()
        {
            moAction.Dispose();
            CommandManager.RemoveHandler("/pmoaction");
        }

        private void OnCommandDebugMouseover(string command, string arguments)
        {
            isImguiMoSetupOpen = true;
        }

        private void SortActions()
        {
            List<Lumina.Excel.Sheets.Action> tmp = new();

            foreach (var x in JobAbbreviations)
            {
                var elem = x.Name.ExtractText();
                foreach (var action in applicableActions)
                {
                    var nameStr = action.ClassJobCategory.Value.Name.ExtractText();
                    if ((nameStr.Contains(elem) || nameStr.Contains(x.Abbreviation.ExtractText())) && !action.IsRoleAction)
                        tmp.Add(action);
                }
            }

            foreach (var x in JobAbbreviations)
            {
                var elem = x.Name;
                foreach (var action in applicableActions)
                {
                    var nameStr = action.ClassJobCategory.Value.Name.ExtractText();
                    if ((nameStr.Contains(elem.ExtractText()) || nameStr.Contains(x.Abbreviation.ExtractText())) && action.IsRoleAction && !tmp.Contains(action))
                        tmp.Add(action);
                }
            }
            applicableActions.Clear();
            foreach (var elem in tmp)
            {
                applicableActions.Add(elem);
            }
            applicableActions.Sort((x, y) => x.Name.ExtractText().CompareTo(y.Name.ExtractText()));
        }
    }
}
