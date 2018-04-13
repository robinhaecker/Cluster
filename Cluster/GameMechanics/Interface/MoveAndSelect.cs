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

        private static Vec3 focus = null;
        private static ISelection selection = null;

        private static Vec4 selectionBox = null;
        private static float selMx;
        private static float selMy;
        private static readonly List<ISelection> selections = new List<ISelection>();

        public static void init()
        {
            selections.Add(new PlanetSelection());
            selections.Add(new UnitSelection());
        }

        private MoveAndSelect()
        {
        }

        public static void update()
        {
            changeView();
            selectStuff();
            if (selection != null)
            {
                selection.updateGui();
            }
        }

        public static bool isMouseOver()
        {
            if (selection != null)
            {
                return selection.isMouseOver();
            }

            return false;
        }

        private static void selectStuff()
        {
            if (GuiMouse.mouseHitLeft && !CombinedGui.isMouseOver())
            {
                if (selectionBox == null) // Selektionsbox anfangen.
                {
                    selMx = GuiMouse.mouseX;
                    selMy = GuiMouse.mouseY;
                    selectionBox = new Vec4(
                        Space.screenToSpaceX(GuiMouse.mouseX),
                        Space.screenToSpaceY(GuiMouse.mouseY),
                        0.0f,
                        0.0f);
                }

                pick();
            }
            else if (GuiMouse.mousePressLeft && !GuiMouse.mouseHitLeft && selectionBox != null)
            {
                // Selektionsbox-Ende ziehen bei gedrückter linker Maustaste.
                selectionBox.z = Space.screenToSpaceX(GuiMouse.mouseX);
                selectionBox.w = Space.screenToSpaceY(GuiMouse.mouseY);
            }
            else if (selectionBox != null) // Maustaste nicht mehr gedrückt? Selektionsbox beenden.
            {
                if (Math.Abs(GuiMouse.mouseX - selMx) + Math.Abs(GuiMouse.mouseY - selMy) > 20.0f)
                {
                    selectByBox(selectionBox);
                }

                selectionBox = null;
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
            selection = null;
            foreach (ISelection selection in selections)
            {
                selection.setActive(false);
                if (selection.selectByPick(mSpaceX, mSpaceY))
                {
                    selection.setActive(true);
                    MoveAndSelect.selection = selection;
                    return;
                }
            }
        }

        private static void selectByBox(Vec4 box)
        {
            selection = null;
            foreach (ISelection selection in selections)
            {
                selection.setActive(false);
                if (selection.selectByBox(box))
                {
                    selection.setActive(true);
                    MoveAndSelect.selection = selection;
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
            if (!GameWindow.mouse.IsButtonDown(MouseButton.Middle) || selection == null)
            {
                return;
            }

            var position = selection.getCenterOfMass();
            focus = new Vec3(position.x, position.y, 1.0f);
        }

        private static void moveViewToFocus()
        {
            if (focus == null)
            {
                return;
            }

            Space.scrollX = Space.scrollX * focus.z + focus.x * (1.0f - focus.z);
            Space.scrollY = Space.scrollY * focus.z + focus.y * (1.0f - focus.z);
            focus.z -= 0.002f;
            if (focus.z <= 0.0f)
            {
                focus = null;
            }
        }

        public static void render()
        {
            renderSelectionBox();
            if (selection != null)
            {
                selection.renderGui();
            }
        }

        private static void renderSelectionBox()
        {
            if (selectionBox == null || !(Math.Abs(selectionBox.z) + Math.Abs(selectionBox.w) > 0.000001f))
            {
                return;
            }

            Primitives.setColor(1.0f, 1.0f, 1.0f, 0.125f);
            Primitives.setLineWidth(2.0f);
            var box = new Vec4(
                Space.spaceToScreenX(selectionBox.x),
                Space.spaceToScreenY(selectionBox.y),
                Space.spaceToScreenX(selectionBox.z),
                Space.spaceToScreenY(selectionBox.w));

            Primitives.drawLine(box.x, box.y, box.x, box.w);
            Primitives.drawLine(box.z, box.y, box.z, box.w);
            Primitives.drawLine(box.x, box.y, box.z, box.y);
            Primitives.drawLine(box.x, box.w, box.z, box.w);
            Primitives.setLineWidth();
        }
    }
}