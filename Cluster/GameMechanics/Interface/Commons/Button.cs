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
        protected ButtonState state;
        protected float x;
        protected float y;
        protected float width;
        protected float height;
        protected Vec4 color;
        
        internal Button(float x = 0, float y = 0, float size = Properties.BUTTON_SIZE_DEFAULT)
        {
            active = true;
            this.x = x;
            this.y = y;
            width = height = size;
            color = Properties.colorDefault;
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

        public void updateState()
        {
            state = ButtonState.NONE;
            if (GuiMouse.isMouseInRect(x, y, width, height))
            {
                state = ButtonState.MOUSE_OVER;
                if (GuiMouse.mouseHitLeft)
                {
                    state = ButtonState.LEFT_CLICKED;
                }
                else if (GuiMouse.mouseHitRight)
                {
                    state = ButtonState.RIGHT_CLICKED;
                }
            }

            updateColor();
        }

        protected void updateColor()
        {
            if (isMouseOver())
            {
                color = Properties.colorHighlight;
            }
            else
            {
                color = Properties.colorDefault;
            }
        }

        public bool isMouseOver()
        {
            return state > ButtonState.NONE;
        }

        public IGui getElementAtMousePosition()
        {
            return isMouseOver() ? this : null;
        }

        public bool isActive()
        {
            return active;
        }

        public virtual void render()
        {
            Primitives.setColor(color.r(), color.g(), color.b(), color.a());
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