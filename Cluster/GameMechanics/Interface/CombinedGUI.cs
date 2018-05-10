﻿using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface
{
    public class CombinedGui
    {
        private static readonly CombinedGui combinedGui = new CombinedGui();

        private static int fps;
        private static int fps2;
        private static int msec;
        private static int msec2;

        private readonly List<IGui> elements;
        private readonly InfoBox ressourcesInfoBox;
        private readonly InfoBox performanceInfoBox;
        private readonly InfoBox toolTipBox;

        public static void init()
        {
            MoveAndSelect.init();
        }

        private CombinedGui()
        {
            elements = new List<IGui>();

            ressourcesInfoBox = new InfoBox(10, 10);
            performanceInfoBox = new InfoBox(GameWindow.active.width, 10, InfoBox.SpecifiedCorner.UPPER_RIGHT);
            toolTipBox = new InfoBox(10, GameWindow.active.height - 130, InfoBox.SpecifiedCorner.LOWER_LEFT);
            elements.Add(ressourcesInfoBox);
            elements.Add(performanceInfoBox);
            elements.Add(toolTipBox);
        }

        public static void update()
        {
            MoveAndSelect.update();
            combinedGui.updateInfos();
            foreach (var element in combinedGui.elements)
            {
                element.updateState();
            }
            GuiMouse.update();
        }

        public static void render()
        {
            Primitives.setDepth(-0.1f);
            MoveAndSelect.render();
            foreach (var element in combinedGui.elements)
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
            
            foreach (var element in combinedGui.elements)
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
            ressourcesInfoBox.setText("Ressourcen: \t" + (int) player.ressources + "\n" +
                                       "Forschung:\t" + (int) player.science + "\n" +
                                       "Bevölkerung:\t" + player.population + " / " + player.maxPopulation);

            performanceInfoBox.setText("FPS: " + fps + "\n" +
                                        "1/FPS: " + (100.0f / (float) fps) + " ms von 16 ms\n" +
                                        "Particles rendered: " + Particle.renderedCount);
            
            toolTipBox.setText(MoveAndSelect.getToolTipText());
            //Frames per second
            fps2++;
            msec2 = Environment.TickCount;
            if (msec2 - msec >= 1000)
            {
                msec = msec2;
                fps = fps2;
                fps2 = 0;
            }
        }
    }
}