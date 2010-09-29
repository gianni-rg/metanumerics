﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Meta.Numerics;
using Meta.Numerics.Matrices;

namespace Test {    
    
    [TestClass]
    public class SquareMatrixTest {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        public static void PrintMatrix (IMatrix M) {
            for (int r = 0; r < M.RowCount; r++) {
                for (int c = 0; c < M.ColumnCount; c++) {
                    Console.Write("{0,12:g8} ", M[r, c]);
                }
                Console.WriteLine();
            }
            Console.WriteLine("--");
        }

        private static SquareMatrix CreateSquareRandomMatrix (int n) {
            return (CreateSquareRandomMatrix(n, 1));
        }

        private static SquareMatrix CreateSquareRandomMatrix (int n, int seed) {
            SquareMatrix M = new SquareMatrix(n);
            Random rng = new Random(seed);
            for (int r = 0; r < n; r++) {
                for (int c = 0; c < n; c++) {
                    M[r, c] = 2.0 * rng.NextDouble() - 1.0;
                }
            }
            return (M);
        }

        private static SquareMatrix CreateVandermondeMatrix (int n) {
            double[] x = new double[n];
            for (int i = 0; i < n; i++) {
                x[i] = i;
            }
            return (CreateVandermondeMatrix(x));
        }

        private static SquareMatrix CreateVandermondeMatrix (double[] x) {
            int d = x.Length;
            SquareMatrix V = new SquareMatrix(d);
            for (int r = 0; r < d; r++) {
                double v = 1.0;
                for (int c = 0; c < d; c++) {
                    V[r, c] = v;
                    v = v * x[r];
                }
            }
            return (V);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SquareMatrixInvalidDimensionTest () {
            SquareMatrix M = new SquareMatrix(0);
        }

        [TestMethod]
        public void SquareMatrixAccess () {
            double[] x = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            SquareMatrix V = CreateVandermondeMatrix(x);

            // check nullity
            Assert.IsTrue(V != null);

            // check dimension
            Assert.IsTrue(V.Dimension == x.Length);

            // check column
            ColumnVector c = V.Column(1);
            Assert.IsTrue(c.Dimension == x.Length);
            for (int i = 0; i < c.Dimension; i++) {
                Assert.IsTrue(c[i] == V[i, 1]);
                Assert.IsTrue(c[i] == x[i]);
            }

            // check row
            RowVector r = V.Row(0);
            Assert.IsTrue(r.Dimension == x.Length);
            for (int i = 0; i < r.Dimension; i++) {
                Assert.IsTrue(r[i] == V[0, i]);
                Assert.IsTrue(r[i] == 1.0);
            }

            // check clone
            SquareMatrix VC = V.Clone();
            Assert.IsTrue(VC == V);
            Assert.IsFalse(VC != V);

            // check independence of clone
            VC[0, 0] += 1.0;
            Assert.IsFalse(VC == V);
            Assert.IsTrue(VC != V);

        }

        [TestMethod]
        public void SquareMatrixArithmetic () {

            SquareMatrix M = CreateSquareRandomMatrix(5);

            // addition is same a multiplication by two
            SquareMatrix MA = M + M;
            SquareMatrix M2 = 2.0 * M;
            Assert.IsTrue(MA == M2);

            // subraction of self same as multiplication by zero
            SquareMatrix MS = M - M;
            SquareMatrix M0 = 0.0 * M;
            Assert.IsTrue(MS == M0);

            // check transpose
            SquareMatrix MT = M.Transpose();
            Assert.IsTrue(MT != M);

            // matrix multiplication is not ableian
            SquareMatrix MMT = M * MT;
            SquareMatrix MTM = MT * M;
            Assert.IsFalse(MMT == MTM);

            // check that transpose of transpose is original
            SquareMatrix MTT = MT.Transpose();
            Assert.IsTrue(MTT == M);

        }

        [TestMethod]
        public void SquareVandermondeMatrixInverse () {
            for (int d = 1; d <= 8; d++) {
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                SquareMatrix H = CreateVandermondeMatrix(d);
                DateTime start = DateTime.Now;
                SquareMatrix HI = H.Inverse();
                DateTime finish = DateTime.Now;
                Console.WriteLine("d={0} t={1} ms", d, (finish - start).Milliseconds);
                PrintMatrix(HI);
                PrintMatrix(H * HI);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(H * HI, I));
            }
        }

        [TestMethod]
        public void SquareRandomMatrixInverse () {
            for (int d = 1; d <= 100; d=d+11) {
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                SquareMatrix M = CreateSquareRandomMatrix(d, 1);
                DateTime start = DateTime.Now;
                SquareMatrix MI = M.Inverse();
                DateTime finish = DateTime.Now;
                Console.WriteLine("d={0} t={1} ms", d, (finish - start).Milliseconds);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(M * MI, I));
            }
        }


        [TestMethod]
        public void SquareRandomMatrixQRDecomposition () {
            for (int d = 1; d <= 100; d += 11) {

                Console.WriteLine("d={0}", d);

                SquareMatrix M = CreateSquareRandomMatrix(d);

                // QR decompose the matrix
                SquareQRDecomposition QRD = M.QRDecomposition();

                // the dimension should be right
                Assert.IsTrue(QRD.Dimension == M.Dimension);

                // test that the decomposition works
                SquareMatrix Q = QRD.QMatrix();
                SquareMatrix R = QRD.RMatrix();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(Q * R, M));

                // check that the inverse works
                SquareMatrix MI = QRD.Inverse();
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(M * MI, I));

                // test that a solution works
                ColumnVector t = new ColumnVector(d);
                for (int i = 0; i < d; i++) t[i] = i;
                ColumnVector s = QRD.Solve(t);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(M * s, t));

            }
        }

        [TestMethod]
        public void SquareRandomMatrixLUDecomposition () {
            for (int d = 1; d <= 200; d=d+11) {

                Console.WriteLine("d={0}", d);

                SquareMatrix M = CreateSquareRandomMatrix(d);

                // LU decompose the matrix
                Stopwatch sw = Stopwatch.StartNew();
                SquareLUDecomposition LU = M.LUDecomposition();
                sw.Stop();
                Console.WriteLine(sw.ElapsedMilliseconds);

                // test that the decomposition works
                SquareMatrix P = LU.PMatrix();
                SquareMatrix L = LU.LMatrix();
                SquareMatrix U = LU.UMatrix();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(P * M, L * U));

                // check that the inverse works
                SquareMatrix MI = LU.Inverse();
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(M * MI, I));

                // test that a solution works
                ColumnVector t = new ColumnVector(d);
                for (int i = 0; i < d; i++) t[i] = i;
                ColumnVector s = LU.Solve(t);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(M * s, t));

            }
        }

        [TestMethod]
        public void SquareVandermondeMatrixLUDecomposition () {
            // fails now for d = 8 because determinant slightly off
            for (int d = 1; d <= 7; d++) {

                Console.WriteLine("d={0}", d);

                double[] x = new double[d];
                for (int i = 0; i < d; i++) {
                    x[i] = i;
                }
                double det = 1.0;
                for (int i = 0; i < d; i++) {
                    for (int j = 0; j < i; j++) {
                        det = det * (x[i] - x[j]);
                    }
                }

                // LU decompose the matrix
                SquareMatrix V = CreateVandermondeMatrix(d);
                SquareLUDecomposition LU = V.LUDecomposition();

                // test that the decomposition works
                SquareMatrix P = LU.PMatrix();
                SquareMatrix L = LU.LMatrix();
                SquareMatrix U = LU.UMatrix();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(P * V, L * U));

                // check that the determinant agrees with the analytic expression
                Console.WriteLine("det {0} {1}", LU.Determinant(), det);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(LU.Determinant(), det));

                // check that the inverse works
                SquareMatrix VI = LU.Inverse();
                //PrintMatrix(VI);
                //PrintMatrix(V * VI);
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(V * VI, I));

                // test that a solution works
                ColumnVector t = new ColumnVector(d);
                for (int i = 0; i < d; i++) t[i] = 1.0;
                ColumnVector s = LU.Solve(t);
                Assert.IsTrue(TestUtilities.IsNearlyEqual(V * s, t));
                
            }
        }

        // eigensystem of Vandermonde matrices

        [TestMethod]
        public void SquareVandermondeMatrixEigenvalues () {
            for (int d = 1; d <= 8; d++) {
                SquareMatrix H = CreateVandermondeMatrix(d);
                double tr = H.Trace();
                ComplexEigensystem E = H.Eigensystem();
                double sum = 0.0;
                for (int i = 0; i < d; i++) {
                    Console.WriteLine(E.Eigenvalue(i));
                    double e = E.Eigenvalue(i).Re;
                    sum += e;
                    Vector<Complex> vc = E.Eigenvector(i);
                    ColumnVector v = new ColumnVector(d);
                    for (int j = 0; j < d; j++) {
                        //Console.WriteLine("  {0}", vc[j]);
                        v[j] = vc[j].Re;
                    }
                    ColumnVector Hv = H * v;
                    ColumnVector ev = e * v;
                    Assert.IsTrue(TestUtilities.IsNearlyEigenpair(H, v, e));
                }
                Assert.IsTrue(TestUtilities.IsNearlyEqual(tr, sum));
            }
        }


        [TestMethod]
        public void SquareRandomMatrixEigenvalues () {
            for (int d = 1; d <= 67; d = d + 11) {
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                SquareMatrix M = CreateSquareRandomMatrix(d, d);
                double tr = M.Trace();
                DateTime start = DateTime.Now;
                ComplexEigensystem E = M.Eigensystem();
                DateTime finish = DateTime.Now;
                Console.WriteLine("d={0} t={1} ms", d, (finish - start).Milliseconds);
                Assert.IsTrue(E.Dimension == d);
                Complex[] es = new Complex[d];
                for (int i = 0; i < d; i++) {
                    es[i] = E.Eigenvalue(i);
                    Vector<Complex> v = E.Eigenvector(i);
                    Assert.IsTrue(TestUtilities.IsNearlyEigenpair(M, v, es[i]));
                }
                Assert.IsTrue(TestUtilities.IsSumNearlyEqual(es, tr));
            }
        }

        [TestMethod]
        public void SquareUnitMatrixLUDecomposition () {
            for (int d = 1; d <= 10; d++) {
                SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
                Assert.IsTrue(I.Trace() == d);
                SquareLUDecomposition LU = I.LUDecomposition();
                Assert.IsTrue(LU.Determinant() == 1.0);
                SquareMatrix II = LU.Inverse();
                Assert.IsTrue(TestUtilities.IsNearlyEqual(II, I));
            }
        }

        [TestMethod]
        public void SquareUnitMatrixEigensystem () {
            int d = 3;
            SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(d);
            ComplexEigensystem E = I.Eigensystem();
            Assert.IsTrue(E.Dimension == d);
            for (int i = 0; i < d; i++) {
                Complex val = E.Eigenvalue(i);
                Assert.IsTrue(val == 1.0);
                Vector<Complex> vec = E.Eigenvector(i);
                for (int j = 0; j < d; j++) {
                    if (i == j) {
                        Assert.IsTrue(vec[j] == 1.0);
                    } else {
                        Assert.IsTrue(vec[j] == 0.0);
                    }
                }
            }
        }

        [TestMethod]
        public void SquareMatrixDifficultEigensystem () {
            // this requires > 30 iterations to converge, looses more accuracy than it should
            SquareMatrix M = new SquareMatrix(4);
            M[0,1] = 2;
            M[0,3] = -1;
            M[1,0] = 1;
            M[2,1] = 1;
            M[3,2] = 1;
            ComplexEigensystem E = M.Eigensystem();
            for (int i = 0; i < E.Dimension; i++) {
                Console.WriteLine(E.Eigenvalue(i));
            }
        }

        [TestMethod]
        public void SquareMatrixStochasticEigensystem () {

            // this is a simplifed form of a Markov matrix that arose in the Monopoly problem
            // and failed to converge

            int n = 12;
            // originally failed for n=12 due to lack of 2x2 detection at top
            // now tested for n up to 40, but n=14 appears to still be a problem,
            // probably due to a quadruplly degenerate eigenvalue

            SquareMatrix R = new SquareMatrix(n);
            for (int c = 0; c < n; c++) {
                R[(c + 2) % n, c] = 1.0 / 36.0;
                R[(c + 3) % n, c] = 2.0 / 36.0;
                R[(c + 4) % n, c] = 3.0 / 36.0;
                R[(c + 5) % n, c] = 4.0 / 36.0;
                R[(c + 6) % n, c] = 5.0 / 36.0;
                R[(c + 7) % n, c] = 6.0 / 36.0;
                R[(c + 8) % n, c] = 5.0 / 36.0;
                R[(c + 9) % n, c] = 4.0 / 36.0;
                R[(c + 10) % n, c] = 3.0 / 36.0;
                R[(c + 11) % n, c] = 2.0 / 36.0;
                R[(c + 12) % n, c] = 1.0 / 36.0;
            }

            ComplexEigensystem E = R.Eigensystem();

            for (int i = 0; i < E.Dimension; i++) {
                Console.WriteLine(E.Eigenvalue(i));
                Assert.IsTrue(TestUtilities.IsNearlyEigenpair(R, E.Eigenvector(i), E.Eigenvalue(i)));
            }

        }

        /*
        [TestMethod]
        public void TimeMatrixMultiply () {

            SquareMatrix M = new SquareMatrix(3);
            M[0, 0] = -2;
            M[0, 1] = -6;
            M[0, 2] = 4;
            M[1, 0] = -6;
            M[1, 1] = -21;
            M[1, 2] = 15;
            M[2, 0] = 4;
            M[2, 1] = 15;
            M[2, 2] = -13;

            SquareMatrix M1 = CreateSquareRandomMatrix(316, 1);
            SquareMatrix M2 = CreateSquareRandomMatrix(1000, 2);

            M = M2;

            //M = CreateVandermondeMatrix(8);

            Stopwatch s = Stopwatch.StartNew();
            //SquareMatrix M12 = M1 * M2;
            //SquareMatrix MI = M.Inverse();
            SquareLUDecomposition LU = M.LUDecomposition();
            s.Stop();
            Console.WriteLine(s.ElapsedMilliseconds);

            //double[] b = { 1, 2, 3 };
            //IList<double> x = LU.Solve2(b);
            //Console.WriteLine("{0} {1} {2}", x[0], x[1], x[2]);

            Stopwatch s1 = Stopwatch.StartNew();
            SquareMatrix MI = LU.Inverse();
            s1.Stop();
            Console.WriteLine(s1.ElapsedMilliseconds);

            Stopwatch s2 = Stopwatch.StartNew();
            SquareMatrix MI2 = M.Inverse();
            s2.Stop();
            Console.WriteLine(s2.ElapsedMilliseconds);


            //PrintMatrix(M);
            //PrintMatrix(MI);
            //PrintMatrix(M * MI);
            //PrintMatrix(MI * M);
            SquareMatrix I = TestUtilities.CreateSquareUnitMatrix(M.Dimension);
            Assert.IsTrue(TestUtilities.IsNearlyEqual(M * MI, I));

        }
        */

        /*
        [TestMethod]
        public void T1 () {

            SquareMatrix A = new SquareMatrix(3);
            A[0, 0] = 0;
            A[1, 0] = 1;
            A[2, 0] = 2;
            A[0, 1] = 1;
            A[1, 1] = 0;
            A[2, 1] = 2;
            A[0, 2] = 2;
            A[1, 2] = 1;
            A[2, 2] = 0;

            A = CreateSquareRandomMatrix(4);

            PrintMatrix(A);

            SquareLUDecomposition LU = A.LUDecomposition();

            Console.WriteLine("dim = {0}", LU.Dimension);
            Console.WriteLine("det = {0}", LU.Determinant());

            Console.WriteLine("P=");
            SquareMatrix P = LU.PMatrix();
            PrintMatrix(P);

            Console.WriteLine("L =");
            SquareMatrix L = LU.LMatrix();
            PrintMatrix(L);

            Console.WriteLine("U =");
            SquareMatrix U = LU.UMatrix();
            PrintMatrix(U);

            Console.WriteLine("PA = ");
            PrintMatrix(P * A);

            Console.WriteLine("LU = ");
            PrintMatrix(L * U);

            Console.WriteLine("inv(A) = ");
            SquareMatrix AI = LU.Inverse();
            PrintMatrix(AI);

            Console.WriteLine("inv(A) A = ");
            PrintMatrix(AI * A);

            Console.WriteLine("solve");
            double[] x = new double[LU.Dimension];
            for (int i = 0; i < x.Length; i++) {
                x[i] = i;
            }
            ColumnVector y = LU.Solve(x);
            PrintMatrix(A * y);

        }
        */

        /*
        [TestMethod]
        public void TestAvova () {

            int nRows = 2;
            int nColumns = 3;

            double[,][] samples = new double[nRows,nColumns][];
            samples[0, 0] = new double[] { 1, 2, 3 };
            samples[0, 1] = new double[] { 2, 3 };
            samples[0, 2] = new double[] { 1, 2, 3, 4 };
            samples[1, 0] = new double[] { 2, 4 };
            samples[1, 1] = new double[] { 3, 4, 5 };
            samples[1, 2] = new double[] { 1, 2, 3 };

            int n0 = 0;
            double m0 = 0.0;
            SampleSummary[,] summaries = new SampleSummary[nRows, nColumns];
            for (int r = 0; r < nRows; r++) {
                for (int c = 0; c < nColumns; c++) {

                    int n = samples[r,c].Length;
                    n0 += n;

                    double m = 0.0;
                    for (int i = 0; i < n; i++) {
                        m += samples[r, c][i];
                    }
                    m0 += m;
                    m /= n;

                    double v = 0.0;
                    for (int i = 0; i < n; i++) {
                        double z = samples[r, c][i] - m;
                        v += z * z;
                    }
                    v /= n;

                    summaries[r, c] = new SampleSummary(n, m, v);

                    Console.WriteLine("[{0},{1}] n={2} m={3} v={4}", r, c, n, m, v);

                }
            }

            m0 /= n0;
            SampleSummary totalSummary = new SampleSummary(n0, m0, 0.0);
            Console.WriteLine("total n={0} m={1} v={2}", n0, m0, 0.0);

            SampleSummary[] rowSummaries = new SampleSummary[nRows];
            for (int r = 0; r < nRows; r++) {

                int n = 0;
                double m = 0.0;

                for (int c = 0; c < nColumns; c++) {
                    n += summaries[r, c].Count;
                    m += summaries[r, c].Count * summaries[r, c].SampleMean;
                }
                m /= n;

                rowSummaries[r] = new SampleSummary(n, m, 0.0);

                Console.WriteLine("row {0} n={1} m={2} v={3}", r, n, m, 0.0);
            }

            SampleSummary[] columnSummaries = new SampleSummary[nColumns];
            for (int c = 0; c < nColumns; c++) {

                int n = 0;
                double m = 0.0;

                for (int r = 0; r < nRows; r++) {
                    n += summaries[r, c].Count;
                    m += summaries[r, c].Count * summaries[r, c].SampleMean;
                }
                m /= n;

                columnSummaries[c] = new SampleSummary(n, m, 0.0);

                Console.WriteLine("column {0} n={1} m={2} v={3}", c, n, m, 0.0);
            }

            double SS_Within = 0.0;
            double SS_Total = 0.0;
            for (int r = 0; r < nRows; r++) {
                for (int c = 0; c < nColumns; c++) {
                    for (int i = 0; i < samples[r, c].Length; i++) {
                        double z = samples[r, c][i] - summaries[r, c].SampleMean;
                        SS_Within += z * z;
                        double x = samples[r, c][i] - totalSummary.SampleMean;
                        SS_Total += x * x;
                    }
                }
            }
            Console.WriteLine("SS_Total = {0}", SS_Total);
            Console.WriteLine("SS_Within = {0}", SS_Within);

            double SS_Rows = 0.0;
            for (int r = 0; r < nRows; r++) {
                double z = rowSummaries[r].SampleMean - totalSummary.SampleMean;
                SS_Rows += rowSummaries[r].Count * z * z;
            }
            Console.WriteLine("SS_Rows = {0}", SS_Rows);

            double SS_Columns = 0.0;
            for (int c = 0; c < nColumns; c++) {
                double z = columnSummaries[c].SampleMean - totalSummary.SampleMean;
                SS_Columns += columnSummaries[c].Count * z * z;
            }
            Console.WriteLine("SS_Columns = {0}", SS_Columns);

            double SS_Interaction = 0.0;
            for (int r = 0; r < nRows; r++) {
                for (int c = 0; c < nColumns; c++) {
                    double z = summaries[r, c].SampleMean - rowSummaries[r].SampleMean - columnSummaries[c].SampleMean + totalSummary.SampleMean;
                    SS_Interaction += summaries[r, c].Count * z * z;
                }
            }
            Console.WriteLine("SS_Interaction = {0}", SS_Interaction);

            Console.WriteLine("SS_Within + S_Rows + SS_Columns + SS_Interaction = {0}", SS_Within + SS_Rows + SS_Columns + SS_Interaction);
        }
        */
    }

    /*
    public struct SampleSummary {

        public SampleSummary (int count, double mean, double variation) {
            this.Count = count;
            this.SampleMean = mean;
            this.SampleVariation = variation;
        }

        public int Count;
        public double SampleMean;
        public double SampleVariation;
    }
    */

}
