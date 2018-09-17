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
using System.Windows.Forms;

namespace TCGA_Genetic_Workbench
{
    public class ACEUniverse : Universe
    {
        static Random rnd = null;
        public int numSamples;
        public int numTargetMeasures;
        public int numSourceMeasures;
        public int[][] coverage;
        public int coverageX, coverageY;
        public Int64 numOrganisms;
        public Int64 nextThreshold;
        public Int64 thresholdStep;
        public Boolean passedThreshold;
        public Cache cache;
        public int correlationType;
        public bool allTargets;
        public string evoSummary;
        public int brafMode;

        public ACEUniverse()
        {
            adam = new Correlation(this);
            if (rnd == null)
            {
                rnd = new Random();
            }

            nextThreshold = -1;
            correlationType = 0;
            allTargets = false;
            evoSummary = "";
            brafMode = 0;
        }

        public void updateCoverage(int sourceMeasure, int targetMeasure)
        {
            int progressX = rnd.Next(20);
            int progressY = rnd.Next(50);
            coverage[progressX][progressY]++;

            if (numOrganisms > nextThreshold)
            {
                passedThreshold = true;
                nextThreshold = numOrganisms + ((numTargetMeasures * numSourceMeasures) / 400);
            }
        }
    }
}
