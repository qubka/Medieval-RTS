public enum Direction
{
    Forward,
    ForwardLeft,
    ForwardRight,
    Left,
    Right,
    Backward,
    BackwardLeft,
    BackwardRight
}

public static class DirectionUtils
{
    public static Direction AngleToDirection(float angle)
    {
        var deg = (angle + 180f) % 360f - 180f;

        if (deg <= 22.5f && deg > -22.5f) {
            return Direction.Forward;
        }

        if (deg <= -22.5f && deg > -67.5f) {
            return Direction.ForwardLeft;
        }

        if (deg > 22.5f && deg <= 67.5f) {
            return Direction.ForwardRight;
        }

        if (deg <= -67.5f && deg > -112.5f) {
            return Direction.Left;
        }

        if (deg > 67.5f && deg <= 112.5f) {
            return Direction.Right;
        }
        
        /*if (deg <= -112.5f && deg > -157.5f) {
            return Direction.BackwardLeft;
        }

        if (deg > 112.5f && deg <= 157.5f) {
            return Direction.BackwardRight;
        }*/

        return Direction.Backward;
    }
}