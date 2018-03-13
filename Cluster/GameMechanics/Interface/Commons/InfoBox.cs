using System;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class InfoBox : IGui
    {
        public enum SpecifiedCorner
        {
            UPPER_LEFT,
            UPPER_RIGHT,
            LOWER_LEFT,
            LOWER_RIGHT
        }
        
        private bool _active;
        private float _x;
        private float _y;
        private SpecifiedCorner _corner;
        private String _text;

        public InfoBox(float x, float y, SpecifiedCorner corner = SpecifiedCorner.UPPER_LEFT)
        {
            _x = x;
            _y = y;
            _corner = corner;
        }

        public void setPosition(float x, float y)
        {
            setPosition(x, y, SpecifiedCorner.UPPER_LEFT);
        }

        public void setPosition(float x, float y, SpecifiedCorner corner)
        {
            _x = x;
            _y = y;
            _corner = corner;
        }

        public void setText(String text)
        {
            _text = text;
        }

        public void updateState()
        {
            _active = !String.IsNullOrWhiteSpace(_text);
        }

        public bool isMouseOver()
        {
            return false;
        }

        public bool isActive()
        {
            return _active;
        }

        public void render()
        {
            Text.setTextSize();
            Text.drawText(_text, getTextX(), getTextY());
        }

        private float getTextX()
        {
            if (_corner == SpecifiedCorner.LOWER_LEFT || _corner == SpecifiedCorner.UPPER_LEFT)
            {
                return _x;
            }

            return _x + Text.estimatedTextWidth(_text);
        }

        private float getTextY()
        {
            if (_corner == SpecifiedCorner.UPPER_LEFT || _corner == SpecifiedCorner.UPPER_RIGHT)
            {
                return _y;
            }

            return _y - Text.textHeight(_text);
        }
    }
}