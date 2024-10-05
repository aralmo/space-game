public static class Test
{
    public static Simulation DefaultSimulation2()
    {
        var simulation = new Simulation();
        var sun = CelestialBody
            .CreateCelestial(Vector3D.Zero, 100000f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.YellowSun, 12), size: 60f, Color.Yellow)
            .WithInfo(name: "Aeon Prime");

        var planet = CelestialBody
            .CreateCelestial(centralBody: sun, radius: 2000f, mass: 900f, eccentricity: 0.05f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.EarthLike, 1), size: 4f, Color.Blue)
            .WithInfo(name: "Aeon-1");

        simulation
            .AddOrbitingBody(sun)
            .AddOrbitingBody(planet)
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 85f, mass: 90f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 2), size: 1f)
                .WithInfo(name: "Aeon-1A"))
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 220f, mass: 100f,.1f, 1.7f,Math.PI)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.IcePlanet, 3), size: 2f)
                .WithInfo(name: "Aeon-1B"));


        return simulation;
    }
    public static Simulation DefaultSimulation()
    {
        var simulation = new Simulation();
        var sun = CelestialBody
            .CreateCelestial(Vector3D.Zero, 100000f)
            .WithModelVisuals(model: null, size: 60f, Color.Yellow)
            .WithInfo(name: "Aeon Prime");

        var planet = CelestialBody
            .CreateCelestial(centralBody: sun, radius: 2000f, mass: 900f, eccentricity: 0.05f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.EarthLike, 1), size: 4f, Color.Blue)
            .WithInfo(name: "Aeon-1");

        simulation
            .AddOrbitingBody(sun)
            .AddOrbitingBody(planet)
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 190f, mass: 90f, eccentricity: 0.12f, inclination: 1.27f, argumentOfPeriapsis: 1f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 2), size: 1f)
                .WithInfo(name: "Aeon-1A"))
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 276f, mass: 65f, eccentricity: 0.13f, inclination: 1.73f, argumentOfPeriapsis: 2.3f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 3), size: 0.7f)
                .WithInfo(name: "Aeon-1B"))
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 105f, mass: 44f, eccentricity: -0.27f, inclination: 0.23f, argumentOfPeriapsis: 4.2f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.IcePlanet, 4), size: 0.9f)
                .WithInfo(name: "Aeon-1C"));

        return simulation;
    }
}