﻿using System.Collections.Generic;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class Panel : IGui
    {
        private bool active;
        private float x;
        private float y;
        private List<IGui> elements;
        private int gridWidth;
        private int gridHeight;
        private float gridSize;
        private int indexX;
        private int indexY;

        public Panel(float x, float y, int gridWidth, int gridHeight, float gridSize = Properties.BUTTON_SIZE_DEFAULT)
        {
            active = true;
            elements = new List<IGui>();
            this.x = x;
            this.y = y;
            this.gridWidth = gridWidth;
            this.gridHeight = gridHeight;
            this.gridSize = gridSize + 2;
            indexX = 0;
            indexY = 0;
        }

        public void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public void enable()
        {
            active = true;
        }

        public void disable()
        {
            active = false;
        }

        public void clear()
        {
            elements.Clear();
            indexX = 0;
            indexY = 0;
        }

        public void addElement(IGui element)
        {
            elements.Add(element);
            element.setPosition(x + indexX * gridSize, y + indexY * gridSize);
            indexY++;
            if (indexY >= gridHeight)
            {
                indexY = 0;
                indexX++;
            }
        }

        public void addLargeElement(IGui largeElement)
        {
            addElement(largeElement);
            if (indexY > 0)
            {
                indexY = 0;
                indexX++;
            }
            indexX++;
            indexY = 0;
        }

        public void updateState()
        {
            foreach (IGui element in elements)
            {
                if (element.isActive())
                {
                    element.updateState();
                }
            }
        }

        public bool isMouseOver()
        {
            foreach (IGui element in elements)
            {
                if (element.isActive() && element.isMouseOver())
                {
                    return true;
                }
            }

            return false;
        }
        
        public IGui getElementAtMousePosition()
        {
            foreach (IGui element in elements)
            {
                var elementAtMousePosition = element.getElementAtMousePosition();
                if (elementAtMousePosition != null)
                {
                    return elementAtMousePosition;
                }
            }

            return null;
        }


        public bool isActive()
        {
            return active;
        }

        public void render()
        {
            foreach (IGui element in elements)
            {
                if (element.isActive())
                {
                    element.render();
                }
            }
        }
    }
}