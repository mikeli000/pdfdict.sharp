using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFDict.SDK.Sharp.Core
{
    public class Matrix
    {
        public const int SIZE = 9;
        private float[] _single;

        public Matrix()
        {
            _single = new float[SIZE];
        }

        public Matrix(float[] single)
        {
            if (single.Length != SIZE)
            {
                throw new ArgumentException("Invalid matrix size");
            }
            this._single = single;
        }

        public Matrix(float a, float b, float c, float d, float e, float f)
        {
            _single = new float[SIZE];
            _single[0] = a;
            _single[1] = b;
            _single[3] = c;
            _single[4] = d;
            _single[6] = e;
            _single[7] = f;
            _single[8] = 1;
        }
        public object Clone()
        {
            return new Matrix(_single.ToArray());
        }

        public static Matrix GetRotateInstance(double theta, float x, float y)
        {
            float cos = (float)Math.Cos(theta);
            float sin = (float)Math.Sin(theta);
            return new Matrix(cos, sin, -sin, cos, x - cos * x + sin * y, y - sin * x - cos * y);
        }

        public static Matrix GetScaleInstance(float sx, float sy)
        {
            return new Matrix(sx, 0, 0, sy, 0, 0);
        }

        public static Matrix GetTranslateInstance(float tx, float ty)
        {
            return new Matrix(1, 0, 0, 1, tx, ty);
        }

        public void Translate(float tx, float ty)
        {
            Matrix translationMatrix = GetTranslateInstance(tx, ty);
            Multiply(translationMatrix);
        }

        public void Scale(float sx, float sy)
        {
            Matrix scaleMatrix = GetScaleInstance(sx, sy);
            Multiply(scaleMatrix);
        }

        public void Rotate(double theta, float x, float y)
        {
            Matrix rotateMatrix = GetRotateInstance(theta, x, y);
            Multiply(rotateMatrix);
        }

        public void Multiply(Matrix other)
        {
            float[] result = new float[SIZE];
            result[0] = _single[0] * other._single[0] + _single[1] * other._single[3];
            result[1] = _single[0] * other._single[1] + _single[1] * other._single[4];
            result[3] = _single[3] * other._single[0] + _single[4] * other._single[3];
            result[4] = _single[3] * other._single[1] + _single[4] * other._single[4];
            result[6] = _single[6] * other._single[0] + _single[7] * other._single[3] + other._single[6];
            result[7] = _single[6] * other._single[1] + _single[7] * other._single[4] + other._single[7];
            result[8] = 1;
            _single = result;
        }

        public float[] GetValues()
        {
            return _single.ToArray();
        }

        public float GetScalingFactorX()
        {
            if (Math.Abs(_single[1]) > 0.00001f)
            {
                return (float)Math.Sqrt(Math.Pow(_single[0], 2) + Math.Pow(_single[1], 2));
            }
            return _single[0];
        }

        public float GetScalingFactorY()
        {
            if (Math.Abs(_single[3]) > 0.00001f)
            {
                return (float)Math.Sqrt(Math.Pow(_single[3], 2) + Math.Pow(_single[4], 2));
            }
            return _single[4];
        }

        public float GetScaleX()
        {
            return _single[0];
        }

        public float GetShearY()
        {
            return _single[1];
        }

        public float GetShearX()
        {
            return _single[3];
        }

        public float GetScaleY()
        {
            return _single[4];
        }

        public float GetTranslateX()
        {
            return _single[6];
        }

        public float GetTranslateY()
        {
            return _single[7];
        }

        public override string ToString()
        {
            return $"[{_single[0]}, {_single[1]}, 0, {_single[3]}, {_single[4]}, 0, {_single[6]}, {_single[7]}, 1]";
        }
    }
}
