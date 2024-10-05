public struct PredictedPath
{
    public Vector3D[] Points;
    public OrbitingObject? CurrentBodyOfInfluence;
    public (OrbitingObject Body, Vector3D BodyPosition, Vector3D ShipPosition)[] ClosestToBodyPositions;

}