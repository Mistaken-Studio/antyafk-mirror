// -----------------------------------------------------------------------
// <copyright file="AntiAFKHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Exiled.API.Features;
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
    [CommandSystem.CommandHandler(typeof(CommandSystem.ClientCommandHandler))]
    public class CommandHandler : IBetterCommand
    {
        /// <inheritdoc/>
        public override string Command => "notafk";

        /// <inheritdoc/>
        public override string[] Aliases => new string[] { };

        /// <inheritdoc/>
        public override string Description => "I'm not afk";

        /// <inheritdoc/>
        public override string[] Execute(global::CommandSystem.ICommandSender sender, string[] args, out bool success)
        {
            var player = sender.GetPlayer();
            if (AntiAFKHandler.AfkPosition.ContainsKey(player.Id))
                AntiAFKHandler.AfkPosition.Remove(player.Id);
            success = true;
            return new string[] { "Done" };
        }
    }
}
