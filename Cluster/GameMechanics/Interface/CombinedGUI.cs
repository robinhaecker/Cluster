using System.Collections.Generic;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface
{
    public class CombinedGui
    {
        public static readonly CombinedGui combinedGui = new CombinedGui();

        private List<IGui> _elements;

        public void init()
        {
            MoveAndSelect.init();
        }

        private CombinedGui()
        {
            _elements = new List<IGui>();
        }

        public static void update()
        {
            GuiMouse.update();
            foreach (var element in combinedGui._elements)
            {
                element.updateState();
            }
            
            MoveAndSelect.update();
        }

        public static void render()
        {
            Primitives.setDepth(-0.1f);
            MoveAndSelect.render();
            foreach (var element in combinedGui._elements)
            {
                element.render();
            }
        }

        public static bool isMouseOver()
        {
            foreach (var element in combinedGui._elements)
            {
                if (element.isMouseOver())
                {
                    return true;
                }
            }

            return false;
        }
    }
}