namespace MissionTypes;

public class Tutorial : IMission
{
    public string Title { get => "The totorial mission."; }
    public bool Completed => hasOrbitedMoon && inPlanetOrbit;
    public MissionObjective[] Objectives => objectives;
    MissionObjective[] objectives;
    bool hasOrbitedMoon = false;
    bool inPlanetOrbit = false;
    public Tutorial()
    {
        objectives = [
            new MissionObjective()
            {
                Completed = () => hasOrbitedMoon,
                Title = "Enter Aeon1-A moon orbit."
            },
            new MissionObjective()
            {
                Completed = () => inPlanetOrbit && hasOrbitedMoon,
                Title = "Return to Aeon1 orbit."
            }
        ];
    }
    public void Update()
    {
        //check first objective
        var moon = Game.Simulation.OrbitingBodies.FirstOrDefault(b => (b is CelestialBody body) && body.Name == "Aeon-1A");
        if (Game.PlayerShip.DynamicSimulation.MajorInfluenceBody != null
            && Game.PlayerShip.DynamicSimulation.MajorInfluenceBody == moon && !hasOrbitedMoon)
        {
            var pos = Game.PlayerShip.DynamicSimulation.Position - Game.PlayerShip.DynamicSimulation.MajorInfluenceBody.GetPosition(Game.Simulation.Time);
            var vel = Game.PlayerShip.DynamicSimulation.Velocity - Game.PlayerShip.DynamicSimulation.MajorInfluenceBody.GetVelocity(Game.Simulation.Time);
            var orbit = Solve.KeplarOrbit(pos, vel, Game.PlayerShip.DynamicSimulation.MajorInfluenceBody.Mass, Game.Simulation.Time);
            if (orbit.Type == OrbitType.Elliptical && orbit.Eccentricity < .2f)
            {
                hasOrbitedMoon = true;
            }
        }

        //check second objective
        var second = Game.PlayerShip.DynamicSimulation.MajorInfluenceBody == moon.CentralBody;
        if (second && !inPlanetOrbit && hasOrbitedMoon)
        {
            var pos = Game.PlayerShip.DynamicSimulation.Position - Game.PlayerShip.DynamicSimulation.MajorInfluenceBody.GetPosition(Game.Simulation.Time);
            var vel = Game.PlayerShip.DynamicSimulation.Velocity - Game.PlayerShip.DynamicSimulation.MajorInfluenceBody.GetVelocity(Game.Simulation.Time);
            var orbit = Solve.KeplarOrbit(pos, vel, Game.PlayerShip.DynamicSimulation.MajorInfluenceBody.Mass, Game.Simulation.Time);
            if (orbit.Type == OrbitType.Elliptical && orbit.Eccentricity < .2f)
            {
                inPlanetOrbit = true;
                DialogController.Play("test-complete");
            }
        }
    }

    public void Draw2D()
    {
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();
        int padding = 10;
        int lineHeight = 35;
        int titleFontSize = 40;
        int objectiveFontSize = 30;
        int boxX = screenWidth - 20; // Temporary X position, will be adjusted based on text width
        int boxY = 20;
        int maxTextWidth = 0;

        // Calculate the width of the longest text line
        int titleWidth = MeasureText(Title, titleFontSize);
        maxTextWidth = titleWidth;
        foreach (var objective in Objectives)
        {
            int objectiveWidth = MeasureText(objective.Title, objectiveFontSize);
            if (objectiveWidth > maxTextWidth)
            {
                maxTextWidth = objectiveWidth;
            }
        }

        // Calculate box width based on the longest text line
        int boxWidth = maxTextWidth + padding * 2;
        boxX -= boxWidth; // Adjust box X position based on calculated width

        // Calculate box height based on the number of objectives
        int boxHeight = (Objectives.Length * lineHeight) + padding * 3 + titleFontSize;

        // Draw the box for the mission title and objectives
        DrawRectangle(boxX, boxY, boxWidth, boxHeight, Color.LightGray);

        // Draw the mission title
        DrawText(Title, boxX + padding, boxY + padding, titleFontSize, Color.Black);

        // Draw the objectives
        for (int i = 0; i < Objectives.Length; i++)
        {
            Color textColor = Objectives[i].Completed() ? Color.DarkGreen : Color.Gray;
            DrawText(Objectives[i].Title, boxX + padding, boxY + padding * 2 + titleFontSize + i * lineHeight, objectiveFontSize, textColor);
        }
    }

    public void MissionAdded()
    {
    }
}
public struct MissionObjective
{
    public Func<bool> Completed;
    public string Title;
}
public interface IMission
{
    string Title { get; }
    bool Completed { get; }
    MissionObjective[] Objectives { get; }
    void MissionAdded();
    void Update();
    void Draw2D();
}
