/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Evolution;
using System.Drawing;
using MathNet.Numerics;

namespace TCGA_Genetic_Workbench
{
    public class Correlation : Species
    {
        static Random rnd = null;
        static ACEUniverse universe = null;
        public int sourceMeasure;
        public int targetMeasure;
        public string sourceName = "";
        public string targetName = "";
        public List<RegressionMetrics> regressionMetrics;
        public float rSquared;
        public int sourceTransformation;
        public int targetTransformation;
        const int NOTF = 0;
        const int LOGTF = 1;
        const int SQRTTF = 2;
        const int ARCSIN = 3;
        const int NOTF_WEIGHTING = 7;
        //const int LOGTF_WEIGHTING = 0;
        //const int SQRTTF_WEIGHTING = 0;
        //const int ARCSIN_WEIGHTING = 0;
        const int LOGTF_WEIGHTING = 2;
        const int SQRTTF_WEIGHTING = 1;
        const int ARCSIN_WEIGHTING = 10;

        public Correlation(ACEUniverse tu)
        {
            universe = tu;
            if (rnd == null)
            {
                rnd = new Random();
            }
        }

        public Correlation()
        {
        }

        public override Species create()
        {
            return (new Correlation());
        }

        override public void makeRandom(int status)
        {
            sourceMeasure = rnd.Next(universe.numSourceMeasures);
            targetMeasure = rnd.Next(universe.numTargetMeasures);

            sourceTransformation = -1;
            targetTransformation = -1;

            bool sourceMeth = false;
            bool targetMeth = false;

            if (universe.cache.getSourcePipeline(sourceMeasure).Contains("meth"))
            {
                sourceMeth = true;
            }

            if (universe.cache.getTargetPipeline(targetMeasure).Contains("meth"))
            {
                targetMeth = true;
            }

            shuffleTransformations(ref sourceTransformation, ref targetTransformation, sourceMeth, targetMeth);

            universe.updateCoverage(sourceMeasure, targetMeasure);

            universe.numOrganisms++;
        }

        private void shuffleTransformations(ref int sourcetf, ref int targettf, bool sourceMeth, bool targetMeth)
        {
            if (sourcetf == -1)
            {
                sourcetf = rndTransformation(-1, -1, sourceMeth);
                targettf = rndTransformation(-1, -1,targetMeth);
            }
            else if (rnd.Next(2) == 0)
            {
                sourcetf = rndTransformation(sourcetf, -1, sourceMeth);
            }
            else
            {
                targettf = rndTransformation(-1, targettf, targetMeth);
            }
        }

        private int rndTransformation(int not1, int not2, bool isMeth)
        {
            // not1, not2 used to avoid returning the same transformation as before,
            // and to avoid making source and target transformations identical.

            int rndint = 0;
            int rndtf = 0;
            int totalWeighting = NOTF_WEIGHTING + LOGTF_WEIGHTING + SQRTTF_WEIGHTING;

            if (isMeth)
            {
                totalWeighting += ARCSIN_WEIGHTING;
            }

            int loopCount = 0;

            do
            {
                if (++loopCount > 500)
                {
                    loopCount = 600;
                }

                rndint = rnd.Next(totalWeighting);

                if (rndint < NOTF_WEIGHTING)
                {
                    rndtf = NOTF;
                }
                else if (rndint < NOTF_WEIGHTING + LOGTF_WEIGHTING)
                {
                    rndtf = LOGTF;
                }
                else if (rndint < NOTF_WEIGHTING + LOGTF_WEIGHTING + SQRTTF_WEIGHTING)
                {
                    rndtf = SQRTTF;
                }
                else
                {
                    rndtf = ARCSIN;
                }
            }
            while ((rndtf == not1) || (rndtf == not2));

            return (rndtf);
        }

        override public void makeMutation(int status, Species parent)
        {
            this.makeClone((Correlation)parent);

            bool sourceMeth = false;
            bool targetMeth = false;

            string sPipeline = universe.cache.getSourcePipeline(sourceMeasure);
            string tPipeline = universe.cache.getTargetPipeline(targetMeasure);

            if (sPipeline.Contains("meth"))
            {
                sourceMeth = true;
            }

            if (tPipeline.Contains("meth"))
            {
                targetMeth = true;
            }

            if (rnd.Next(2) == 0)
            {
                this.sourceTransformation = rndTransformation(-1, -1, sourceMeth);
            }
            else
            {
                this.targetTransformation = rndTransformation(-1, -1, targetMeth);
            }

            universe.updateCoverage(sourceMeasure, targetMeasure);

            universe.numOrganisms++;
        }

        private void makeClone(Correlation p)
        {
            this.generation = p.generation;
            this.mutations = p.mutations;
        }

        override public void makeChild(int status, Species parent1, Species parent2)
        {
            this.makeClone((Correlation)parent1);
            Correlation p2 = (Correlation)parent2;
            
            if (rnd.Next(2) == 0)
            {
                sourceMeasure = p2.sourceMeasure;
                sourceTransformation = p2.sourceTransformation;
            }
            else
            {
                targetMeasure = p2.targetMeasure;
                targetTransformation = p2.targetTransformation;
            }
 
            universe.updateCoverage(sourceMeasure, targetMeasure);

            universe.numOrganisms++;
        }

        override public Boolean isDuplicate(Species s)
        {
            Correlation other = (Correlation)s;
            /*
            if ((other.sourceMeasure == this.sourceMeasure) && ((universe.allTargets) || (other.targetMeasure == this.targetMeasure)))
            {
                return (true);
            }
             * */

            if ((universe.numSourceMeasures > 30) && (other.sourceMeasure == this.sourceMeasure))
            {
                return (true);
            }

            if ((universe.numTargetMeasures > 30) && (other.targetMeasure == this.targetMeasure))
            {
                return (true);
            }

            return (false);
        }

        override public Boolean incompatible(Species partner)
        {
            return (false);
        }

        override public void testOrganism(Boolean recalcAll)
        {

            Boolean isSourceEnumerated = false, isTargetEnumerated = false;
            float[] sourceData = null, targetData = null;

            universe.cache.getSourceMeasureInfo(sourceMeasure, ref sourceName, ref isSourceEnumerated, ref sourceData, sourceTransformation, universe.brafMode);

            int numTargets = 1;

            if (universe.allTargets)
            {
                numTargets = universe.numTargetMeasures;
            }

            success = 0;

            regressionMetrics = new List<RegressionMetrics>();
            rSquared = 0;

            for (int targetIndex = 0; targetIndex < numTargets; targetIndex++)
            {
                RegressionMetrics rm = new RegressionMetrics();

                int measure;
                if (universe.allTargets)
                {
                    measure = targetIndex;
                }
                else
                {
                    measure = targetMeasure;
                }

                universe.cache.getTargetMeasureInfo(measure, ref targetName, ref isTargetEnumerated, ref targetData, targetTransformation, universe.brafMode);

                int okCount = 0;
                int sokCount = 0;
                int tokCount = 0;

                for (int i = 0; i < sourceData.Length; i++)
                {
                    if (universe.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue) && (targetData[i] != float.MinValue))
                    {
                        okCount++;
                    }
                    if (universe.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue))
                    {
                        sokCount++;
                    }
                    if (universe.cache.okAfterFilters[i] && (targetData[i] != float.MinValue))
                    {
                        tokCount++;
                    }
                }

                float[] sd = new float[okCount];
                float[] td = new float[okCount];
                int didx = 0;

                for (int i = 0; i < sourceData.Length; i++)
                {
                    if (universe.cache.okAfterFilters[i] && (sourceData[i] != float.MinValue) && (targetData[i] != float.MinValue))
                    {
                        sd[didx] = sourceData[i];
                        td[didx++] = targetData[i];
                    }
                }

                linearRegression(sd, td, 0, sd.Length, out rm.rSquared, out rm.lineIntercept, out rm.lineSlope);

                int targetSuccess = (int)(rm.rSquared * 1000000);

                if ((universe.correlationType == 1) && (rm.lineSlope < 0))
                {
                    targetSuccess = 0;
                }

                if ((universe.correlationType == 2) && (rm.lineSlope > 0))
                {
                    targetSuccess = 0;
                }

                rSquared += rm.rSquared;

                regressionMetrics.Add(rm);

                if (sokCount > 7)
                {
                    sokCount = 8;
                }

                if (tokCount > 7)
                {
                    tokCount = 8;
                }

                if (okCount > 7)
                {
                    success += targetSuccess;
                }
            }

            rSquared = rSquared / regressionMetrics.Count;
        }

        override public void addGenesToPool()
        {
        }

        public void linearRegression(float[] xVals, float[] yVals,
                                            int inclusiveStart, int exclusiveEnd,
                                            out float rsquared, out float yintercept,
                                            out float slope)
        {
            float sumOfX = 0;
            float sumOfY = 0;
            float sumOfXSq = 0;
            float sumOfYSq = 0;
            float ssX = 0;
            float ssY = 0;
            float sumCodeviates = 0;
            float sCo = 0;
            float count = exclusiveEnd - inclusiveStart;

            for (int ctr = inclusiveStart; ctr < exclusiveEnd; ctr++)
            {
                float x = xVals[ctr];
                float y = yVals[ctr];
                sumCodeviates += x * y;
                sumOfX += x;
                sumOfY += y;
                sumOfXSq += x * x;
                sumOfYSq += y * y;
            }
            ssX = sumOfXSq - ((sumOfX * sumOfX) / count);
            ssY = sumOfYSq - ((sumOfY * sumOfY) / count);
            float RNumerator = (count * sumCodeviates) - (sumOfX * sumOfY);
            float RDenom = (count * sumOfXSq - (sumOfX * sumOfX))
             * (count * sumOfYSq - (sumOfY * sumOfY));
            sCo = sumCodeviates - ((sumOfX * sumOfY) / count);

            float meanX = sumOfX / count;
            float meanY = sumOfY / count;

            if ((RDenom >= -0.0001) && (RDenom <= 0.0001))
            {
                RDenom = 0.001F;
            }


            float dblR = (float)((double)RNumerator / Math.Sqrt(RDenom));
            rsquared = dblR * dblR;
            yintercept = meanY - ((sCo / ssX) * meanX);
            slope = sCo / ssX;
            /*
            Double[] xd = new Double[xVals.Length];
            Double[] yd = new Double[yVals.Length];

            for (int i = 0; i < xd.Length; i++)
            {
                xd[i] = xVals[i];
                yd[i] = yVals[i];
            }

            Tuple<double, double> tuple = MathNet.Numerics.LinearRegression.SimpleRegression.Fit(xd, yd);
            double mnintercept = tuple.Item1;
            double mnslope = tuple.Item2;
             * */
        }
    }
}
