using System;
using System.Threading;

namespace Zombies
{
    internal class Program
    {
        // Amount of zombies placed in the game
        public const int AmountZombie       = 5; // 10

        // Amount of holes placed in the game
        public const int AmountHole         = 25; // 20

        // Board height and length + 2
        public static int BoardLength       = 34; // 34

        // The string used for the walls
        public const string Wallstring      = "[#]";

        // The string used for the player
        public const string Playerstring    = " @ ";

        // The string used for zombies
        public const string Zombiestring    = "(Z)";

        // The string used for holes
        public const string Holestring      = "{O}";

        // The string used for nothing/air
        public const string Airstring       = "   ";

        private static string[,] playingField = new string[BoardLength, BoardLength];
        private static (int x, int y) playerpos;
        private static ConsoleColor defaultConsoleColor = Console.ForegroundColor;

        private static void Main()
        {
            Console.CursorVisible = false;
            Console.CancelKeyPress += (sender, e) => {
                Console.ForegroundColor = defaultConsoleColor;
                Console.CursorVisible = true;
                Console.Clear();
                Console.WriteLine("Rage quitting...");
                Environment.Exit(0);
            };

            while (true)
            {
                Console.Clear();

                CreateBoard();
                PlacePlayer();
                PlaceZombiesAndHoles();
                PrintBoard();
                PlayTurns();
            }
        }

        private static void CreateBoard()
        {
            for (int x = 0; x < BoardLength; x++)
                for (int y = 0; y < BoardLength; y++)
                    playingField[x, y] = Airstring;

            for (int x = 0; x < BoardLength; x++)
            {
                playingField[x, 0] = Wallstring;
                playingField[BoardLength - 1, x] = Wallstring;
            }

            for (int y = 0; y < BoardLength; y++)
            {
                playingField[0, y] = Wallstring;
                playingField[y, BoardLength - 1] = Wallstring;
            }
        }

        private static void PlacePlayer()
        {
            playerpos = ((int)Math.Floor(BoardLength / 2F),
                         (int)Math.Floor(BoardLength / 2F));

            playingField[
                    playerpos.x,
                    playerpos.y
                ] = Playerstring;
        }

        private static void PlaceZombiesAndHoles()
        {
            Random r = new((int)(DateTime.Now.Ticks % int.MaxValue));

            int holeCount = 0;
            while (holeCount < AmountHole + 1)
            {
                int holeX = r.Next(BoardLength);
                int holeY = r.Next(BoardLength);

                if (playingField[holeX, holeY] == Airstring)
                {
                    playingField[holeX, holeY] = Holestring;
                    holeCount++;
                }
            }

            int zombieCount = 0;
            while (zombieCount < AmountZombie + 1)
            {
                int zombX = r.Next(BoardLength);
                int zombY = r.Next(BoardLength);

                if (playingField[zombX, zombY] == Airstring)
                {
                    playingField[zombX, zombY] = Zombiestring;
                    zombieCount++;
                }
            }
        }

        private static void PlayTurns()
        {
            while (true)
            {
                /*
                 * Listen for player input
                */

                int xOffset = 0;
                int yOffset = 0;

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.NumPad1: // Down - Left
                        xOffset = -1;
                        yOffset = 1;
                        break;
                    case ConsoleKey.NumPad2: // Down
                        yOffset = 1;
                        break;
                    case ConsoleKey.NumPad3: // Down - Right
                        xOffset = 1;
                        yOffset = 1;
                        break;
                    case ConsoleKey.NumPad4: // Left
                        xOffset = -1;
                        break;
                    case ConsoleKey.NumPad6: // Right
                        xOffset = 1;
                        break;
                    case ConsoleKey.NumPad7: // Up - Left
                        xOffset = -1;
                        yOffset = -1;
                        break;
                    case ConsoleKey.NumPad8: // Up
                        yOffset = -1;
                        break;
                    case ConsoleKey.NumPad9: // Up - Right
                        xOffset = 1;
                        yOffset = -1;
                        break;
                }

                if (playingField[playerpos.x + xOffset, playerpos.y + yOffset] == Airstring)
                {
                    MovePlayer(playerpos, xOffset, yOffset);
                    playerpos.x += xOffset;
                    playerpos.y += yOffset;
                }

                /*
                 * Moving zombies 
                */

                if(!MoveZombies(out bool zombiesStillAlive))
                {
                    return;
                }

                if (!zombiesStillAlive)
                {
                    Console.ForegroundColor = defaultConsoleColor;
                    Console.Clear();
                    Console.WriteLine("You won!\nPress any key to play again");
                    Console.ReadKey(true);
                    return;
                }
            }
        }

        private static bool MoveZombies(out bool zombiesStillAlive)
        {
            string[,] playingFieldBuffer = (string[,])playingField.Clone();
            zombiesStillAlive = false;

            for (int y = 0; y < BoardLength; y++)
            {
                for (int x = 0; x < BoardLength; x++)
                {
                    if (playingField[x, y] == Zombiestring)
                    {
                        int distanceX = playerpos.x - x;
                        int distanceY = playerpos.y - y;

                        int xMoveOffset = 0;
                        int yMoveOffset = 0;

                        if (Math.Abs(distanceX) == Math.Abs(distanceY))
                        {
                            xMoveOffset = distanceX < 0 ? -1 : 1;
                            yMoveOffset = distanceY < 0 ? -1 : 1;
                        }
                        else if (Math.Abs(distanceX) > Math.Abs(distanceY))
                        {
                            xMoveOffset = distanceX < 0 ? -1 : 1;
                        }
                        else
                        {
                            yMoveOffset = distanceY < 0 ? -1 : 1;
                        }


                        switch (playingFieldBuffer[x + xMoveOffset, y + yMoveOffset])
                        {
                            case Zombiestring:
                            case Holestring:

                                // Zombie eats another zombie / Zombie falls in hole
                                RemoveZombie(playingFieldBuffer, (x, y));
                                break;

                            case Airstring:

                                // Zombie moves towards player/death
                                MoveZombie(playingFieldBuffer, (x, y), xMoveOffset, yMoveOffset);
                                
                                zombiesStillAlive = true;
                                break;

                            case Playerstring:
                                // Player died returning false
                                PrintDeathScreen(playingFieldBuffer);
                                
                                Console.ReadKey(true);
                                return false;
                        }

                    }
                }
            }
            
            playingField = playingFieldBuffer;

            return true;
        }

        private static void PrintBoard()
        {
            Console.SetCursorPosition(0, 0);

            for (int y = 0; y < BoardLength; y++)
            {
                for (int x = 0; x < BoardLength; x++)
                {
                    switch (playingField[x, y])
                    {
                        case Wallstring:
                            PrintWallString();
                            break;
                        case Holestring:
                            PrintHoleString();
                            break;
                        case Playerstring:
                            PrintPlayerString();
                            break;
                        case Zombiestring:
                            PrintZombieString();
                            break;
                        case Airstring:
                            PrintAirString();
                            break;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }
        }

        private static void PrintDeathScreen(string[,] playingField)
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkGray;

            for (int y = 0; y < BoardLength; y++)
            {
                for (int x = 0; x < BoardLength; x++)
                {

                    if(playingField[x, y] == Playerstring)
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(playingField[x, y]);
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    } else
                    {
                        Console.Write(playingField[x, y]);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine();
            }

            Console.ForegroundColor = defaultConsoleColor;
            Console.WriteLine("\n\nYou died!\nPress any key to play again");
        }

        private static void PrintWallString()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Wallstring);
        }

        private static void PrintHoleString()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(Holestring);
        }

        private static void PrintPlayerString()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(Playerstring);
        }

        private static void PrintZombieString()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(Zombiestring);
        }

        private static void PrintAirString() {
            Console.Write(Airstring);
        }

        private static void MovePlayer((int x, int y) playerPos, int xOffset, int yOffset)
        {
            playingField[playerPos.x, playerPos.y] = Airstring;
            playingField[playerPos.x + xOffset, playerPos.y + yOffset] = Playerstring;

            Console.SetCursorPosition(playerpos.x * 3, playerpos.y);
            PrintAirString();
            Console.SetCursorPosition((playerpos.x + xOffset) * 3, playerpos.y + yOffset);
            PrintPlayerString();
        }

        private static void MoveZombie(string[,] playingField, (int x, int y) zombiePos, int xOffset, int yOffset)
        {
            playingField[zombiePos.x, zombiePos.y] = Airstring;
            playingField[zombiePos.x + xOffset, zombiePos.y + yOffset] = Zombiestring;

            Console.SetCursorPosition(zombiePos.x * 3, zombiePos.y);
            PrintAirString();
            Console.SetCursorPosition((zombiePos.x + xOffset) * 3, zombiePos.y + yOffset);
            PrintZombieString();
        }

        private static void RemoveZombie(string[,] playingField, (int x, int y) zombiePos)
        {
            playingField[zombiePos.x, zombiePos.y] = Airstring;

            Console.SetCursorPosition(zombiePos.x * 3, zombiePos.y);
            PrintAirString();
        }
    }
}