namespace PathApproximation
{
    /**
     * Represents a point in 2D cartesian coordinate system (X-axis points right, Y-axis points up).
     * Not suitable for denoting the position on Earth (sphere) due to longitude difference creates a shorter distance the closer to the poles.
     */
    public record Point(double X, double Y);

    public record IndexedPoint(double X, double Y, int Index);
    public record Vector(IndexedPoint From, IndexedPoint To);
}
