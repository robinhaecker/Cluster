using System;
using Cluster.Mathematics;
using Cluster.Rendering.Draw2D;

namespace Cluster.GameMechanics.Interface.Commons
{
    public class Button : IGui, IToolTip
    {
        public enum ButtonState
        {
            NONE,
            MOUSE_OVER,
            LEFT_CLICKED,
            RIGHT_CLICKED
        }

        protected bool active;
        private String toolTip;
        protected ButtonState _state;
        protected float x;
        protected float y;
        protected float width;
        protected float height;
        protected Vec4 _color;

        internal Button(float x, float y, float size = Properties.BUTTON_SIZE_DEFAULT)
        {
            active = true;
            this.x = x;
            this.y = y;
            width = height = size;
        }

        public void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public void updateState()
        {
            _state = ButtonState.NONE;
            if (GuiMouse.isMouseInRect(x, y, width, height))
            {
                _state = ButtonState.MOUSE_OVER;
                if (GuiMouse.mouseHitLeft)
                {
                    _state = ButtonState.LEFT_CLICKED;
                }
                else if (GuiMouse.mouseHitRight)
                {
                    _state = ButtonState.RIGHT_CLICKED;
                }
            }

            updateColor();
        }

        protected void updateColor()
        {
            if (isMouseOver())
            {
                _color = Properties.colorHighlight;
            }
            else
            {
                _color = Properties.colorDefault;
            }
        }

        public bool isMouseOver()
        {
            return _state > ButtonState.NONE;
        }

        public bool isActive()
        {
            return active;
        }

        public void render()
        {
            Primitives.setColor(_color.r(), _color.g(), _color.b(), _color.a());
            Primitives.drawRect(x, y, width, height);
        }

        public string getInfoText()
        {
            return toolTip;
        }

        public void setInfoText(string infoText)
        {
            toolTip = infoText;
        }
    }
}