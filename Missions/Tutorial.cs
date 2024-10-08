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
            && Game.PlayerShip.DynamicSimulation.MajorInfluenceBody == moon)
        {
            hasOrbitedMoon = true;
        }

        //check second objective
        inPlanetOrbit = Game.PlayerShip.DynamicSimulation.MajorInfluenceBody == moon.CentralBody;
    }

    public void Draw2D()
    {
        int screenWidth = GetScreenWidth();
        int screenHeight = GetScreenHeight();
        int padding = 10;
        int lineHeight = 20;
        int titleFontSize = 20;
        int objectiveFontSize = 15;
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
            Color textColor = Objectives[i].Completed() ? Color.Green : Color.Gray;
            DrawText(Objectives[i].Title, boxX + padding, boxY + padding * 2 + titleFontSize + i * lineHeight, objectiveFontSize, textColor);
        }
    }

    public void MissionAdded()
    {
        throw new NotImplementedException();
    }
}
public struct MissionObjective
{
    public Func<bool> Completed;
    public string Title;
}
public interface IMission
{
    string Title {get;}
    bool Completed {get;}
    MissionObjective[] Objectives {get;}
    void MissionAdded();
    void Update();
    void Draw2D();
}
