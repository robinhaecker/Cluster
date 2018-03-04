using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Cluster
{
    class Program
    {
        static void Main(string[] args)
        {
            using (GameWindow game = new GameWindow())//800,800))
            {
                game.VSync = OpenTK.VSyncMode.Off;
                game.Run();
            }

        }







    }
}
