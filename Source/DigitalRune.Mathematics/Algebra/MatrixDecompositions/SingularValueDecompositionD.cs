// DigitalRune Engine - Copyright (C) DigitalRune GmbH
// This file is subject to the terms and conditions defined in
// file 'LICENSE.TXT', which is part of this source code package.

using System;


namespace DigitalRune.Mathematics.Algebra
{
  /// <summary>
  /// Computes the Singular Value Decomposition (SVD) of a matrix (double-precision).
  /// </summary>
  /// <remarks>
  /// <para>
  /// For an m x n matrix A with m ≥ n, the SVD computes the matrices U, S and V so that 
  /// <c>A = U * S * V<sup>T</sup></c>.
  /// </para>
  /// <para>
  /// U is an m x n orthogonal matrix. S is an n x n diagonal matrix. V is is a n x n orthogonal
  /// matrix.
  /// </para>
  /// <para>
  /// The diagonal elements of S are the <i>singular values</i>. The singular values are positive 
  /// or zero and ordered so that S[0, 0] ≥ S[1, 1] ≥ ...
  /// </para>
  /// <para>
  /// The singular value decomposition always exists.
  /// </para>
  /// <para>
  /// Applications: The matrix condition number and the effective numerical rank can be computed
  /// from this decomposition.
  /// </para>
  /// </remarks>
  public class SingularValueDecompositionD
  {
    //--------------------------------------------------------------
    #region Fields
    //--------------------------------------------------------------

    private readonly MatrixD _u;
    private readonly MatrixD _v;
    private readonly VectorD _s;    // Singular values.
    private MatrixD _matrixS;
    private readonly int _m;
    private readonly int _n;
    #endregion


    //--------------------------------------------------------------
    #region Properties
    //--------------------------------------------------------------

    /// <summary>
    /// Gets the condition number of A.
    /// </summary>
    /// <value>The condition number of A.</value>
    /// <remarks>
    /// The condition number is the ratio of the largest (in magnitude) of the singular values to
    /// the smallest singular value. The matrix is singular if the condition number is infinite and 
    /// the matrix is ill-conditioned if its condition number is too large, i.e. if the reciprocal
    /// approaches the machine's floating-point precision (less than 10^-6 for 
    /// <see langword="double"/>).
    /// </remarks>
    public double ConditionNumber
    {
      get { return _s[0] / _s[Math.Min(_m, _n) - 1]; }
    }


    /// <summary>
    /// Gets the diagonal matrix S with the singular values. (This property returns the internal 
    /// matrix, not a copy.)
    /// </summary>
    /// <value>The diagonal matrix S with the singular values.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public MatrixD S
    {
      get
      {
        if (_matrixS != null)
          return _matrixS;

        _matrixS = new MatrixD(_n, _n);
        for (int i = 0; i < _n; i++)
          _matrixS[i, i] = _s[i];

        return _matrixS;
      }
    }


    /// <summary>
    /// Gets the matrix U with the left singular vectors. (This property returns the internal
    /// matrix, not a copy.)
    /// </summary>
    /// <value>The matrix U with the left singular vectors.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public MatrixD U
    {
      get { return _u; }
    }


    /// <summary>
    /// Gets the matrix V with the right singular vectors. (This property returns the internal 
    /// matrix, not a copy.)
    /// </summary>
    /// <value>The matrix V with the right singular vectors.</value>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly")]
    public MatrixD V
    {
      get { return _v; }
    }


    /// <summary>
    /// Gets the two norm of A.
    /// </summary>
    /// <value>The two norm of A.</value>
    /// <remarks>
    /// The two norm is equal to the first (largest) singular value.
    /// </remarks>
    public double Norm2
    {
      get { return _s[0]; }
    }


    /// <summary>
    /// Gets the effective numerical rank of A.
    /// </summary>
    /// <value>The effective numerical rank of A.</value>
    /// <remarks>
    /// Near-zero singular values are considered as zero. The rank is the number of singular values 
    /// greater than 0.
    /// </remarks>
    public int NumericalRank
    {
      get
      {
        if (_numericalRank == -1)
        {
          double eps = Math.Pow(2, -52); // double mantissa is 52 bit.
          double tol = Math.Max(_m, _n) * _s[0] * eps;
          _numericalRank = 0;
          for (int i = 0; i < _s.NumberOfElements; i++)
            if (_s[i] > tol)
              _numericalRank++;
        }
        return _numericalRank;
      }
    }
    private int _numericalRank = -1;


    /// <summary>
    /// Gets the vector of singular values (the diagonal of S). 
    /// </summary>
    /// <value>The vector of singular values.</value>
    public VectorD SingularValues
    {
      get { return _s; }
    }
    #endregion


    //--------------------------------------------------------------
    #region Creation and Cleanup
    //--------------------------------------------------------------

    /// <summary>
    /// Creates the singular value decomposition of the given matrix.
    /// </summary>
    /// <param name="matrixA">
    /// The matrix A. (Can be rectangular. Number of rows ≥ number of columns.)
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="matrixA"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// The number of rows must be greater than or equal to the number of columns.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
    public SingularValueDecompositionD(MatrixD matrixA)
    {
      if (matrixA == null)
        throw new ArgumentNullException("matrixA");

      // Derived from LINPACK code.
      // Initialize.
      _m = matrixA.NumberOfRows;
      _n = matrixA.NumberOfColumns;
      MatrixD matrixAClone = matrixA.Clone();

      if (_m < _n)
        throw new ArgumentException("The number of rows must be greater than or equal to the number of columns.", "matrixA");

      int nu = Math.Min(_m, _n);
      _s = new VectorD(Math.Min(_m + 1, _n));
      _u = new MatrixD(_m, nu);     //Jama getU() returns new Matrix(U,_m,Math.min(_m+1,_n)) ?!
      _v = new MatrixD(_n, _n);
      double[] e = new double[_n];
      double[] work = new double[_m];

      // Abort if A contains NaN values.
      // If we continue with NaN values, we run into an infinite loop.
      for (int i = 0; i < _m; i++)
      {
        for (int j = 0; j < _n; j++)
        {
          if (Numeric.IsNaN(matrixA[i, j]))
          {
            _u.Set(double.NaN);
            _v.Set(double.NaN);
            _s.Set(double.NaN);
            return;
          }
        }
      }

      // By default, we calculate U and V. To calculate only U or V we can set one of the following
      // two constants to false. (This optimization is not yet tested.)
      const bool wantu = true;
      const bool wantv = true;

      // Reduce A to bidiagonal form, storing the diagonal elements
      // in s and the super-diagonal elements in e.

      int nct = Math.Min(_m - 1, _n);
      int nrt = Math.Max(0, Math.Min(_n - 2, _m));
      for (int k = 0; k < Math.Max(nct, nrt); k++)
      {
        if (k < nct)
        {
          // Compute the transformation for the k-th column and
          // place the k-th diagonal in s[k].
          // Compute 2-norm of k-th column without under/overflow.
          _s[k] = 0;
          for (int i = k; i < _m; i++)
            _s[k] = MathHelper.Hypotenuse(_s[k], matrixAClone[i, k]);

          if (_s[k] != 0)
          {
            if (matrixAClone[k, k] < 0)
              _s[k] = -_s[k];

            for (int i = k; i < _m; i++)
              matrixAClone[i, k] /= _s[k];

            matrixAClone[k, k] += 1;
          }

          _s[k] = -_s[k];
        }
        for (int j = k + 1; j < _n; j++)
        {
          if ((k < nct) && (_s[k] != 0))
          {
            // Apply the transformation.
            double t = 0;
            for (int i = k; i < _m; i++)
              t += matrixAClone[i, k] * matrixAClone[i, j];

            t = -t / matrixAClone[k, k];
            for (int i = k; i < _m; i++)
              matrixAClone[i, j] += t * matrixAClone[i, k];
          }

          // Place the k-th row of A into e for the
          // subsequent calculation of the row transformation.

          e[j] = matrixAClone[k, j];
        }

        if (wantu & (k < nct))
        {
          // Place the transformation in U for subsequent back
          // multiplication.
          for (int i = k; i < _m; i++)
            _u[i, k] = matrixAClone[i, k];
        }

        if (k < nrt)
        {
          // Compute the k-th row transformation and place the
          // k-th super-diagonal in e[k].
          // Compute 2-norm without under/overflow.
          e[k] = 0;
          for (int i = k + 1; i < _n; i++)
            e[k] = MathHelper.Hypotenuse(e[k], e[i]);

          if (e[k] != 0)
          {
            if (e[k + 1] < 0)
              e[k] = -e[k];

            for (int i = k + 1; i < _n; i++)
              e[i] /= e[k];

            e[k + 1] += 1;
          }

          e[k] = -e[k];
          if ((k + 1 < _m) && (e[k] != 0))
          {
            // Apply the transformation.

            for (int i = k + 1; i < _m; i++)
              work[i] = 0;

            for (int j = k + 1; j < _n; j++)
              for (int i = k + 1; i < _m; i++)
                work[i] += e[j] * matrixAClone[i, j];

            for (int j = k + 1; j < _n; j++)
            {
              double t = -e[j] / e[k + 1];
              for (int i = k + 1; i < _m; i++)
                matrixAClone[i, j] += t * work[i];
            }
          }

          if (wantv)
          {
            // Place the transformation in V for subsequent
            // back multiplication.
            for (int i = k + 1; i < _n; i++)
              _v[i, k] = e[i];
          }
        }
      }

      // Set up the final bidiagonal matrix or order p.

      int p = Math.Min(_n, _m + 1);
      if (nct < _n)
        _s[nct] = matrixAClone[nct, nct];

      if (_m < p)
        _s[p - 1] = 0;

      if (nrt + 1 < p)
        e[nrt] = matrixAClone[nrt, p - 1];

      e[p - 1] = 0;

      // If required, generate U.

      if (wantu)
      {
        for (int j = nct; j < nu; j++)
        {
          for (int i = 0; i < _m; i++)
            _u[i, j] = 0;

          _u[j, j] = 1;
        }

        for (int k = nct - 1; k >= 0; k--)
        {
          if (_s[k] != 0)
          {
            for (int j = k + 1; j < nu; j++)
            {
              double t = 0;
              for (int i = k; i < _m; i++)
                t += _u[i, k] * _u[i, j];

              t = -t / _u[k, k];
              for (int i = k; i < _m; i++)
                _u[i, j] += t * _u[i, k];

            }
            for (int i = k; i < _m; i++)
              _u[i, k] = -_u[i, k];

            _u[k, k] = 1 + _u[k, k];
            for (int i = 0; i < k - 1; i++)
              _u[i, k] = 0;
          }
          else
          {
            for (int i = 0; i < _m; i++)
              _u[i, k] = 0;

            _u[k, k] = 1;
          }
        }
      }

      // If required, generate V.
      if (wantv)
      {
        for (int k = _n - 1; k >= 0; k--)
        {
          if ((k < nrt) & (e[k] != 0.0))
          {
            for (int j = k + 1; j < nu; j++)
            {
              double t = 0;
              for (int i = k + 1; i < _n; i++)
                t += _v[i, k] * _v[i, j];

              t = -t / _v[k + 1, k];
              for (int i = k + 1; i < _n; i++)
                _v[i, j] += t * _v[i, k];
            }
          }

          for (int i = 0; i < _n; i++)
            _v[i, k] = 0;

          _v[k, k] = 1;
        }
      }

      // Main iteration loop for the singular values.

      int pp = p - 1;
      int iter = 0;
      double eps = Math.Pow(2, -52);
      double tiny = Math.Pow(2, -966);   // Original: 2^-966 for double
      while (p > 0)
      {
        int k, kase;

        // Here is where a test for too many iterations would go.

        // This section of the program inspects for
        // negligible elements in the s and e arrays. On
        // completion the variables kase and k are set as follows.

        // kase = 1     if s(p) and e[k-1] are negligible and k<p
        // kase = 2     if s(k) is negligible and k<p
        // kase = 3     if e[k-1] is negligible, k<p, and
        //              s(k), ..., s(p) are not negligible (qr step).
        // kase = 4     if e(p-1) is negligible (convergence).

        for (k = p - 2; k >= -1; k--)
        {
          if (k == -1)
            break;

          if (Math.Abs(e[k]) <= tiny + eps * (Math.Abs(_s[k]) + Math.Abs(_s[k + 1])))
          {
            e[k] = 0;
            break;
          }
        }

        if (k == p - 2)
        {
          kase = 4;
        }
        else
        {
          int ks;
          for (ks = p - 1; ks >= k; ks--)
          {
            if (ks == k)
              break;

            double t = (ks != p ? Math.Abs(e[ks]) : 0) + (ks != k + 1 ? Math.Abs(e[ks - 1]) : 0);
            if (Math.Abs(_s[ks]) <= tiny + eps * t)
            {
              _s[ks] = 0;
              break;
            }
          }
          if (ks == k)
          {
            kase = 3;
          }
          else if (ks == p - 1)
          {
            kase = 1;
          }
          else
          {
            kase = 2;
            k = ks;
          }
        }

        k++;

        // Perform the task indicated by kase.

        switch (kase)
        {
          // Deflate negligible s(p).
          case 1:
            {
              double f = e[p - 2];
              e[p - 2] = 0;
              for (int j = p - 2; j >= k; j--)
              {
                double t = MathHelper.Hypotenuse(_s[j], f);
                double cs = _s[j] / t;
                double sn = f / t;
                _s[j] = t;
                if (j != k)
                {
                  f = -sn * e[j - 1];
                  e[j - 1] = cs * e[j - 1];
                }

                if (wantv)
                {
                  for (int i = 0; i < _n; i++)
                  {
                    t = cs * _v[i, j] + sn * _v[i, p - 1];
                    _v[i, p - 1] = -sn * _v[i, j] + cs * _v[i, p - 1];
                    _v[i, j] = t;
                  }
                }
              }
            }
            break;

          // Split at negligible s(k).
          case 2:
            {
              double f = e[k - 1];
              e[k - 1] = 0;
              for (int j = k; j < p; j++)
              {
                double t = MathHelper.Hypotenuse(_s[j], f);
                double cs = _s[j] / t;
                double sn = f / t;
                _s[j] = t;
                f = -sn * e[j];
                e[j] = cs * e[j];
                if (wantu)
                {
                  for (int i = 0; i < _m; i++)
                  {
                    t = cs * _u[i, j] + sn * _u[i, k - 1];
                    _u[i, k - 1] = -sn * _u[i, j] + cs * _u[i, k - 1];
                    _u[i, j] = t;
                  }
                }
              }
            }
            break;

          // Perform one qr step.
          case 3:
            {
              // Calculate the shift.

              double scale = Math.Max(Math.Max(Math.Max(Math.Max(
                      Math.Abs(_s[p - 1]), Math.Abs(_s[p - 2])), Math.Abs(e[p - 2])),
                      Math.Abs(_s[k])), Math.Abs(e[k]));
              double sp = _s[p - 1] / scale;
              double spm1 = _s[p - 2] / scale;
              double epm1 = e[p - 2] / scale;
              double sk = _s[k] / scale;
              double ek = e[k] / scale;
              double b = ((spm1 + sp) * (spm1 - sp) + epm1 * epm1) / 2;
              double c = (sp * epm1) * (sp * epm1);
              double shift = 0;
              if ((b != 0.0) | (c != 0.0))
              {
                shift = Math.Sqrt(b * b + c);
                if (b < 0.0)
                  shift = -shift;

                shift = c / (b + shift);
              }
              double f = (sk + sp) * (sk - sp) + shift;
              double g = sk * ek;

              // Chase zeros.

              for (int j = k; j < p - 1; j++)
              {
                double t = MathHelper.Hypotenuse(f, g);
                double cs = f / t;
                double sn = g / t;
                if (j != k)
                  e[j - 1] = t;

                f = cs * _s[j] + sn * e[j];
                e[j] = cs * e[j] - sn * _s[j];
                g = sn * _s[j + 1];
                _s[j + 1] = cs * _s[j + 1];
                if (wantv)
                {
                  for (int i = 0; i < _n; i++)
                  {
                    t = cs * _v[i, j] + sn * _v[i, j + 1];
                    _v[i, j + 1] = -sn * _v[i, j] + cs * _v[i, j + 1];
                    _v[i, j] = t;
                  }
                }

                t = MathHelper.Hypotenuse(f, g);
                cs = f / t;
                sn = g / t;
                _s[j] = t;
                f = cs * e[j] + sn * _s[j + 1];
                _s[j + 1] = -sn * e[j] + cs * _s[j + 1];
                g = sn * e[j + 1];
                e[j + 1] = cs * e[j + 1];
                if (wantu && (j < _m - 1))
                {
                  for (int i = 0; i < _m; i++)
                  {
                    t = cs * _u[i, j] + sn * _u[i, j + 1];
                    _u[i, j + 1] = -sn * _u[i, j] + cs * _u[i, j + 1];
                    _u[i, j] = t;
                  }
                }
              }

              e[p - 2] = f;
              iter = iter + 1;
            }
            break;

          // Convergence.

          case 4:
            {
              // Make the singular values positive.

              if (_s[k] <= 0.0)
              {
                _s[k] = (_s[k] < 0.0 ? -_s[k] : 0);
                if (wantv)
                {
                  for (int i = 0; i <= pp; i++)
                    _v[i, k] = -_v[i, k];
                }
              }

              // Order the singular values.

              while (k < pp)
              {
                if (_s[k] >= _s[k + 1])
                  break;

                double t = _s[k];
                _s[k] = _s[k + 1];
                _s[k + 1] = t;
                if (wantv && (k < _n - 1))
                {
                  for (int i = 0; i < _n; i++)
                  {
                    t = _v[i, k + 1];
                    _v[i, k + 1] = _v[i, k];
                    _v[i, k] = t;
                  }
                }
                if (wantu && (k < _m - 1))
                {
                  for (int i = 0; i < _m; i++)
                  {
                    t = _u[i, k + 1];
                    _u[i, k + 1] = _u[i, k];
                    _u[i, k] = t;
                  }
                }

                k++;
              }

              iter = 0;
              p--;
            }
            break;
        }
      }
    }
    #endregion


    //--------------------------------------------------------------
    #region Methods
    //--------------------------------------------------------------
    #endregion
  }
}
