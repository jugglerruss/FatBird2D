using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Vector2 = UnityEngine.Vector2;

public static class Bezier {

    public static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = EvaluateQuadratic(a, b, c, t);
        Vector2 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }
    public static List<double> GetRootsOfCubicEquations(double a, double b, double c)
    {
        var q = (Math.Pow(a, 2) - 3 * b) / 9;
        var r = (2 * Math.Pow(a, 3) - 9 * a * b + 27 * c) / 54;
 
        if (Math.Pow(r, 2) < Math.Pow(q, 3))
        {
            var t = Math.Acos(r/Math.Sqrt(Math.Pow(q, 3)))/3;
            var x1 = - 2 * Math.Sqrt(q) * Math.Cos(t) - a/3;
            var x2 = - 2 * Math.Sqrt(q) * Math.Cos(t + (2 * Math.PI/3)) - a/3;
            var x3 = - 2 * Math.Sqrt(q) * Math.Cos(t - (2 * Math.PI/3)) - a/3;
            return new List<double> {x1, x2, x3};
        }
        else
        {
            var A = -Math.Sign(r) * Math.Pow(Math.Abs(r) + Math.Sqrt(Math.Pow(r, 2) - Math.Pow(q, 3)), (1.0/3.0));
            var B = (A == 0) ? 0.0 : q/A;
 
            var x1 = (A + B) - a/3;
 
            if (A == B)
            {
                var x2 = -A - a/3;
                return new List<double> {x1, x2};
            }
            return new List<double> {x1};
        }
    }
}
