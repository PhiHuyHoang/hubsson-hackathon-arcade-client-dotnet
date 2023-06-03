using System;
using ClientGameState = Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.ClientGameState;

namespace Hubsson.Hackathon.Arcade.Client.Dotnet.Services
{
    public class MatchService
    {
        private MatchRepository _matchRepository;
        private ArcadeSettings _arcadeSettings;

        private readonly ILogger<MatchService> _logger;

        Random rnd = new Random();

        int MapWidth;
        int MapHeight;
        double tickMs;

        Domain.Action action;

        public MatchService(ArcadeSettings settings, ILogger<MatchService> logger)
        {
            _logger = logger;
            _matchRepository = new MatchRepository();
            _arcadeSettings = settings;
        }

        public void Init()
        {
            _logger.LogInformation("Init is running");

        }

        double DistanceBetweenPlayer(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)));
        }

        string MinDistance(Dictionary<string, double> allDistance)
        {
            return allDistance.MinBy(kvp => kvp.Value).Key;
        }
        public Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.Action Update(ClientGameState gameState)
        {

            tickMs = gameState.tickTimeInMs;
            MapWidth = gameState.width;
            MapHeight = gameState.height;

            Dictionary<string, double> allDistance = new Dictionary<string, double>();

            int numberOfPlayers = gameState.players.Length;

            int IndexOfOurTeam = Array.FindIndex(gameState.players, player => player.playerId == _arcadeSettings.TeamId);

            int OurlengthCoordinate = gameState.players[IndexOfOurTeam].coordinates.Length;
            int ourCurrentX = gameState.players[IndexOfOurTeam].coordinates[OurlengthCoordinate - 1].x;
            int ourCurrentY = gameState.players[IndexOfOurTeam].coordinates[OurlengthCoordinate - 1].y;

            for (int i = 0; i < numberOfPlayers; i++)
            {
                if (i != IndexOfOurTeam)
                {
                    string playerID = gameState.players[i].playerId;
                    int lengthCoordinate = gameState.players[i].coordinates.Length;
                    if (lengthCoordinate > 0)
                    {
                        int currentX = gameState.players[i].coordinates[lengthCoordinate - 1].x;
                        int currentY = gameState.players[i].coordinates[lengthCoordinate - 1].y;
                        double d = DistanceBetweenPlayer(ourCurrentX, ourCurrentY, currentX, currentY);
                        allDistance[playerID] = d;
                        _logger.LogInformation("distance between {0} and us is {1}", playerID, d.ToString());
                    }

                }

            }

            _logger.LogInformation("best player is: {0}", MinDistance(allDistance));

            int IndexOfBestPlayer = Array.FindIndex(gameState.players, player => player.playerId == MinDistance(allDistance));
            int BestPlayerlengthCoordinate = gameState.players[IndexOfBestPlayer].coordinates.Length;
            int bestPlayerX = gameState.players[IndexOfBestPlayer].coordinates[BestPlayerlengthCoordinate - 1].x;
            int bestPlayerY = gameState.players[IndexOfBestPlayer].coordinates[BestPlayerlengthCoordinate - 1].y;

            int x0 = ourCurrentX - bestPlayerX;
            int y0 = ourCurrentY - bestPlayerY;
            if (x0 < y0 && y0 >= 0 && ourCurrentY < MapHeight)
            {
                action = new Domain.Action()
                {
                    direction = Domain.Direction.Up,
                    iteration = gameState.iteration
                };
            }
            else if (x0 < y0 && y0 < 0 && ourCurrentX < MapWidth)
            {
                action = new Domain.Action()
                {
                    direction = Domain.Direction.Down,
                    iteration = gameState.iteration
                };
            }
            else if (x0 > y0 && x0 >= 0)
            {
                action = new Domain.Action()
                {
                    direction = Domain.Direction.Left,
                    iteration = gameState.iteration
                };
            }
            else
            {
                action = new Domain.Action()
                {
                    direction = Domain.Direction.Right,
                    iteration = gameState.iteration
                };
            }


            return action;

        }

        public class MatchRepository
        {
            public ClientGameState[] _clientGameState { get; set; }
        }
    }
}