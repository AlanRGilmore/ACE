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
    public class Filter
    {
        public string pipeline;
        public int measure;
        public List<int> excluded;
    }
}
