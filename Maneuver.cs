public static class Maneuver
{
    public static (DateTime maneuverTime, Vector3D deltaV) CalculateTransferManeuver(CelestialBody body1, CelestialBody body2, double deltaVLimit)
    {
        DateTime bestTime = DateTime.UtcNow;
        Vector3D bestDeltaV = new Vector3D();
        double shortestTime = double.MaxValue;

        for (int days = 0; days < 365; days++)
        {
            DateTime currentTime = DateTime.UtcNow.AddDays(days);
            Vector3D pos1 = body1.GetPosition(currentTime);
            Vector3D pos2 = body2.GetPosition(currentTime);

            Vector3D relativePosition = pos2 - pos1;
            Vector3D relativeVelocity = body2.OrbitParameters.Value.VelocityAtTime(currentTime) - body1.OrbitParameters.Value.VelocityAtTime(currentTime);

            double distance = relativePosition.Length();
            double requiredDeltaV = relativeVelocity.Length();

            if (requiredDeltaV <= deltaVLimit && distance < shortestTime)
            {
                shortestTime = distance;
                bestTime = currentTime;
                bestDeltaV = relativeVelocity;
            }
        }

        return (bestTime, bestDeltaV);
    }
}