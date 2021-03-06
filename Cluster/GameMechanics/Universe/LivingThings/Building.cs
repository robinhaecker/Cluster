﻿using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Behaviour;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.CelestialBodies;
using Cluster.Mathematics;
using static Cluster.GameMechanics.Universe.LivingThings.Civilisation.TechBonus;

namespace Cluster.GameMechanics.Universe.LivingThings
{
    public class Building
    {
        public enum Status
        {
            NONE,
            UNDER_CONSTRUCTION,
            DESTROYED,
            DESTROYED_DURING_CONSTRUCTION
        }

        readonly Planet planet;
        readonly int location;
        public readonly Civilisation owner;

        public Blueprint blueprint;
        internal float health;
        internal float healthMax;
        internal Status status;
        internal float timer;

        internal readonly List<Prototype> production;
        internal float productionTimer;


        public Building(Planet p, int pos, Blueprint blueprint, Civilisation owner)
        {
            planet = p;
            location = pos;
            this.blueprint = blueprint;
            this.owner = owner;

            health = 1.0f;
            healthMax = blueprint.getHealth(owner);
            timer = 0.0f;
            status = Status.UNDER_CONSTRUCTION;

            production = new List<Prototype>();
            productionTimer = 0.0f;
        }

        public Planet getPlanet()
        {
            return planet;
        }

        public int getSpotId()
        {
            return location;
        }

        public Vec2 getPosition()
        {
            float phi = getSpotRotation();
            float planetRadius = planet.size * 20.0f;
            return new Vec2(planet.x + (float)Math.Cos(phi) * planetRadius,
                planet.y + (float)Math.Sin(phi) * planetRadius);
        }

        public float getSpotRotation()
        {
            return (location + 0.5f) / planet.size * 2.0f * (float) Math.PI;
        }

        public float getHealthFraction()
        {
            return health / healthMax;
        }

        public IEnumerable<Prototype> listOfPrototypes()
        {
            if (blueprint.specials != Blueprint.SpecialAbility.SHIPS) yield break;
            foreach (Prototype pr in Prototype.data)
            {
                if (pr.activation.researchedBy(owner) && blueprint.specialStrength >= pr.infraLevel)
                {
                    yield return pr;
                }
            }
        }

        public void produceUnit(Prototype type)
        {
            production.Add(type);
            owner.ressources -= type.cost;
        }

        public void abortUnit(Prototype type)
        {
            for (int i = production.Count - 1; i >= 0; i--)
            {
                if (production[i] == type)
                {
                    production.RemoveAt(i);
                    if (i == 0) productionTimer = 0.0f;
                    owner.ressources += type.cost;
                    break;
                }
            }
        }

        private Unit createUnit(Prototype type)
        {
            return new Unit(type, this);
        }

        public int getProductionCount(Prototype type)
        {
            int ct = 0;
            foreach (Prototype p in production)
            {
                if (p == type) ct++;
            }

            return ct;
        }

        internal void simulate(float dt, float efficiency)
        {
            if (updateStatus(dt)) return;
            useSpecialAbility(dt);
        }

        private bool updateStatus(float dt)
        {
            switch (status)
            {
                case Status.UNDER_CONSTRUCTION:
                    processIfUnderConstruction(dt);
                    break;

                case Status.DESTROYED_DURING_CONSTRUCTION:
                case Status.DESTROYED:
                    processIfDestroyed(dt);
                    return true;
            }

            if (health <= 0.0f)
            {
                processIfNoHealth();
                return true;
            }

            return status == Status.UNDER_CONSTRUCTION;
        }

        private void processIfNoHealth()
        {
            if (status == Status.UNDER_CONSTRUCTION) status = Status.DESTROYED_DURING_CONSTRUCTION;
            else status = Status.DESTROYED;
            timer = 0.0f;
        }

        private void processIfUnderConstruction(float dt)
        {
            if (health < healthMax)
            {
                health += dt * 1.25f;
            }
            else
            {
                health = healthMax;
                status = Status.NONE;
            }
        }

        private void processIfDestroyed(float dt)
        {
            health = Math.Max(0.0f, health);
            timer += dt;
            if (timer > 100.0f)
            {
                planet.infra[location] = null;
            }
        }

        private void useSpecialAbility(float dt)
        {
            switch (blueprint.specials)
            {
                case Blueprint.SpecialAbility.SETTLEMENT:
                    useAbilityForSettling();
                    break;

                case Blueprint.SpecialAbility.MINING:
                    useAbilityForMining(dt);
                    break;

                case Blueprint.SpecialAbility.RESEARCH:
                    useAbilityForResearch(dt);
                    break;

                case Blueprint.SpecialAbility.SHIPS:
                    if (production.Count > 0)
                    {
                        useAbilityForShipbuilding(dt);
                    }

                    break;

                case Blueprint.SpecialAbility.DEFEND:
                    useAbilityForDefense(dt);
                    break;
            }
        }

        private void useAbilityForDefense(float dt)
        {
            float constructionSpeed = dt * 0.003f * owner.getMultiplicator(CONSTRUCTION_SPEED);
            productionTimer += constructionSpeed;
            if (productionTimer >= 1.0f && Sector.areUnitsInRange(getPosition(), owner, 1000.0f))
            {
                productionTimer = 0.0f;
                createShot();
            }
        }

        private void createShot()
        {
            var shot = new Shot(this);
        }

        private void useAbilityForSettling()
        {
            owner.maxPopulationNew += (int) blueprint.specialStrength;
        }

        private void useAbilityForMining(float dt)
        {
            float factor = 0.005f;
            if (planet.terra[location] == Planet.Terrain.MOUNTAIN) factor = 0.0075f;
            else if (planet.terra[location] == Planet.Terrain.RESSOURCES) factor = 0.02f;

            owner.ressources += dt * factor * blueprint.specialStrength *
                                owner.getMultiplicator(RESSOURCES);
        }

        private void useAbilityForResearch(float dt)
        {
            owner.science += dt * 0.0025f * blueprint.specialStrength *
                             owner.getMultiplicator(RESEARCH);
        }

        private void useAbilityForShipbuilding(float dt)
        {
            float constructionSpeed = dt * 0.02f / production[0].cost *
                                      owner.getMultiplicator(CONSTRUCTION_SPEED);
            productionTimer += constructionSpeed;
            if (productionTimer >= 1.0f)
            {
                productionTimer = 0.0f;
                createUnit(production[0]);
                production.RemoveAt(0);
            }
        }

        public bool damage(float dmg)
        {
            if (status == Status.DESTROYED || status == Status.DESTROYED_DURING_CONSTRUCTION) return false;
            health -= dmg;
            if (health <= 0.0f)
            {
                health = 0.0f;
                if (status == Status.UNDER_CONSTRUCTION) status = Status.DESTROYED_DURING_CONSTRUCTION;
                else status = Status.DESTROYED;
                timer = 0.0f;
                return true;
            }

            return false;
        }

        public string getInfoText()
        {
            return blueprint.getName() + " (" + (int) health + "/" + (int) +healthMax + ")\n" +
                   "Besitzer: " + (owner?.name ?? "Niemand") + "\n" +
                   string.Join("\n", blueprint.description);
        }
    }
}