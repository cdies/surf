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
        // Находит гомографию и ругие параметры
        public static void FindMatch(Image<Bgr, Byte> modelImage, VectorOfKeyPoint observedKeyPoints,
            Matrix<float> observedDescriptors, out VectorOfKeyPoint modelKeyPoints, 
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
        // Возвращает изображение с гомографией(сопоставлением) одного объекта
        public static Image<Bgr, Byte> DrawWithHomography(Image<Bgr, Byte> modelImage,
            Image<Bgr, byte> observedImage, Image<Bgr, byte> SURF_image_result)
        {
            HomographyMatrix homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Matrix<float> observedDescriptors;
            Matrix<int> indices;
            Matrix<byte> mask;

            observedKeyPoints = KeyPointAndFeatures(observedImage, out observedDescriptors);

            FindMatch(modelImage, observedKeyPoints, observedDescriptors, out modelKeyPoints,
                        out indices, out mask, out homography);

            //Draw the matched keypoints
            Image<Bgr, Byte> result = Features2DToolbox.DrawMatches(modelImage, modelKeyPoints, SURF_image_result, observedKeyPoints,
               indices, new Bgr(255, 0, 0), new Bgr(0, 255, 0), mask, Features2DToolbox.KeypointDrawType.NOT_DRAW_SINGLE_POINTS);

            return result;

        }
        // Возвращает все найденные объекты
        public static Image<Bgr, Byte> Draw(Image<Bgr, Byte>[] modelImage, Image<Bgr, byte> observedImage)
        {
            HomographyMatrix homography;
            VectorOfKeyPoint modelKeyPoints;
            VectorOfKeyPoint observedKeyPoints;
            Matrix<float> observedDescriptors;
            Matrix<int> indices;
            Matrix<byte> mask;

            observedKeyPoints = KeyPointAndFeatures(observedImage, out observedDescriptors);

            for(int i = 0; i< modelImage.Length; i++)
            {
                if (modelImage[i] != null)
                {
                    FindMatch(modelImage[i], observedKeyPoints, observedDescriptors, out modelKeyPoints,
                        out indices, out mask, out homography);
                    observedImage = DrawRectangle(modelImage[i], observedImage, homography);
                }
            }

            return observedImage;
        }
        // Возвращает ключевые точки и локальные особенности одного изображения
        public static VectorOfKeyPoint KeyPointAndFeatures(Image<Bgr, Byte> image, out Matrix<float> descriptors)
        {
            SURFDetector surfCPU = new SURFDetector(500, false);
            VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();
            Image<Gray, Byte> cpuImage = image.Convert<Gray, byte>();
            descriptors = surfCPU.DetectAndCompute(cpuImage, null, keyPoints);
            return keyPoints;
        }
        // Показывает только локальные особенности без гомографии
        public static Image<Bgr, Byte> SingleDraw(Image<Bgr, Byte> image)
        {
            SURFDetector surfCPU = new SURFDetector(500, false);

            // Вычисление ключевой точки(Detect) и сразу дескриптора(локальной особенности, Compute) для image
            VectorOfKeyPoint keyPoints = new VectorOfKeyPoint();
            Image<Gray, Byte> cpuImage = image.Convert<Gray, byte>();
            Matrix<float> Descriptors = surfCPU.DetectAndCompute(cpuImage, null, keyPoints);
            return Features2DToolbox.DrawKeypoints(image, keyPoints, new Bgr(255, 0, 0), Features2DToolbox.KeypointDrawType.DEFAULT);
        }
        // Рисует рамку вокруг найденного изображения
        public static Image<Bgr, Byte> DrawRectangle(Image<Bgr, Byte> modelImage, Image<Bgr, Byte> observedImage, HomographyMatrix homography)
        {
            if (homography != null)
            {  //draw a rectangle along the projected model
                Rectangle rect = modelImage.ROI;
                PointF[] pts = new PointF[] { 
               new PointF(rect.Left, rect.Bottom),
               new PointF(rect.Right, rect.Bottom),
               new PointF(rect.Right, rect.Top),
               new PointF(rect.Left, rect.Top)};
                homography.ProjectPoints(pts);

                observedImage.DrawPolyline(Array.ConvertAll<PointF, Point>(pts, Point.Round), true, new Bgr(Color.Red), 2);
            }

            return observedImage;
        }

    }
}
