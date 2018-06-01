using System;

namespace Tic_Tac_Toe_AI
{
    public partial class DoubleMatrix
    {
        private double[,] Matrix { get; set; }

        public int Rows => Matrix.GetLength(0);
        public int Cols => Matrix.GetLength(1);
        
        public DoubleMatrix(int rows, int cols, MatrixInitMode initMode)
        {
            Matrix = new double[rows, cols];
            if (initMode == MatrixInitMode.Null) return;
            else if (initMode == MatrixInitMode.RanNorm)
            {
                Random random = new Random();
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        Matrix[i, j] = random.NextDouble();
                    }
                }
                return;
            }
            else if (initMode == MatrixInitMode.Random)
            {
                Random random = new Random();
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Cols; j++)
                    {
                        Matrix[i, j] = random.NextDouble() * random.Next(int.MinValue, int.MaxValue);
                    }
                }
                return;
            }
        }

        public void Activate(ActivationFunction activation)
        {
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    Matrix[i, j] = activation(Matrix[i, j]);
                }
            }
        }

        public double this[int row, int col]
        {
            get
            {
                if (row >= Matrix.GetLength(0)) throw new ArgumentException("Row is greater than row count", "row");
                if (col >= Matrix.GetLength(1)) throw new ArgumentException("Col is greater than col count", "col");
                return Matrix[row, col];
            }
            set
            {
                if (row >= Matrix.GetLength(0)) throw new ArgumentException("Row is greater than row count", "row");
                if (col >= Matrix.GetLength(1)) throw new ArgumentException("Col is greater than col count", "col");
                Matrix[row, col] = value;
            }
        }

        public static DoubleMatrix operator +(DoubleMatrix a, DoubleMatrix b)
        {
            if (a.Cols != b.Cols || a.Rows != b.Rows) throw new ArgumentException("Matrices don't have the same dimensions");
            DoubleMatrix mx = new DoubleMatrix(a.Rows, a.Cols, MatrixInitMode.Null);
            for (int i = 0; i < mx.Rows; i++)
            {
                for (int j = 0; j < mx.Cols; j++)
                {
                    mx[i, j] = a[i, j] + b[i, j];
                }
            }
            return mx;
        }

        public static DoubleMatrix operator -(DoubleMatrix mx)
        {
            for (int i = 0; i < mx.Rows; i++)
            {
                for (int j = 0; j < mx.Cols; j++)
                {
                    mx[i, j] *= -1;
                }
            }
            return mx;
        }

        public static DoubleMatrix operator -(DoubleMatrix a, DoubleMatrix b) => a + -b;

        public static DoubleMatrix operator !(DoubleMatrix mx)
        {
            DoubleMatrix t = new DoubleMatrix(mx.Cols, mx.Rows, MatrixInitMode.Null);
            for (int i = 0; i < mx.Rows; i++)
            {
                for (int j = 0; j < mx.Cols; j++)
                {
                    t[j, i] = mx[i, j];
                }
            }
            return t;
        }

        #region Matrix Number multiplication overloads
        public static DoubleMatrix operator *(sbyte c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(byte c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(short c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(ushort c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(int c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(uint c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(long c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(ulong c, DoubleMatrix mx) => (double)c * mx; 
        public static DoubleMatrix operator *(float c, DoubleMatrix mx) => (double)c * mx;
        public static DoubleMatrix operator *(decimal c, DoubleMatrix mx) => (double)c * mx;

        public static DoubleMatrix operator *(DoubleMatrix mx, sbyte c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, byte c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, short c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, ushort c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, int c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, uint c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, long c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, ulong c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, float c) => (double)c * mx;
        public static DoubleMatrix operator *(DoubleMatrix mx, decimal c) => (double)c * mx;

        public static DoubleMatrix operator *(DoubleMatrix mx, double c) => c * mx;
        #endregion

        public static DoubleMatrix operator *(double c, DoubleMatrix mx)
        {
            for (int i = 0; i < mx.Rows; i++)
            {
                for (int j = 0; j < mx.Cols; j++)
                {
                    mx[i, j] *= c;
                }
            }
            return mx;
        }

        public static DoubleMatrix operator *(DoubleMatrix a, DoubleMatrix b)
        {
            if (a.Cols != b.Rows) throw new ArgumentException("Matrix a's column count must match matrix b's row count");
            DoubleMatrix mx = new DoubleMatrix(a.Rows, b.Cols, MatrixInitMode.Null);
            for (int i = 0; i < mx.Rows; i++)
            {
                for (int j = 0; j < mx.Cols; j++)
                {
                    mx[i, j] = 0;
                    for (int k = 0; k < a.Cols; k++)
                    {
                        mx[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
            return mx;
        }

        public override string ToString()
        {
            int[] columnWidths = new int[Cols];
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    if (this[i, j].ToString().Length > columnWidths[j]) columnWidths[j] = this[i, j].ToString().Length;
                }
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                {
                    sb.AppendFormat(" {0}{1} ", this[i, j], new string(' ', columnWidths[j] - this[i, j].ToString().Length));
                }
                sb.Append(System.Environment.NewLine);
            }
            return sb.ToString();
        }

        public static DoubleMatrix Auto(int rows, int cols)
        {
            DoubleMatrix mx = new DoubleMatrix(rows, cols, MatrixInitMode.Null);
            for (int i = 0; i < mx.Rows; i++)
            {
                for (int j = 0; j < mx.Cols; j++)
                {
                    mx[i, j] = i * cols + j;
                }
            }
            return mx;
        }
    }

    public enum MatrixInitMode { Null, RanNorm, Random }
}
