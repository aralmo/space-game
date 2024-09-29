public static class Test
{
    public static Simulation DefaultSimulation()
    {
        var simulation = new Simulation();
        var sun = CelestialBody
            .Create(Vector3D.Zero, 100000f)
            .WithModelVisuals(model: null, size: 60f, Color.Yellow)
            .WithInfo(name: "Aeon Prime");

        var planet = CelestialBody
            .Create(centralBody: sun, radius: 2000f, mass: 900f, eccentricity: 0.05f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.EarthLike, 1), size: 4f, Color.Blue)
            .WithInfo(name: "Aeon-1");

        simulation
            .AddCelestialBody(sun)
            .AddCelestialBody(planet)
            .AddCelestialBody(CelestialBody
                .Create(centralBody: planet, radius: 41f, mass: 90f, eccentricity: 0.12f, inclination: 1.67f, argumentOfPeriapsis: 1f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 2), size: 1f)
                .WithInfo(name: "Aeon-1A"))
            .AddCelestialBody(CelestialBody
                .Create(centralBody: planet, radius: 76f, mass: 65f, eccentricity: 0.23f, inclination: 1.73f, argumentOfPeriapsis: 2.3f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 3), size: 0.7f)
                .WithInfo(name: "Aeon-1B"))
            .AddCelestialBody(CelestialBody
                .Create(centralBody: planet, radius: 126f, mass: 44f, eccentricity: -0.17f, inclination: 0.23f, argumentOfPeriapsis: 4.2f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.IcePlanet, 4), size: 0.9f)
                .WithInfo(name: "Aeon-1C"));

        return simulation;
    }
}