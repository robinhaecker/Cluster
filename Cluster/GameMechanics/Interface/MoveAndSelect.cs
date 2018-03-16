using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Mathematics;
using Cluster.Rendering.Draw2D;
using OpenTK.Input;

namespace Cluster.GameMechanics.Interface
{
    public class MoveAndSelect
    {
        private const float SCROLL_BOUNDARY = 7000.0f;
        private const float SCROLL_SPEED = 1.75f;
        private const float ZOOM_MAX = 1.75f;
        private const float ZOOM_MIN = 0.08f;

        private static Vec3 _focus = null;
        private static ISelection _selection = null;

        private static Vec4 _selectionBox = null;
        private static float _selMx;
        private static float _selMy;
        private static readonly List<ISelection> _selections = new List<ISelection>();

        public static void init()
        {
            _selections.Add(new PlanetSelection());
            _selections.Add(new UnitSelection());
        }

        private MoveAndSelect()
        {
        }

        public static void update()
        {
            changeView();
            selectStuff();
            if (_selection != null)
            {
                _selection.updateGui();
            }
        }

        public static bool isMouseOver()
        {
            if (_selection != null)
            {
                return _selection.isMouseOver();
            }

            return false;
        }

        private static void selectStuff()
        {
            if (GuiMouse.mouseHitLeft && !CombinedGui.isMouseOver())
            {
                if (_selectionBox == null) // Selektionsbox anfangen.
                {
                    _selMx = GuiMouse.mouseX;
                    _selMy = GuiMouse.mouseY;
                    _selectionBox = new Vec4(
                        Space.screenToSpaceX(GuiMouse.mouseX),
                        Space.screenToSpaceY(GuiMouse.mouseY),
                        0.0f,
                        0.0f);
                }

                pick();
            }
            else if (GuiMouse.mousePressLeft && !GuiMouse.mouseHitLeft && _selectionBox != null)
            {
                // Selektionsbox-Ende ziehen bei gedrückter linker Maustaste.
                _selectionBox.z = Space.screenToSpaceX(GuiMouse.mouseX);
                _selectionBox.w = Space.screenToSpaceY(GuiMouse.mouseY);
            }
            else if (_selectionBox != null) // Maustaste nicht mehr gedrückt? Selektionsbox beenden.
            {
                if (Math.Abs(GuiMouse.mouseX - _selMx) + Math.Abs(GuiMouse.mouseY - _selMy) > 20.0f)
                {
                    selectByBox(_selectionBox);
                }

                _selectionBox = null;
            }
        }

        private static void pick()
        {
            float mSpaceX = Space.screenToSpaceX(GuiMouse.mouseX);
            float mSpaceY = Space.screenToSpaceY(GuiMouse.mouseY);
            selectByPick(mSpaceX, mSpaceY);
        }

        private static void selectByPick(float mSpaceX, float mSpaceY)
        {
            _selection = null;
            foreach (ISelection selection in _selections)
            {
                selection.setActive(false);
                if (selection.selectByPick(mSpaceX, mSpaceY))
                {
                    selection.setActive(true);
                    _selection = selection;
                    return;
                }
            }
        }

        private static void selectByBox(Vec4 box)
        {
            _selection = null;
            foreach (ISelection selection in _selections)
            {
                selection.setActive(false);
                if (selection.selectByBox(box))
                {
                    selection.setActive(true);
                    _selection = selection;
                    return;
                }
            }
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
            if (_focus == null)
            {
                return;
            }

            Space.scrollX = Space.scrollX * _focus.z + _focus.x * (1.0f - _focus.z);
            Space.scrollY = Space.scrollY * _focus.z + _focus.y * (1.0f - _focus.z);
            _focus.z -= 0.002f;
            if (_focus.z <= 0.0f)
            {
                _focus = null;
            }
        }

        public static void render()
        {
            renderSelectionBox();
            if (_selection != null)
            {
                _selection.renderGui();
            }
        }

        private static void renderSelectionBox()
        {
            if (_selectionBox == null || !(Math.Abs(_selectionBox.z) + Math.Abs(_selectionBox.w) > 0.000001f))
            {
                return;
            }

            Primitives.setColor(1.0f, 1.0f, 1.0f, 0.125f);
            Primitives.setLineWidth(2.0f);
            var box = new Vec4(
                Space.spaceToScreenX(_selectionBox.x),
                Space.spaceToScreenY(_selectionBox.y),
                Space.spaceToScreenX(_selectionBox.z),
                Space.spaceToScreenY(_selectionBox.w));

            Primitives.drawLine(box.x, box.y, box.x, box.w);
            Primitives.drawLine(box.z, box.y, box.z, box.w);
            Primitives.drawLine(box.x, box.y, box.z, box.y);
            Primitives.drawLine(box.x, box.w, box.z, box.w);
            Primitives.setLineWidth();
        }
    }
}