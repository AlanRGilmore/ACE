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
    public class CacheBucket
    {
        byte[] data;

        public CacheBucket(int numSlots, int slotSize)
        {
            data = new byte[numSlots * slotSize];
        }

        public void putSlot(int slot, string name, float[] valArray)
        {
        }

        public void getSlot(int slot, ref string name, ref float[] valArray)
        {
        }
    }
}
