using System.Diagnostics.CodeAnalysis;
using OpenTK;

namespace Cluster
{
    class Program
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public static void Main(string[] args)
        {
            using (GameWindow game = new GameWindow(DisplayDevice.Default.Width, DisplayDevice.Default.Height)) //800,800))
            {
                game.VSync = VSyncMode.Off;
                game.Run();
            }
        }
    }
}