//----------------------------------------------------------------------------
//  Copyright (C) 2004-2013 by EMGU. All rights reserved.       
//----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;


namespace SURF
{
    public static class DrawMatches
    {
        public static void FindMatch(Image<Bgr, Byte> modelImage, Image<Bgr, byte> observedImage,  
            out VectorOfKeyPoint modelKeyPoints, out VectorOfKeyPoint observedKeyPoints, 
            out Matrix<int> indices, out Matrix<byte> mask, out HomographyMatrix homography)
        {
            int k = 2;
            double uniquenessThreshold = 0.8;
            SURFDetector surfCPU = new SURFDetector(500, false);
            homography = null;


            // Вычисление ключевой точки(Detect) и сразу дескриптора(локальной особенности, Compute) для model image
            modelKeyPoints = new VectorOfKeyPoint();
            Image<Gray, Byte> cpuModelImage = modelImage.Convert<Gray, byte>();
            Matrix<float> modelDescriptors = surfCPU.DetectAndCompute(cpuModelImage, null, modelKeyPoints);

            // Вычисление ключевой точки и сразу дескриптора(локальной особенности) для observe image
            observedKeyPoints = new VectorOfKeyPoint();
            Image<Gray, Byte> cpuObservedImage = observedImage.Convert<Gray, byte>();
            Matrix<float> observedDescriptors = surfCPU.DetectAndCompute(cpuObservedImage, null, observedKeyPoints);

            // Сравнение дескрипторов
            BruteForceMatcher<float> matcher = new BruteForceMatcher<float>(DistanceType.L2);
            matcher.Add(modelDescriptors);
            indices = new Matrix<int>(observedDescriptors.Rows, k);
            using (Matrix<float> dist = new Matrix<float>(observedDescriptors.Rows, k))
            {
                matcher.KnnMatch(observedDescriptors, indices, dist, k, null);
                mask = new Matrix<byte>(dist.Rows, 1);
                mask.SetValue(255);
                Features2DToolbox.VoteForUniqueness(dist, uniquenessThreshold, mask);
            }

            // Сопоставление изображений (гомография)
            int nonZeroCount = CvInvoke.cvCountNonZero(mask);
            if (nonZeroCount >= 4)
            {
                nonZeroCount = Features2DToolbox.VoteForSizeAndOrientation(modelKeyPoints, 
                    observedKeyPoints, indices, mask, 1.5, 20);
                if (nonZeroCount >= 4)
                    homography = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(modelKeyPoints, 
                        observedKeyPoints, indices, mask, 2);
            }         
        }

        /// <summary>
        /// Draw the model image and observed image, the matched features and homography projection.
        /// </summary>
        /// <param name="modelImage">The model image</param>
        /// <param name="observedImage">The observed image</param>
        /// <returns>The model image and observed image, the matched features and homography projection.</returns>
        public static HomographyMatrix Draw(Image<Bgr, Byte> modelImage, Image<Bgr, byte> observedImage)
        {
            HomographyMatrix homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Matrix<int> indices;
            Matrix<byte> mask;

            FindMatch(modelImage, observedImage,  out modelKeyPoints, out observedKeyPoints, out indices, out mask, out homography);

            //Draw the matched keypoints
           //Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, observedImage, observedKeyPoints,
           //   indices, new Bgr(255, 0, 0), new Bgr(0, 255, 0), mask, Features2DToolbox.KeypointDrawType.DEFAULT);

            return homography;
        }

        public static Image<Bgr, Byte> SingleDraw(Image<Bgr, Byte> image)
        {
            SURFDetector surfCPU = new SURFDetector(500, false);

            // Вычисление ключевой точки(Detect) и сразу дескриптора(локальной особенности, Compute) для model image
            VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();
            Image<Gray, Byte> cpuImage = image.Convert<Gray, byte>();
            Matrix<float> Descriptors = surfCPU.DetectAndCompute(cpuImage, null, keyPoints);
            return Features2DToolbox.DrawKeypoints(image, keyPoints, new Bgr(255, 0, 0), Features2DToolbox.KeypointDrawType.DEFAULT);
        }

        public static Image<Bgr, Byte> DrawRectangle(Image<Bgr, Byte> modelImage, Image<Bgr, Byte> observedImage, HomographyMatrix homography, Bgr color)
        {
            #region draw the projected region on the image
            if (homography != null)
            {  //draw a rectangle along the projected model
                Rectangle rect = modelImage.ROI;
                PointF[] pts = new PointF[] { 
               new PointF(rect.Left, rect.Bottom),
               new PointF(rect.Right, rect.Bottom),
               new PointF(rect.Right, rect.Top),
               new PointF(rect.Left, rect.Top)};
                homography.ProjectPoints(pts);

                observedImage.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, color, 2);
            }
            #endregion

            return observedImage;
        }

    }
}
