using Finite_State_Machine_Designer.Models.FSM;

namespace Finite_State_Machine_Designer.Client.Helper
{
    /// <summary>
    /// Class with collection of methods to assist in matrix operations.
    /// </summary>
    public static class Matrix
    {
        /// <summary>
		/// Finds the determinant of 3x3 matrix
		/// </summary>
		/// <param name="a">1 row, column 1 cell</param>
		/// <param name="b">1 row, column 2 cell</param>
		/// <param name="c">1 row, column 3 cell</param>
		/// <param name="d">2 row, column 1 cell</param>
		/// <param name="e">2 row, column 2 cell</param>
		/// <param name="f">2 row, column 3 cell</param>
		/// <param name="g">3 row, column 1 cell</param>
		/// <param name="h">3 row, column 2 cell</param>
		/// <param name="i">3 row, column 3 cell</param>
		/// <returns>Determinant of 3x3 matrix</returns>
		public static double Determinant(
            double a, double b, double c,
            double d, double e, double f,
            double g, double h, double i) => a * e * i + b * f * g + c * d * h - a * f * h - b * d * i - c * e * g;

        /// <summary>
        /// Generates 3 circles from 3 points using
        /// <a href="https://en.wikipedia.org/wiki/Laplace_expansion">Laplace Expansion</a>
        /// to find the curve of the transition.
        /// </summary>
        /// <param name="coord1">Coordinate 1</param>
        /// <param name="coord2">Coordinate 2</param>
        /// <param name="coord3">Coordinate 3</param>
        /// <returns>First and second numbers are x and y values respectively
        /// and the last number is the radius of the circle.
        /// </returns>
        public static Tuple<double, double, double> CircleCentreRadiiFrom3Points(
            CanvasCoordinate coord1, CanvasCoordinate coord2, CanvasCoordinate coord3)
        {
            double a = Determinant(
                coord1.X, coord1.Y, 1,
                coord2.X, coord2.Y, 1,
                coord3.X, coord3.Y, 1);

            double mouseLengthSquare = (coord3.X * coord3.X) + (coord3.Y * coord3.Y);
            double fromLengthSquare = (coord1.X * coord1.X) + (coord1.Y * coord1.Y);
            double toLengthSquare = (coord2.X * coord2.X) + (coord2.Y * coord2.Y);
            double bx = Determinant(
                fromLengthSquare, coord1.Y, 1,
                toLengthSquare, coord2.Y, 1,
                mouseLengthSquare, coord3.Y, 1);

            double by = Determinant(
                fromLengthSquare, coord1.X, 1,
                toLengthSquare, coord2.X, 1,
                mouseLengthSquare, coord3.X, 1);

            double c = Determinant(
                fromLengthSquare, coord1.X, coord1.Y,
                toLengthSquare, coord2.X, coord2.Y,
                mouseLengthSquare, coord3.X, coord3.Y);

            double circleX = bx / (2 * a);
            double circleY = -(by / (2 * a));
            double circleRadius = Math.Sqrt((circleX * circleX) + (circleY * circleY) + (c / a));

            return new Tuple<double, double, double>(circleX, circleY, circleRadius);
        }
    }
}
