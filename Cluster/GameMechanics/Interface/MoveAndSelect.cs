using System;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.GameMechanics.Universe;
using Cluster.Mathematics;
using OpenTK.Input;

namespace Cluster.GameMechanics.Interface
{
    public class MoveAndSelect
    {
        private const float SCROLL_BOUNDARY = 7000.0f;
        private const float SCROLL_SPEED = 1.75f;
        private const float ZOOM_MAX = 1.75f;
        private const float ZOOM_MIN = 0.08f;

        private static Vec3 _focus;
        private static ISelection _selection = null;

        private MoveAndSelect()
        {
        }

        public static void update()
        {
            changeView();
        }

        private static void changeView()
        {
            scrollView();
            updateFocus();
            moveViewToFocus();
        }

        private static void scrollView()
        {
            Space.zoom = Math.Min(ZOOM_MAX, Math.Max(ZOOM_MIN, Space.zoom + GuiMouse.mouseZSpeed * 0.027f));
            
            if (GuiMouse.mouseX < 10 && Space.scrollX > -SCROLL_BOUNDARY)
            {
                Space.scrollX -= SCROLL_SPEED / Space.zoom;
            }

            if (GuiMouse.mouseX > GameWindow.active.width - 10 && Space.scrollX < SCROLL_BOUNDARY)
            {
                Space.scrollX += SCROLL_SPEED / Space.zoom;
            }

            if (GuiMouse.mouseY < 10 && Space.scrollY < SCROLL_BOUNDARY)
            {
                Space.scrollY += SCROLL_SPEED / Space.zoom;
            }

            if (GuiMouse.mouseY > GameWindow.active.height - 10 && Space.scrollY > -SCROLL_BOUNDARY)
            {
                Space.scrollY -= SCROLL_SPEED / Space.zoom;
            }
        }

        private static void updateFocus()
        {
            if (!GameWindow.mouse.IsButtonDown(MouseButton.Middle) || _selection == null)
            {
                return;
            }

            var position = _selection.getCenterOfMass();
            _focus = new Vec3(position.x, position.y, 1.0f);
        }

        private static void moveViewToFocus()
        {
            if (_focus != null)
            {
                Space.scrollX = Space.scrollX * _focus.z + _focus.x * (1.0f - _focus.z);
                Space.scrollY = Space.scrollY * _focus.z + _focus.y * (1.0f - _focus.z);
                _focus.z -= 0.002f;
                if (_focus.z <= 0.0f) _focus = null;
            }
        }
    }
}