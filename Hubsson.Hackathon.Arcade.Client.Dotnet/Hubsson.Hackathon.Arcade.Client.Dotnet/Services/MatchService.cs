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

        int IndexOfOurTeam;

        int OurlengthCoordinate;
        int ourCurrentX ;
        int ourCurrentY;

        Domain.Action action;

        int initial = 0;
        bool[,] grid;
        public MatchService(ArcadeSettings settings, ILogger<MatchService> logger)
        {
            _logger = logger;
            _matchRepository = new MatchRepository();
            _arcadeSettings = settings;
        }

        public void Init()
        {
            _logger.LogInformation("Init is running");
            initial += 1;     

        }

        double DistanceBetweenPlayer(int x1, int y1, int x2, int y2)
        {
            return Math.Sqrt((Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2)));
        }

        string MinDistance(Dictionary<string, double> allDistance)
        {
            return allDistance.MinBy(kvp => kvp.Value).Key;
        }

        private static List<string> RemoveWhereOccupied(int x, int y, bool[,] grid, int MapWidth, int MapHeight, List<string> validDirections)
        {
            if (x > 0 && grid[x - 1, y] && validDirections.Any(d => d == "Right")) { }
            
                validDirections.Remove("Right"); // can't go left
            
            if (x < MapWidth -1 && grid[x + 1, y] && validDirections.Any(d => d == "Left"))
            
                validDirections.Remove("Left"); // can't go right
            
                
            if (y > 0 && grid[x, y - 1] && validDirections.Any(d => d == "Up"))
            
                validDirections.Remove("Up"); // can't go up
            
                
            if (y < MapHeight -1 && grid[x, y + 1] && validDirections.Any(d => d == "Down"))
            
                validDirections.Remove("Down"); // can't go down
            
                

           // _logger.LogInformation("here: {0} {1} {2}", x.ToString(), y.ToString(), string.Join(",",validDirections));
            return validDirections;
        }
        

        private static string RandomMouse(List<string> directions)
        {
            var randomiser = new Random(DateTime.Now.Millisecond);
            return directions[randomiser.Next(directions.Count)];
        }
        public Hubsson.Hackathon.Arcade.Client.Dotnet.Domain.Action Update(ClientGameState gameState)
        {
            List<string> directions = new List<string> { "Up", "Down", "Left", "Right" };
            if (initial <= 1)
            {
                tickMs = gameState.tickTimeInMs;
                MapWidth = gameState.width;
                MapHeight = gameState.height;
                grid = new bool[MapWidth, MapHeight];
                IndexOfOurTeam = Array.FindIndex(gameState.players, player => player.playerId == _arcadeSettings.TeamId);

            }
            initial = 2;
            _logger.LogInformation(initial.ToString());

            Dictionary<string, double> allDistance = new Dictionary<string, double>();
            int numberOfPlayers = gameState.players.Length;
            OurlengthCoordinate = gameState.players[IndexOfOurTeam].coordinates.Length;
            ourCurrentX = gameState.players[IndexOfOurTeam].coordinates[OurlengthCoordinate - 1].x;
            ourCurrentY = gameState.players[IndexOfOurTeam].coordinates[OurlengthCoordinate - 1].y;
            _logger.LogInformation("{0}, {1}",ourCurrentX.ToString(), ourCurrentY.ToString());
            //grid[ourCurrentX, ourCurrentY] = true;
            
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
                        //_logger.LogInformation("distance between {0} and us is {1}", playerID, d.ToString());
                    }

                }

            }

            var validDirections = RemoveWhereOccupied(ourCurrentX,ourCurrentY, grid, MapWidth, MapHeight, directions);
            _logger.LogInformation("valid {0} grid {1}", string.Join(",",validDirections), grid[ourCurrentX-1,ourCurrentY-1].ToString());
            var nextMove = RandomMouse(validDirections);
            Domain.Direction myEnum = (Domain.Direction)Enum.Parse(typeof(Domain.Direction), nextMove);

            action = new Domain.Action()
            {
                direction = myEnum,
                iteration = gameState.iteration
            };

            //int IndexOfBestPlayer = Array.FindIndex(gameState.players, player => player.playerId == MinDistance(allDistance));
            //int BestPlayerlengthCoordinate = gameState.players[IndexOfBestPlayer].coordinates.Length;
            //int bestPlayerX = gameState.players[IndexOfBestPlayer].coordinates[BestPlayerlengthCoordinate - 1].x;
            //int bestPlayerY = gameState.players[IndexOfBestPlayer].coordinates[BestPlayerlengthCoordinate - 1].y;

            //int x0 = ourCurrentX - bestPlayerX;
            //int y0 = ourCurrentY - bestPlayerY;
            //if (x0 < y0 && y0 >= 0 && ourCurrentY < MapHeight)
            //{
            //    action = new Domain.Action()
            //    {
            //        direction = Domain.Direction.Up,
            //        iteration = gameState.iteration
            //    };
            //}
            //else if (x0 < y0 && y0 < 0 && ourCurrentX < MapWidth)
            //{
            //    action = new Domain.Action()
            //    {
            //        direction = Domain.Direction.Down,
            //        iteration = gameState.iteration
            //    };
            //}
            //else if (x0 > y0 && x0 >= 0)
            //{
            //    action = new Domain.Action()
            //    {
            //        direction = Domain.Direction.Left,
            //        iteration = gameState.iteration
            //    };
            //}
            //else
            //{
            //    action = new Domain.Action()
            //    {
            //        direction = Domain.Direction.Right,
            //        iteration = gameState.iteration
            //    };
            //}


            return action;

        }

        public class MatchRepository
        {
        }
    }
}