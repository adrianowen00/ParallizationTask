using System;
using System.Collections.Generic;
using System.Text;

namespace DMParallel
{
    public class noteGraph
    {
        public double baseFreq;
        public double[] heights;
        public float div;

        public noteGraph(float inRange, float divisor)
        {
            this.baseFreq = inRange;
            this.div = divisor;
            this.heights = new double[(int)Math.Ceiling(baseFreq / div)];

        }

        public void setRectHeights(float[] values)
        {

            for (int ii = 0; ii < heights.Length; ii++)
            {
                int index = (int)Math.Floor(baseFreq / div + ii);

                heights[ii] = values[index];
            }


        }
    }
}
