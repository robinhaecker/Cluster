using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Mathematics;
using OpenTK.Input;
using static Cluster.GameMechanics.Interface.Commons.Properties;

namespace Cluster.GameMechanics.Interface
{
    public class UnitSelection : ISelection
    {
        private readonly List<Unit> units = new List<Unit>();

        private Panel panel;

        public UnitSelection()
        {
            panel = new Panel(10, GameWindow.active.height - BUTTON_SIZE_DEFAULT, 10, 1);
        }

        public Target pickTarget(float x, float y)
        {
            return null;
        }

        public bool selectByPick(float x, float y)
        {
            if (!(GameWindow.keyboard.IsKeyDown(Key.ControlLeft) || GameWindow.keyboard.IsKeyDown(Key.ControlRight)))
            {
                deselectAll();
            }

            var unit = pick(x, y);
            if (unit != null)
            {
                units.Add(unit);
            }

            return units.Count > 0;
        }

        private Unit pick(float x, float y)
        {
            var sector = Sector.get(x, y);
            foreach (var listOfSectorShips in sector.ships)
            {
                foreach (var unit in listOfSectorShips)
                {
                    if (Math.Sqrt((unit.x - x) * (unit.x - x) + (unit.y - y) * (unit.y - y)) <
                        unit.getPrototype().shapeScaling * unit.getPrototype().shape.radius + 20.0f)
                    {
                        return unit;
                    }
                }
            }

            return null;
        }

        public bool selectByBox(Vec4 box)
        {
            deselectAll();
            float cx = (box.x + box.z) * 0.5f;
            float cy = (box.y + box.w) * 0.5f;
            float dx = Math.Abs(box.x - box.z) * 0.5f;
            float dy = Math.Abs(box.y - box.w) * 0.5f;

            foreach (Unit unit in Unit.units)
            {
                var isInBox = (Math.Abs(cx - unit.x) < dx && Math.Abs(cy - unit.y) < dy);
                var mayBeAdded = unit.isAlive() && unit.getOwner().getId() == Civilisation.player;
                var notYetInList = !units.Contains(unit);
                if (isInBox && mayBeAdded && notYetInList)
                {
                    units.Add(unit);
                    unit.isSelected = true;
                }
            }

            return units.Count > 0;
        }

        public bool isMouseOver()
        {
            return panel.isMouseOver();
        }

        public string getToolTipText()
        {
            var elementAtMousePosition = panel.getElementAtMousePosition();
            return (elementAtMousePosition as IToolTip)?.getInfoText();
        }

        public Vec2 getCenterOfMass()
        {
            return new Vec2(units[0].x, units[0].y);
        }

        public void setActive(bool active)
        {
            if (active)
            {
                panel.enable();
            }
            else
            {
                panel.disable();
            }
        }

        public void updateGui()
        {
            commandUnits();
            panel.enable();
            panel.clear();

            Counter<Prototype> counter = new Counter<Prototype>(getPrototypes());
            foreach (Prototype prototype in counter.getKeys())
            {
                var button = new ProgressBar(prototype.shape);
                panel.addElement(button);
                button.setAnzahlFolgende(counter.getCount(prototype));
                button.setProgress(0);
                button.onLeftClick(() =>
                {
                    unselectAllUnitsExcept(prototype);
                    return true;
                });
                button.onRightClick(() =>
                {
                    unselectAllUnitsWith(prototype);
                    return true;
                });
            }

            panel.updateState();
        }

        public void unselectAllUnitsExcept(Prototype prototype)
        {
            var toRemove = chooseUnitsExceptPrototype(prototype);
            removeTheseUnitsFromSelection(toRemove);
        }

        public void unselectAllUnitsWith(Prototype prototype)
        {
            var toRemove = chooseUnitsOfPrototype(prototype);
            removeTheseUnitsFromSelection(toRemove);
        }

        private IEnumerable<Unit> chooseUnitsOfPrototype(Prototype prototype)
        {
            List<Unit> list = new List<Unit>();
            foreach (Unit unit in units)
            {
                if (unit.getPrototype() == prototype)
                {
                    list.Add(unit);
                }
            }

            return list;
        }

        private IEnumerable<Unit> chooseUnitsExceptPrototype(Prototype prototype)
        {
            List<Unit> list = new List<Unit>();
            foreach (Unit unit in units)
            {
                if (unit.getPrototype() != prototype)
                {
                    list.Add(unit);
                }
            }

            return list;
        }

        private void removeTheseUnitsFromSelection(IEnumerable<Unit> toRemove)
        {
            foreach (Unit unit in toRemove)
            {
                unit.isSelected = false;
                units.Remove(unit);
            }
        }

        private IEnumerable<Prototype> getPrototypes()
        {
            foreach (var unit in units)
            {
                yield return unit.getPrototype();
            }
        }

        private void commandUnits()
        {
            if (GuiMouse.mouseHitRight && !CombinedGui.isMouseOver())
            {
                Target target = MoveAndSelect.pickTargetAtMousePosition();
                foreach (Unit unit in Unit.filterByOwner(units, Civilisation.getPlayer()))
                {
                    unit.updateTarget(target);
                }
            }
        }

        public void renderGui()
        {
            panel?.render();
        }

        private void deselectAll()
        {
            foreach (var unit in units)
            {
                unit.isSelected = false;
            }

            units.Clear();
        }
    }
}