﻿using System;
using Cluster.GameMechanics.Universe;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.Mathematics;

namespace Cluster.GameMechanics.Interface
{
    public class PlanetSelection : ISelection
    {
        private Planet _planet;
        private int _pickedIndex;

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

        public Vec2 getCenterOfMass()
        {
            return new Vec2(_planet.x, _planet.y);
        }

        public void updateGui()
        {
        }

        public void renderGui()
        {
        }
    }
}