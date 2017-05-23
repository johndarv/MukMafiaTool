namespace Tests.Helpers
{
    using System.Collections.Generic;
    using MukMafiaTool.Model;

    internal static class PlayerGenerator
    {
        internal static Player GeneratePlayer(string name, IEnumerable<string> aliases)
        {
            return GeneratePlayer(name, aliases, true);
        }

        internal static Player GeneratePlayer(string name, IEnumerable<string> aliases, bool participating)
        {
            var player = new Player
            {
                Name = name,
                Participating = participating,
            };

            player.AddAliases(aliases);

            return player;
        }
    }
}