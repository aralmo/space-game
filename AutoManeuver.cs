// Simulation/ManeuverFinder.cs
public class ManeuverFinder
{
    private Thread? searchThread;
    private bool cancelSearch = false;
    private Vector3D startPosition;
    private Vector3D startVelocity;
    private DateTime startTime;
    private CelestialBody target;

    public ManeuverFinder()
    {
    }

    public void StartSearch(Vector3D startPosition, Vector3D startVelocity, DateTime startTime, CelestialBody target)
    {
        this.startPosition = startPosition;
        this.startVelocity = startVelocity;
        this.startTime = startTime;
        this.target = target;
        cancelSearch = false;

        searchThread = new Thread(SearchForManeuver)
        {
            IsBackground = true,
            Priority = ThreadPriority.BelowNormal
        };
        searchThread.Start();
    }

    private void SearchForManeuver()
    {
        while (!cancelSearch)
        {
            // Your logic to simulate and check for a suitable maneuver goes here.
            // This is a placeholder for the actual simulation and evaluation logic.
            // You might simulate forward from the starting conditions and evaluate
            // whether the ship would reach the target under those conditions.

            // Example pseudo-logic:
            // 1. Simulate the ship's trajectory from startPosition using startVelocity.
            // 2. Evaluate if the trajectory intersects with the target's position.
            // 3. If a suitable maneuver is found, break the loop.

            // Simulate...
            // Evaluate...

            // Placeholder for a condition to stop the search if a suitable maneuver is found.
            bool foundSuitableManeuver = false; // This should be replaced with actual evaluation logic.
            if (foundSuitableManeuver)
            {
                // Possibly invoke a callback or set a property with the found maneuver details.
                break;
            }

            // Sleep a bit to prevent this loop from consuming too much CPU.
            Thread.Sleep(100);
        }
    }

    public void StopSearch()
    {
        cancelSearch = true;
    }
}