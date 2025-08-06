using System;
using Exiled.API.Features;
using Exiled.API.Interfaces;
using PlayerEvents = Exiled.Events.Handlers.Player;
using ServerEvents = Exiled.Events.Handlers.Server;

namespace AutoRestartPlugin
{
    public class Plugin : Plugin<Config>
    {
        public override string Name => "Auto Restart Plugin";
        public override string Author => "星浴";
        public override Version Version => new Version(1, 0, 0);
        public override string Prefix => "AutoRestart";
        
        private EventHandlers eventHandlers;
        
        public override void OnEnabled()
        {
            eventHandlers = new EventHandlers(this);
            
            ServerEvents.RoundStarted += eventHandlers.OnRoundStarted;
            ServerEvents.RoundEnded += eventHandlers.OnRoundEnded;
            
            Log.Info($"{Name} v{Version} 已启用");
            base.OnEnabled();
        }
        
        public override void OnDisabled()
        {
            ServerEvents.RoundStarted -= eventHandlers.OnRoundStarted;
            ServerEvents.RoundEnded -= eventHandlers.OnRoundEnded;
            
            eventHandlers = null;
            
            Log.Info($"{Name} v{Version} 已禁用");
            base.OnDisabled();
        }
    }
}