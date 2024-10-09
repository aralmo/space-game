public struct PredictedPath
{
    public Vector3D[] Positions;
    public Vector3D[] Velocities;
    public DateTime[] Times;
    public OrbitingObject? CurrentBodyOfInfluence;
    public (OrbitingObject Body, Vector3D BodyPosition, Vector3D ShipPosition)[] ClosestToBodyPositions;

}