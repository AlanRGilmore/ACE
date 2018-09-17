/*
 * This file is subject to the terms and conditions defined in
 * file 'LICENSE.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TCGA_Genetic_Workbench
{
    public class Cache
    {
        private int[] targetMeasures;
        private int[] sourceMeasures;
        private CacheSlot[] targetSlots;
        private CacheSlot[] sourceSlots;
        public Boolean[] okAfterFilters;
        private List<CacheBucket> buckets;
        List<PipelineSelection> pipeLineSelections;
        int numSourceCached;
        int numTargetCached;
        int cacheSlotsFilled;

        public Cache(List<PipelineSelection> pls, int numSourceMeasures, int numTargetMeasures, int numCached)
        {
            pipeLineSelections = pls;

            sourceMeasures = new int[numSourceMeasures];
            targetMeasures = new int[numTargetMeasures];

            numSourceCached = 0;
            numTargetCached = numCached;

            if (numTargetMeasures < numCached)
            {
                numTargetCached = numTargetMeasures;
                numSourceCached = numCached - numTargetMeasures;

                if (numSourceMeasures < numSourceCached)
                {
                    numSourceCached = numSourceMeasures;
                }
            }

            cacheSlotsFilled = 0;

            targetSlots = new CacheSlot[numTargetCached];
            sourceSlots = new CacheSlot[numSourceCached];

            for (int i = 0; i < numTargetCached; i++)
            {
                targetSlots[i] = null;
            }

            for (int i = 0; i < numSourceCached; i++)
            {
                sourceSlots[i] = null;
            }
        }

        public int percentCached()
        {
            return ((cacheSlotsFilled * 100) / (numSourceCached + numTargetCached));
        }

        public void setSourceMeasureCode(int measure, int code)
        {
            sourceMeasures[measure] = code;
        }

        public void setTargetMeasureCode(int measure, int code)
        {
            targetMeasures[measure] = code;
        }

        public void getSourceMeasureInfo(int index, ref string name, ref Boolean isEnumerated, ref float[] data, int transformation, int brafMode)
        {
            if (index < numSourceCached)
            {
                if (sourceSlots[index] == null)
                {
                    sourceSlots[index] = new CacheSlot();
                    getMeasureInfo(sourceMeasures[index], ref sourceSlots[index].name, ref sourceSlots[index].isEnumerated, ref sourceSlots[index].data, transformation, brafMode);
                    cacheSlotsFilled++;
                }

                name = sourceSlots[index].name;
                isEnumerated = sourceSlots[index].isEnumerated;
                data = sourceSlots[index].data;
            }
            else
            {
                getMeasureInfo(sourceMeasures[index], ref name, ref isEnumerated, ref data, transformation, brafMode);
            }
        }

        public void getTargetMeasureInfo(int index, ref string name, ref Boolean isEnumerated, ref float[] data, int transformation, int brafMode)
        {
            if (index < numTargetCached)
            {
                if (targetSlots[index] == null)
                {
                    targetSlots[index] = new CacheSlot();
                    getMeasureInfo(targetMeasures[index], ref targetSlots[index].name, ref targetSlots[index].isEnumerated, ref targetSlots[index].data, transformation, brafMode);
                    cacheSlotsFilled++;
                }

                name = targetSlots[index].name;
                isEnumerated = targetSlots[index].isEnumerated;
                data = targetSlots[index].data;
            }
            else
            {
                getMeasureInfo(targetMeasures[index], ref name, ref isEnumerated, ref data, transformation, brafMode);
            }
        }

        public string getSourcePipeline(int index)
        {
            int pipelineIndex = sourceMeasures[index] / 10000000;
            string path = pipeLineSelections[pipelineIndex].path;

            string filename = Path.GetFileName(path);
            string filenameNoType = "";

            if (filename[3] == '.')
            {
                filenameNoType = filename.Substring(4);
            }
            else if (filename.StartsWith("SCORT_"))
            {
                filenameNoType = filename.Substring(6);
            }
            else
            {
                filenameNoType = filename.Substring(5);
            }

            return filenameNoType;
        }

        public string getTargetPipeline(int index)
        {
            int pipelineIndex = targetMeasures[index] / 10000000;
            string path = pipeLineSelections[pipelineIndex].path;

            string filename = Path.GetFileName(path);
            string filenameNoType = "";

            if (filename[3] == '.')
            {
                filenameNoType = filename.Substring(4);
            }
            else if (filename.StartsWith("SCORT_"))
            {
                filenameNoType = filename.Substring(6);
            }
            else
            {
                filenameNoType = filename.Substring(5);
            }

            return filenameNoType;
        }

        private bool isBrafMutated(int index)
        {
            bool mutated = false;
            int[] mutants = { 4, 7, 66, 74, 84, 101, 111, 122, 129, 131, 156, 160, 161, 166, 185, 188, 201, 206, 214, 261};

            for (int i = 0; i < mutants.Length; i++)
            {
                if (index == mutants[i])
                {
                    mutated = true;
                }
            }

            return (mutated);
        }

        private void getMeasureInfo(int code, ref string name, ref Boolean isEnumerated, ref float[] data, int transformation, int brafMode)
        {
            int pipelineIndex = code / 10000000;
            int measureIndex = code % 10000000;
            string barFile = pipeLineSelections[pipelineIndex].path + "\\V_" + measureIndex.ToString().PadLeft(8, '0') + ".bar";

            isEnumerated = false;

            if (File.Exists(pipeLineSelections[pipelineIndex].path + "\\V_" + measureIndex.ToString().PadLeft(8, '0') + ".evl"))
            {
                isEnumerated = true;
            }

            FileInfo f = new FileInfo(barFile);

            int size = (int)(f.Length);
            byte[] valByteArray = new byte[size];
            data = new float[size / sizeof(float)];

            BinaryReader b = new BinaryReader(File.Open(barFile, FileMode.Open));
            b.Read(valByteArray, 0, size);
            b.Close();

            Buffer.BlockCopy(valByteArray, 0, data, 0, size);

            if (brafMode == 1)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (isBrafMutated(i) == false)
                    {
                        data[i] = float.MinValue;
                    }
                }
            }
            else if (brafMode == 2)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (isBrafMutated(i))
                    {
                        data[i] = float.MinValue;
                    }
                }
            }

            switch (transformation)
            {
                case 1:
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != float.MinValue)
                        {
                            data[i] = (float)Math.Log((double)data[i]);
                        }
                    }
                    break;
                case 2:
                    float dmin = float.MaxValue;

                    for (int i = 0; i < data.Length; i++)
                    {
                        if ((data[i] != float.MinValue) && (data[i] < dmin))
                        {
                            dmin = data[i];
                        }
                    }

                    if (dmin < 1)
                    {
                        for (int i = 0; i < data.Length; i++)
                        {
                            if (data[i] != float.MinValue)
                            {
                                data[i] += (1 - dmin);
                            }
                        }
                    }

                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != float.MinValue)
                        {
                            data[i] = (float)Math.Sqrt((double)data[i]);
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i < data.Length; i++)
                    {
                        if (data[i] != float.MinValue)
                        {
                            data[i] = (float)Math.Asin(Math.Sqrt((double)data[i]));
                        }
                    }
                    break;
            }

            name = "";
            Int32 numEntries = 0, numSteps = 0, stepExp = 0;
            float lowThresh = 0, step = 0;
            getFafInfo(pipeLineSelections[pipelineIndex].path + "\\AllValues.faf", measureIndex, ref name, ref numEntries, ref lowThresh, ref step, ref numSteps, ref stepExp);
        }

        private void getFafInfo(string faf, int measure, ref string name, ref Int32 numEntries, ref float lowThresh, ref float step, ref Int32 numSteps, ref Int32 stepExp)
        {
            BinaryReader b = new BinaryReader(File.Open(faf, FileMode.Open), Encoding.Unicode);
            int pos = 100 * measure;
            b.BaseStream.Seek(pos, SeekOrigin.Begin);

            char[] buf = b.ReadChars(38);
            name = new string(buf).Trim();
            numEntries = b.ReadInt32();
            lowThresh = b.ReadSingle();
            step = b.ReadSingle();
            numSteps = b.ReadInt32();
            stepExp = b.ReadInt32();
            b.Close();
        }
    }
}
