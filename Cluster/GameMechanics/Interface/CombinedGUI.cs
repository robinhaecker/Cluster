using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface
{
    public class CombinedGui
    {
        public static readonly CombinedGui combinedGui = new CombinedGui();

        private static int _fps;
        private static int _fps2;
        private static int _msec;
        private static int _msec2;

        private readonly List<IGui> _elements;
        private readonly InfoBox _ressourcesInfoBox;
        private readonly InfoBox _performanceInfoBox;
        public readonly InfoBox toolTip;

        public static void init()
        {
            MoveAndSelect.init();
        }

        private CombinedGui()
        {
            _elements = new List<IGui>();

            _ressourcesInfoBox = new InfoBox(10, 10);
            _performanceInfoBox = new InfoBox(GameWindow.active.width, 10, InfoBox.SpecifiedCorner.UPPER_RIGHT);
            toolTip = new InfoBox(10, GameWindow.active.height - 120, InfoBox.SpecifiedCorner.LOWER_LEFT);
            
            _elements.Add(_ressourcesInfoBox);
            _elements.Add(_performanceInfoBox);
            _elements.Add(toolTip);
        }

        public static void update()
        {
            MoveAndSelect.update();
            combinedGui.updateInfos();
            foreach (var element in combinedGui._elements)
            {
                element.updateState();
            }
            GuiMouse.update();
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
            if (MoveAndSelect.isMouseOver())
            {
                return true;
            }
            
            foreach (var element in combinedGui._elements)
            {
                if (element.isMouseOver())
                {
                    return true;
                }
            }

            return false;
        }
        
        private void updateInfos()
        {
            var player = Civilisation.getPlayer();
            _ressourcesInfoBox.setText("Ressourcen: \t" + (int) player.ressources + "\n" +
                                       "Forschung:\t" + (int) player.science + "\n" +
                                       "Bevölkerung:\t" + player.population + " / " + player.maxPopulation);

            _performanceInfoBox.setText("FPS: " + _fps + "\n" +
                                        "1/FPS: " + (100.0f / (float) _fps) + " ms von 16 ms\n" +
                                        "Particles rendered: " + Particle.rendered_count);
            
            //Frames per second
            _fps2++;
            _msec2 = Environment.TickCount;
            if (_msec2 - _msec >= 1000)
            {
                _msec = _msec2;
                _fps = _fps2;
                _fps2 = 0;
            }
        }
    }
}