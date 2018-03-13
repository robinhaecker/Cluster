using System.Collections.Generic;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class Panel : IGui
    {
        private bool _active;
        private float _x;
        private float _y;
        private List<IGui> _elements;
        private int _gridWidth;
        private int _gridHeight;
        private float _gridSize;
        private int _indexX;
        private int _indexY;

        public Panel(float x, float y, int gridWidth, int gridHeight, float gridSize = Properties.BUTTON_SIZE_DEFAULT)
        {
            _active = true;
            _elements = new List<IGui>();
            _x = x;
            _y = y;
            _gridWidth = gridWidth;
            _gridHeight = gridHeight;
            _gridSize = gridSize;
            _indexX = 0;
            _indexY = 0;
        }

        public void setPosition(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public void addElement(IGui element)
        {
            _elements.Add(element);
            element.setPosition(_x + _indexX * _gridSize, _y + _indexY * _gridSize);
            _indexY++;
            if (_indexY >= _gridHeight)
            {
                _indexY = 0;
                _indexX++;
            }
        }

        public void addLargeElement(IGui largeElement)
        {
            if (_indexY > 0)
            {
                _indexY = 0;
                _indexX++;
            }
            addElement(largeElement);
            _indexX++;
            _indexY = 0;
        }

        public void updateState()
        {
            foreach (IGui element in _elements)
            {
                if (element.isActive())
                {
                    element.updateState();
                }
            }
        }

        public bool isMouseOver()
        {
            foreach (IGui element in _elements)
            {
                if (element.isActive() && element.isMouseOver())
                {
                    return true;
                }
            }

            return false;
        }

        public bool isActive()
        {
            return _active;
        }

        public void render()
        {
            foreach (IGui element in _elements)
            {
                if (element.isActive())
                {
                    element.render();
                }
            }
        }
    }
}