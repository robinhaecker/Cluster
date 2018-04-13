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
        
        private bool active;
        private float x;
        private float y;
        private SpecifiedCorner corner;
        private String text;

        public InfoBox(float x, float y, SpecifiedCorner corner = SpecifiedCorner.UPPER_LEFT)
        {
            this.x = x;
            this.y = y;
            this.corner = corner;
            this.text = "";
        }

        public void setPosition(float x, float y)
        {
            setPosition(x, y, SpecifiedCorner.UPPER_LEFT);
        }

        public void setPosition(float x, float y, SpecifiedCorner corner)
        {
            this.x = x;
            this.y = y;
            this.corner = corner;
        }

        public void setText(String text)
        {
            this.text = text;
        }

        public void updateState()
        {
            active = !String.IsNullOrWhiteSpace(text);
        }

        public bool isMouseOver()
        {
            return false;
        }
        
        public IGui getElementAtMousePosition()
        {
            return isMouseOver() ? this : null;
        }

        public bool isActive()
        {
            return active;
        }

        public void render()
        {
            Text.setTextSize();
            Text.drawText(text, getTextX(), getTextY());
        }

        private float getTextX()
        {
            if (corner == SpecifiedCorner.LOWER_LEFT || corner == SpecifiedCorner.UPPER_LEFT)
            {
                return x;
            }

            return x + Text.estimatedTextWidth(text);
        }

        private float getTextY()
        {
            if (corner == SpecifiedCorner.UPPER_LEFT || corner == SpecifiedCorner.UPPER_RIGHT)
            {
                return y;
            }

            return y - Text.textHeight(text);
        }
    }
}