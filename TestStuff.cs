public static class Test
{
    public static Simulation DefaultSimulation()
    {
        var simulation = new Simulation(new DateTime(2400,1,1));
        var sun = CelestialBody
            .CreateCelestial(Vector3D.Zero, 100000f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.YellowSun, 12), size: 60f, Color.Yellow)
            .WithInfo(name: "Aeon Prime");

        var planet = CelestialBody
            .CreateCelestial(centralBody: sun, radius: 2000f, mass: 900f,time: simulation.Time, eccentricity: 0.05f)
            .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.EarthLike, 1), size: 4f, Color.Blue)
            .WithInfo(name: "Aeon-1");

        simulation
            .AddOrbitingBody(sun)
            .AddOrbitingBody(planet)
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 85f, mass: 90f,time: simulation.Time, argumentOfPeriapsis: -1.5f)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.Moon, 2), size: 1f)
                .WithInfo(name: "Aeon-1A"))
            .AddOrbitingBody(CelestialBody
                .CreateCelestial(centralBody: planet, radius: 220f, mass: 100f,time: simulation.Time,.1f, 1.7f,Math.PI)
                .WithModelVisuals(model: PlanetGenerator.GeneratePlanet(PlanetSettings.IcePlanet, 3), size: 2f)
                .WithInfo(name: "Aeon-1B"));
        return simulation;
    }
}