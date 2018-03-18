using System;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Interface.Commons;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.GameMechanics.Universe.LivingThings;
using Cluster.Mathematics;

namespace Cluster.GameMechanics.Interface
{
    public class PlanetSelection : ISelection
    {
        private Planet _planet;
        private int _pickedIndex;

        private Panel _panel;

        public PlanetSelection()
        {
            _panel = new Panel(10, GameWindow.active.height - 110, 10, 2);
        }

        public bool selectByPick(float x, float y)
        {
            foreach (Planet pl in Planet.planets)
            {
                double delx = x - pl.x;
                double dely = y - pl.y;
                double dist = delx * delx + dely * dely;
                if (dist < pl.size * pl.size * 625.0)
                {
                    _planet = pl;
                    _pickedIndex = -1;

                    if (Space.zoom > 0.3 && dist > pl.size * pl.size * 225.0)
                    {
                        double alpha = Math.Atan2(dely, delx) / (2.0 * Math.PI);
                        _pickedIndex = (int) Math.Floor(((alpha + 1.0) % 1.0) * pl.size);
                    }

                    return true;
                }
            }

            return false;
        }

        public bool selectByBox(Vec4 box)
        {
            _planet = null;
            _pickedIndex = -1;
            return false;
        }

        public void setActive(bool active)
        {
            if (active)
            {
                _panel.enable();
            }
            else
            {
                _panel.disable();
            }
        }

        public bool isMouseOver()
        {
            return _panel.isMouseOver();
        }

        public Vec2 getCenterOfMass()
        {
            return new Vec2(_planet.x, _planet.y);
        }

        public void updateGui()
        {
            _panel.enable();
            _panel.clear();
            if (_pickedIndex > -1)
            {
                if (_planet.infra[_pickedIndex] != null)
                {
                    updateGuiForBuilding();
                }
                else
                {
                    updateGuiForTerrain();
                }
            }

            _panel.updateState();
        }

        private void updateGuiForTerrain()
        {
            _panel.addLargeElement(
                new MeshButton(null, Planet.terraImage[(int) _planet.terra[_pickedIndex], 0],
                    0, 0, Commons.Properties.BUTTON_SIZE_LARGE));

            var player = Civilisation.getPlayer();
            if (_planet.canBuild(player))
            {
                foreach (var blueprint in Blueprint.listOfBlueprints(player, _planet.terra[_pickedIndex]))
                {
                    var meshButton = new MeshButton(blueprint.shape, null);
                    
                    string cres = "&r", cene = "&y";
                    if (blueprint.getCost() <= player.ressources) cres = "&g";
                    String text = "&h" + blueprint.name +
                           "\n" + cres + "Ressourcen:" + blueprint.getCost().ToString().PadLeft(4, ' ') +
                           "\n" + cene + "Energie:\t " + blueprint.getEnergyNeeds().ToString().PadLeft(4, ' ') +
                           "\n&n" + blueprint.description[0] + "\n" + blueprint.description[1] + "\n" +
                           blueprint.description[2];
                    
                    meshButton.setInfoText(text);
                    _panel.addElement(meshButton);
                }
            }
            
        }

        private void updateGuiForBuilding()
        {
            _panel.addLargeElement(
                new MeshButton(_planet.infra[_pickedIndex].blueprint.shape,
                        Planet.terraImage[(int) _planet.terra[_pickedIndex], 1],
                        0, 0, Commons.Properties.BUTTON_SIZE_LARGE)
                    .setColor(_planet.infra[_pickedIndex].owner.red * 0.5f + 0.5f,
                        _planet.infra[_pickedIndex].owner.green * 0.5f + 0.5f,
                        _planet.infra[_pickedIndex].owner.blue * 0.5f + 0.5f));
        }

        public void renderGui()
        {
            if (_panel != null)
            {
                _panel.render();
            }
        }
    }
}