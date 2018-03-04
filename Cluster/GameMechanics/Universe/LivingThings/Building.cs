using System;
using System.Collections.Generic;
using Cluster.GameMechanics.Content;
using Cluster.GameMechanics.Universe.CelestialBodies;

namespace Cluster.GameMechanics.Universe.LivingThings
{
    class Building
    {
        public enum Status
        {
            NONE,
            UNDER_CONSTRUCTION,
            DESTROYED,
            DESTROYED_DURING_CONSTRUCTION
        };

        readonly Planet _planet;
        readonly int _location;
        public readonly Civilisation owner;

        public Blueprint blueprint;
        internal float health;
        internal float healthMax;
        internal Status status;
        internal float timer;

        internal List<Prototype> production;
        internal float productionTimer;


        public Building(Planet p, int pos, Blueprint blueprint, Civilisation owner)
        {
            _planet = p;
            _location = pos;
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
            return _planet;
        }

        public int getSpotId()
        {
            return _location;
        }

        public float getSpotRotation()
        {
            return (float) (((float) _location + 0.5f) / (float) _planet.size) * 2.0f * (float) Math.PI;
        }

        public float getHealthFraction()
        {
            return health / healthMax;
        }

        public List<Prototype> listOfPrototypes(Civilisation civ)
        {
            var list = new List<Prototype>();
            if (blueprint.specials == Blueprint.SpecialAbility.SHIPS)
            {
                foreach (Prototype pr in Prototype.data)
                {
                    if (pr.activation.researchedBy(civ) && blueprint.specialStrength >= pr.infra_level) list.Add(pr);
                }
            }

            return list;
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
                    Console.WriteLine(i.ToString());
                    break;
                }
            }
        }

        public Unit createUnit(Prototype type)
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


        // Updated das Gebäude
        internal void simulate(float dt, float efficiency)
        {
            //Status berücksichtigen
            switch (status)
            {
                case Status.UNDER_CONSTRUCTION:
                    if (health < healthMax)
                    {
                        health += dt * 1.25f;
                    }
                    else
                    {
                        health = healthMax;
                        status = Status.NONE;
                    }

                    break;

                case Status.DESTROYED_DURING_CONSTRUCTION:
                case Status.DESTROYED:
                    health = Math.Max(0.0f, health);
                    timer += dt;
                    if (timer > 100.0f)
                    {
                        _planet.infra[_location] = null;
                    }

                    return;
            }


            if (health <= 0.0f)
            {
                if (status == Status.UNDER_CONSTRUCTION) status = Status.DESTROYED_DURING_CONSTRUCTION;
                else status = Status.DESTROYED;
                timer = 0.0f;
                return;
            }

            if (status == Status.UNDER_CONSTRUCTION) return;


            //Wenn nicht zerstört oder noch im Bau, dann die Spezialeigenschaft des Gebäudes berücksichtigen:
            switch (blueprint.specials)
            {
                case Blueprint.SpecialAbility.SETTLEMENT:
                    owner.maxPopulationNew += (int) blueprint.specialStrength;
                    break;

                case Blueprint.SpecialAbility.MINING:
                    float factor = 0.005f;
                    if (_planet.terra[_location] == Planet.Terrain.MOUNTAIN) factor = 0.0075f;
                    else if (_planet.terra[_location] == Planet.Terrain.RESSOURCES) factor = 0.02f;

                    owner.ressources += dt * factor * blueprint.specialStrength *
                                        owner.getMultiplicator(Civilisation.BONUS_RESSOURCES);
                    break;

                case Blueprint.SpecialAbility.RESEARCH:
                    owner.science += dt * 0.0025f * blueprint.specialStrength *
                                     owner.getMultiplicator(Civilisation.BONUS_RESEARCH);
                    break;

                case Blueprint.SpecialAbility.SHIPS:
                    if (production.Count > 0)
                    {
                        float constructionSpeed = dt * 0.02f / (float) production[0].cost *
                                                  owner.getMultiplicator(Civilisation.BONUS_CONSTRUCTION_SPEED);
                        productionTimer += constructionSpeed;
                        if (productionTimer >= 1.0f)
                        {
                            productionTimer = 0.0f;
                            createUnit(production[0]);
                            production.RemoveAt(0);
                        }
                    }

                    break;
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
    }
}