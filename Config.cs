using System.ComponentModel;
using Exiled.API.Interfaces;

namespace AutoRestartPlugin
{
    public class Config : IConfig
    {
        [Description("是否启用插件")]
        public bool IsEnabled { get; set; } = true;
        
        [Description("是否启用调试模式")]
        public bool Debug { get; set; } = false;
        
        [Description("回合开始后多少秒开始检查玩家数量（避免回合刚开始时的误判）")]
        public int CheckDelaySeconds { get; set; } = 30;
        
        [Description("玩家数量检查间隔（秒）")]
        public int CheckIntervalSeconds { get; set; } = 5;
        
        [Description("只剩一个玩家时的重启延迟（秒）")]
        public int SinglePlayerRestartDelay { get; set; } = 5;
        
        [Description("没有玩家时的重启延迟（秒）")]
        public int NoPlayerRestartDelay { get; set; } = 1;
        
        [Description("是否优先使用服务器重启而不是回合重启")]
        public bool PreferServerRestart { get; set; } = true;
        
        [Description("只剩一个玩家时显示的广播消息")]
        public string SinglePlayerMessage { get; set; } = "<color=yellow>只剩下一个玩家，回合将在{0}秒后自动重启...</color>";
        
        [Description("广播消息显示时长（秒）")]
        public ushort BroadcastDuration { get; set; } = 10;
    }
}