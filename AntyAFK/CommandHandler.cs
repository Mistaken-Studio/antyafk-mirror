// -----------------------------------------------------------------------
// <copyright file="CommandHandler.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using Mistaken.API.Commands;
using Mistaken.API.Extensions;

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
