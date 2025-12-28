using System;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Text;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;

namespace DeathRecap.UI;

public class ConfigWindow : Window {
    private readonly DeathRecapPlugin plugin;

    public ConfigWindow(DeathRecapPlugin plugin) : base("死亡复盘 设置###Death Recap Config", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize) {
        this.plugin = plugin;

        Size = new Vector2(580, 340);
    }

    public override void Draw() {
        var conf = plugin.Configuration;

        ImGui.TextUnformatted("记录设置");
        ImGui.Separator();
        ImGui.Columns(3);
        foreach (var (k, v) in conf.EnumCaptureConfigs()) {
            ImGui.PushID(k);
            var bCapture = v.Capture;
            if (ImGui.Checkbox($"记录{CaptureTargetToChinese(k)}", ref bCapture)) {
                v.Capture = bCapture;
                conf.Save();
            }

            var notificationStyle = (int)v.NotificationStyle;
            ImGui.TextUnformatted("死亡时通知");
            if (ImGui.Combo("##2", ref notificationStyle, ["不处理", "聊天消息", "弹窗提示", "打开复盘"])) {
                v.NotificationStyle = (NotificationStyle)notificationStyle;
                conf.Save();
            }

            var bOnlyInstances = v.OnlyInstances;
            if (ImGui.Checkbox("仅在副本中", ref bOnlyInstances)) {
                v.OnlyInstances = bOnlyInstances;
                conf.Save();
            }

            OnlyInInstancesTooltip();

            var bDisableInPvp = v.DisableInPvp;
            if (ImGui.Checkbox("PvP中禁用", ref bDisableInPvp)) {
                v.DisableInPvp = bDisableInPvp;
                conf.Save();
            }

            ImGui.PopID();
            ImGui.NextColumn();
        }

        ImGui.Columns();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextUnformatted("通用设置");
        ImGui.Spacing();
        var chatTypes = Enum.GetValues<XivChatType>();
        var chatType = Array.IndexOf(chatTypes, conf.ChatType);
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("聊天消息频道");
        ImGui.SameLine();
        ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);
        if (ImGui.Combo("##3", ref chatType, chatTypes.Select(t => t.GetAttribute<XivChatTypeInfoAttribute>()?.FancyName ?? t.ToString()).ToImmutableList(),
                10)) {
            conf.ChatType = chatTypes[chatType];
            conf.Save();
        }

        ChatMessageTypeTooltip();

        var bShowTip = conf.ShowTip;
        if (ImGui.Checkbox("显示聊天提示", ref bShowTip)) {
            conf.ShowTip = bShowTip;
            conf.Save();
        }

        ChatTipTooltip();
        var keepEventsFor = conf.KeepCombatEventsForSeconds;
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("战斗事件保留（秒）");
        ImGui.SameLine(ImGuiHelpers.GlobalScale * 140);
        ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 150);
        if (ImGui.InputInt("##4", ref keepEventsFor, 10)) {
            conf.KeepCombatEventsForSeconds = keepEventsFor;
            conf.Save();
        }

        var keepDeathsFor = conf.KeepDeathsForMinutes;
        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted("死亡记录保留（分钟）");
        ImGui.SameLine(ImGuiHelpers.GlobalScale * 140);
        ImGui.SetNextItemWidth(ImGuiHelpers.GlobalScale * 150);
        if (ImGui.InputInt("##5", ref keepDeathsFor, 10)) {
            conf.KeepDeathsForMinutes = keepDeathsFor;
            conf.Save();
        }
    }

    private static string CaptureTargetToChinese(string key) =>
        key switch {
            "Self" => "自己",
            "Party" => "小队",
            "Others" => "其他人",
            _ => key
        };

    private static void ChatMessageTypeTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("用于筛选“聊天消息”死亡通知的聊天频道类型。\n" +
                             "“Debug” 无视聊天窗口筛选，会出现在所有聊天标签页中。\n" +
                             "注意：该设置只影响你本地看到的显示方式，其他人不会看到这些提示。");
        }
    }

    private static void ChatTipTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("首次关闭死亡复盘窗口时，在聊天栏输出用于重新打开窗口的指令。");
        }
    }

    private static void OnlyInInstancesTooltip() {
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip("仅在副本中（例如：迷宫）显示死亡通知。");
        }
    }
}
