/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGA_Genetic_Workbench
{
    public class MeasureParams
    {
        public string barFile;
        public string name;
        public Int32 numEntries;
        public Int32 numSteps;
        public Int32 stepExp;
        public float lowThresh;
        public float step;
    }
}
