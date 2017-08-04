using shared;
using GameModeID = proto_game.GameMode;

namespace arena.battle.modes
{
    class GameModeFactory :Singleton<GameModeFactory>
    {
        public GameMode Get(GameModeID gameModeID)
        {
            GameMode mode = null;
            switch (gameModeID)
            {
                case GameModeID.FFA:
                    mode = new FFA();
                    break;
            }
            return mode;
        }
    }
}
