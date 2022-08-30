// -----------------------------------------------------------------------
// <copyright file="AntiAFKHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
using Exiled.Permissions.Extensions;
using MEC;
using Mistaken.API;
using Mistaken.API.Commands;
using Mistaken.API.Diagnostics;
using Mistaken.API.Extensions;
using Mistaken.API.GUI;
using Mistaken.RoundLogger;
using UnityEngine;

namespace Mistaken.AntyAFK
{
    /// <inheritdoc/>
    public class AntiAFKHandler : Module
    {
        /// <inheritdoc cref="Module.Module(Exiled.API.Interfaces.IPlugin{Exiled.API.Interfaces.IConfig})"/>
        public AntiAFKHandler(PluginHandler p)
            : base(p)
        {
        }

        /// <inheritdoc/>
        public override bool IsBasic => true;

        /// <inheritdoc/>
        public override string Name => "AntiAFK";

        /// <inheritdoc/>
        public override void OnEnable()
        {
            Exiled.Events.Handlers.Server.RoundStarted += this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound += this.Server_RestartingRound;
        }

        /// <inheritdoc/>
        public override void OnDisable()
        {
            Exiled.Events.Handlers.Server.RoundStarted -= this.Server_RoundStarted;
            Exiled.Events.Handlers.Server.RestartingRound -= this.Server_RestartingRound;
        }

        internal static readonly Dictionary<int, (int value, Vector3 lastPosition)> AfkPosition = new Dictionary<int, (int value, Vector3 lastPosition)>();

        private const string AfkMessage =
            @"<size=40><voffset=1em>
            <color=red><b><size=200>WARNING</size></b></color>
            <br><br><br><br><br>
            You have <color=yellow>{sLeft} seconds</color> to move or type '<color=yellow>.notafk</color>' in console('<color=yellow>~</color>')<br>
            or you will be kicked by AntyAFK system.
            </voffset></size>";

        private void Server_RestartingRound()
        {
            AfkPosition.Clear();
        }

        private void Server_RoundStarted()
        {
            this.RunCoroutine(this.AfkDetector(), "AfkDetector");
        }

        private IEnumerator<float> AfkDetector()
        {
            yield return Timing.WaitForSeconds(1);
            int rid = RoundPlus.RoundId;
            while (Round.IsStarted && rid == RoundPlus.RoundId)
            {
                this.CheckForAfk(5);
                yield return Timing.WaitForSeconds(5);
            }
        }

        private void CheckForAfk(int seconds)
        {
            foreach (var player in RealPlayers.List.Where(p => p.IsAlive && p.Role.Type != RoleType.Scp079 && !p.GetEffectActive<CustomPlayerEffects.Ensnared>()).ToArray())
            {
                if (Permissions.CheckPermission(player, PluginHandler.Instance.Name + ".anti_afk_kick_proof"))
                    continue;
                var ppos = player.Position;
                if (AfkPosition.TryGetValue(player.Id, out var value))
                {
                    int level = value.value;
                    Vector3 pos = value.lastPosition;
                    if (pos.x == ppos.x && pos.y == ppos.y && pos.z == ppos.z)
                    {
                        if (player.GetSessionVariable<bool>(SessionVarType.TALK))
                            continue;

                        AfkPosition[player.Id] = (level + seconds, ppos);

                        switch (level + seconds)
                        {
                            case int x when x >= 180:
                                {
                                    AfkPosition.Remove(player.Id);
                                    player.Disconnect("Anti AFK: You were AFK");
                                    RLogger.Log("ANTY AFK", "DISCONNECT", $"{player.PlayerToString()} was disconnected for being afk");
                                    MapPlus.Broadcast("Anti AFK", 10, $"{player.Nickname} was disconnected for being AFK", Broadcast.BroadcastFlags.AdminChat);

                                    break;
                                }

                            case int x when x >= 120:
                                {
                                    RLogger.Log("ANTY AFK", "WARN", $"{player.PlayerToString()} was warned for being afk");
                                    this.RunCoroutine(this.InformAFK(player), "InformAFK");

                                    break;
                                }
                        }
                    }
                    else
                        AfkPosition[player.Id] = (0, ppos);
                }
                else
                    AfkPosition.Add(player.Id, (0, ppos));
            }
        }

        private IEnumerator<float> InformAFK(Player p)
        {
            PseudoGUIHandler.Ignore(p);
            for (int i = 60; i > -1; i--)
            {
                if (p.IsConnected && p.Connection != null && AfkPosition.TryGetValue(p.Id, out var value))
                {
                    if (value.value >= 120)
                        p.ShowHint(AfkMessage.Replace("{sLeft}", i.ToString("00")), 2);
                    else
                    {
                        PseudoGUIHandler.StopIgnore(p);
                        yield break;
                    }
                }
                else
                {
                    PseudoGUIHandler.StopIgnore(p);
                    yield break;
                }

                yield return Timing.WaitForSeconds(1);
            }

            PseudoGUIHandler.StopIgnore(p);
        }
    }
}
