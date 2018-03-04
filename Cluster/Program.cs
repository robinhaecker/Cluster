using OpenTK;

namespace Cluster
{
    class Program
    {
        static void Main(string[] args)
        {
            using (GameWindow game = new GameWindow()) //800,800))
            {
                game.VSync = VSyncMode.Off;
                game.Run();
            }
        }
    }
}