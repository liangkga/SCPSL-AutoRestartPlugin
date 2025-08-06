using System;
using System.Linq;
using Exiled.API.Features;
using Exiled.Events.EventArgs.Server;
using MEC;

namespace AutoRestartPlugin
{
    public class EventHandlers
    {
        private readonly Plugin plugin;
        private System.Threading.CancellationTokenSource playerCountCheckTask;
        
        public EventHandlers(Plugin plugin)
        {
            this.plugin = plugin;
        }
        
        public void OnRoundStarted()
        {
            if (plugin.Config.Debug)
                Log.Info("回合开始，启动玩家数量检查任务");
                
            StartPlayerCountCheck();
        }
        
        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            if (plugin.Config.Debug)
                Log.Info("回合结束，取消玩家数量检查任务");
                
            // 取消玩家数量检查任务
            playerCountCheckTask?.Cancel();
            playerCountCheckTask = null;
        }
        
        private void StartPlayerCountCheck()
        {
            // 取消之前的任务
            playerCountCheckTask?.Cancel();
            
            // 创建新的取消令牌
            playerCountCheckTask = new System.Threading.CancellationTokenSource();
            var token = playerCountCheckTask.Token;
            
            // 启动玩家数量检查任务
            System.Threading.Tasks.Task.Run(async () =>
            {
                try
                {
                    // 等待回合开始后指定时间再开始检查，避免回合刚开始时的误判
                    await System.Threading.Tasks.Task.Delay(plugin.Config.CheckDelaySeconds * 1000, token);
                    
                    if (plugin.Config.Debug)
                        Log.Info($"开始玩家数量检查，检查间隔：{plugin.Config.CheckIntervalSeconds}秒");
                    
                    while (!token.IsCancellationRequested && Round.IsStarted)
                    {
                        try
                        {
                            // 获取所有在线玩家（排除观察者和未分配角色的玩家）
                            var onlinePlayers = Player.List.Where(p => 
                                p != null && 
                                p.Role.Type != PlayerRoles.RoleTypeId.Spectator && 
                                p.Role.Type != PlayerRoles.RoleTypeId.None &&
                                p.Role.Type != PlayerRoles.RoleTypeId.Overwatch
                            ).ToList();
                            
                            if (plugin.Config.Debug)
                                Log.Info($"当前在线玩家数量：{onlinePlayers.Count}");
                            
                            // 如果只剩下1个玩家，重启回合
                            if (onlinePlayers.Count == 1)
                            {
                                if (plugin.Config.Debug)
                                    Log.Info("只剩下一个玩家，准备重启");
                                    
                                // 向所有玩家广播消息
                                var message = string.Format(plugin.Config.SinglePlayerMessage, plugin.Config.SinglePlayerRestartDelay);
                                foreach (var player in Player.List)
                                {
                                    if (player != null)
                                    {
                                        player.Broadcast(plugin.Config.BroadcastDuration, message);
                                    }
                                }
                                
                                // 等待指定时间后重启
                                await System.Threading.Tasks.Task.Delay(plugin.Config.SinglePlayerRestartDelay * 1000, token);
                                
                                if (!token.IsCancellationRequested)
                                {
                                    PerformRestart("只剩下一个玩家");
                                }
                                break;
                            }
                            // 如果没有玩家，直接重启（不广播）
                            else if (onlinePlayers.Count == 0)
                            {
                                if (plugin.Config.Debug)
                                    Log.Info("没有玩家，准备重启");
                                    
                                await System.Threading.Tasks.Task.Delay(plugin.Config.NoPlayerRestartDelay * 1000, token);
                                
                                if (!token.IsCancellationRequested)
                                {
                                    PerformRestart("没有玩家");
                                }
                                break;
                            }
                            
                            // 等待指定间隔后再次检查
                            await System.Threading.Tasks.Task.Delay(plugin.Config.CheckIntervalSeconds * 1000, token);
                        }
                        catch (System.Threading.Tasks.TaskCanceledException)
                        {
                            // 任务被取消，正常退出
                            break;
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error($"玩家数量检查异常: {ex}");
                            // 出现异常时等待一段时间再继续
                            await System.Threading.Tasks.Task.Delay(10000, token);
                        }
                    }
                }
                catch (System.Threading.Tasks.TaskCanceledException)
                {
                    // 任务被取消，正常退出
                    if (plugin.Config.Debug)
                        Log.Info("玩家数量检查任务被取消");
                }
                catch (System.Exception ex)
                {
                    Log.Error($"玩家数量检查任务异常: {ex}");
                }
            }, token);
        }
        
        private void PerformRestart(string reason)
        {
            try
            {
                Log.Info($"执行重启，原因：{reason}");
                
                if (plugin.Config.PreferServerRestart)
                {
                    // 使用延迟调用避免网络状态不稳定时的异常
                    Timing.CallDelayed(0.1f, () => {
                        try
                        {
                            if (plugin.Config.Debug)
                                Log.Info("尝试执行服务器重启");
                                
                            Server.Restart();
                        }
                        catch (System.Exception ex)
                        {
                            Log.Error($"服务器重启异常: {ex}");
                            Log.Warn("服务器重启失败，尝试使用回合重启");
                            
                            // 如果Server.Restart()失败，尝试使用Round.Restart()
                            try
                            {
                                Round.Restart();
                            }
                            catch (System.Exception ex2)
                            {
                                Log.Error($"回合重启也失败: {ex2}");
                            }
                        }
                    });
                }
                else
                {
                    // 直接使用回合重启
                    if (plugin.Config.Debug)
                        Log.Info("执行回合重启");
                        
                    Round.Restart();
                }
            }
            catch (System.Exception ex)
            {
                Log.Error($"重启执行异常: {ex}");
                
                // 最后的备用方案
                try
                {
                    Round.Restart();
                }
                catch (System.Exception ex2)
                {
                    Log.Error($"备用重启方案也失败: {ex2}");
                }
            }
        }
    }
}