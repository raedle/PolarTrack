﻿using System.Drawing;
using Huddle.Engine.Processor.OpenCv.Filter;

namespace Huddle.Engine.Filter.Impl
{
    public class KalmanFilter
    {
        #region private properties

        private Emgu.CV.KalmanFilter _kalman;
        private SyntheticData _syntheticData;

        #endregion

        #region ctor

        public KalmanFilter(float strengthTMatrix, double processNoise, double measurementNoise)
        {
            InitializeKalman(strengthTMatrix, processNoise, measurementNoise);
        }

        public KalmanFilter()
        {
            InitializeKalman(0.6f, 1.0e-2, 1.0e-1);
        }

        #endregion

        #region private methods

        private void InitializeKalman(float strengthTMatrix, double processNoise, double measurementNoise)
        {
            _syntheticData = new SyntheticData(strengthTMatrix, processNoise, measurementNoise);
            _kalman = new Emgu.CV.KalmanFilter((int)strengthTMatrix,
                (int)measurementNoise,
                (int)processNoise);
        }

        private PointF[] FilterPoint(PointF pt)
        {
            _syntheticData.State[0, 0] = pt.X;
            _syntheticData.State[1, 0] = pt.Y;

            var prediction = _kalman.Predict();

            int[] a = {0, 0};
            int[] b = {1, 0};
            var predictPoint = new PointF(prediction.GetData(a)[0] , prediction.GetData(b)[0]);

            var estimated = _kalman.Correct(_syntheticData.GetMeasurement());

            var estimatedPoint = new PointF(estimated.GetData(a)[0], estimated.GetData(b)[0]);

            var results = new PointF[2];
            results[0] = predictPoint;
            results[1] = estimatedPoint;

            _syntheticData.GoToNextState();

            return results;
        }

        #endregion

        #region public methods

        public Point GetPredictedPoint(Point pt)
        {
            var pointF = FilterPoint(new PointF(pt.X, pt.Y));

            var x = pointF[0].X;
            var y = pointF[0].Y;

            return new Point((int)x, (int)y);
        }

        public Point GetEstimatedPoint(Point pt)
        {
            var pointF = FilterPoint(new PointF(pt.X, pt.Y));

            var x = pointF[1].X;
            var y = pointF[1].Y;

            return new Point((int)x, (int)y);
        }


        #endregion
    }
}
