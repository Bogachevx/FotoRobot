using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FotoRobot
{
    public struct Parameters
    {
        public Size cameraResolution;
        public Size ROISize;
        public Point ROIPosition;
        public int cannyThreshold1;
        public int cannyThreshold2;
        public double approximationEpsilon;
        public int minContLength;
        public int blurValue;
        public bool isAdaptive;
        public Size paperSize; 
    }
}
