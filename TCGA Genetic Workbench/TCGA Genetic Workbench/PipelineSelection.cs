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
    public class PipelineSelection
    {
        public string path;
        public int status;
        public List<int> unSelectedExceptions;
        public List<int> sourceExceptions;
        public List<int> targetExceptions;

        public PipelineSelection(string ppath)
        {
            path = ppath;
            status = 0;
            unSelectedExceptions = new List<int>();
            sourceExceptions = new List<int>();
            targetExceptions = new List<int>();
        }

        public void setPipelineStatus(int value)
        {
            status = value;
            unSelectedExceptions = new List<int>();
            sourceExceptions = new List<int>();
            targetExceptions = new List<int>();
        }

        public void setMeasureStatus(int measure, int value)
        {
            if (unSelectedExceptions.Contains(measure))
            {
                unSelectedExceptions.Remove(measure);
            }

            if (sourceExceptions.Contains(measure))
            {
                sourceExceptions.Remove(measure);
            }

            if (targetExceptions.Contains(measure))
            {
                targetExceptions.Remove(measure);
            }

            if (value == status)
            {
                return;
            }

            switch (value)
            {
                case 0:
                    unSelectedExceptions.Add(measure);
                    unSelectedExceptions.Sort();
                    break;

                case 1:
                    sourceExceptions.Add(measure);
                    sourceExceptions.Sort();
                    break;

                case 2:
                    targetExceptions.Add(measure);
                    targetExceptions.Sort();
                    break;
            }
        }

        public int measureStatus(int measure)
        {
            if (unSelectedExceptions.Contains(measure))
            {
                return (0);
            } else if (sourceExceptions.Contains(measure))
            {
                return (1);
            } else if (targetExceptions.Contains(measure))
            {
                return (2);
            }

            return (status);
        }
    }
}
