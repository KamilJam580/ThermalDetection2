﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThermalOperations
{
    public static class DataConverting
    {
        /*private static readonly log4net.ILog log = 
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);*/
        public static List<int[,]> RawDataToArray(List<string> raw, int height, int width)
        {
            //log.Info("Images in file: " + raw.Count);
            List<int[,]> temperatureData = DeclareTemperatureData(raw.Count);
            bool exception = false;
            Parallel.For(0, raw.Count, (i, loopstate) =>
            {
                String[] rows = raw[i].Split(' ');
                int index = 0;
                for (int x = 0; x < 120; x++)
                {
                    for (int y = 0; y < 160; y++)
                    {
                        try
                        {
                            temperatureData[i][x, y] = int.Parse(rows[index]);
                            index++;
                        }
                        catch (Exception ex)
                        {
                            exception = true;
                            WriteExceptionInfos(ex);
                            break;
                        }
                        if (exception) break;
                    }
                    if (exception) break;
                }
                if (exception) loopstate.Stop();
            }
            );
            if (exception) return null;
            return temperatureData;
        }

        static private List<int[,]> DeclareTemperatureData(int count)
        {
            List<int[,]> temperatureData = new List<int[,]>();
            for (int i = 0; i < count; i++)
            {
                temperatureData.Add(new int[120, 160]);
            }
            return temperatureData;
        }
        static private void WriteExceptionInfos(Exception ex)
        {
            Trace.WriteLine("");
            Trace.WriteLine("Exception");
            Trace.WriteLine("Source: " + ex.Source);
            Trace.WriteLine("Target: " + ex.TargetSite);
            Trace.WriteLine("Inner: " + ex.InnerException);
            Trace.WriteLine("Message: " + ex.Message);
            Trace.WriteLine("HResult " + ex.HResult);
            Trace.WriteLine("Stack: " + ex.StackTrace);
            Trace.WriteLine("");
        }

        static public List<Emgu.CV.UMat> CreateThermalImages(List<Emgu.CV.Matrix<int>> intMatrices,double min,double max)
        {
            try
            {
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, 160, 120);
                if (intMatrices != null)
                {
                    Emgu.CV.CvInvoke.UseOpenCL = false;
                    List<Emgu.CV.UMat> ThermalImage = declareThermalImage(intMatrices.Count);


                    Parallel.For(0, intMatrices.Count, i =>
                    {
                        Emgu.CV.CvInvoke.UseOpenCL = false;
                        Emgu.CV.UMat ColorImg = new Emgu.CV.UMat();
                        Emgu.CV.UMat UMatFloatImg;
                        Emgu.CV.UMat UMatFloatImg2 = new Emgu.CV.UMat();

                        intMatrices[i] = (intMatrices[i] - min);
                        UMatFloatImg = new Emgu.CV.UMat(intMatrices[i].Clone().ToUMat(), rect);
                        UMatFloatImg.ConvertTo(UMatFloatImg, Emgu.CV.CvEnum.DepthType.Cv8U, (1 / max) * 255);
                        Emgu.CV.CvInvoke.ApplyColorMap(UMatFloatImg, UMatFloatImg2, Emgu.CV.CvEnum.ColorMapType.Hot);
                        ThermalImage[i] = UMatFloatImg2.Clone();
                    });
                    return ThermalImage;
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
 
        }
        static private List<Emgu.CV.UMat> declareThermalImage(int Count)
        {
            List<Emgu.CV.UMat> ThermalImages = new List<Emgu.CV.UMat>();
            for (int i = 0; i < Count; i++)
            {
                ThermalImages.Add(new Emgu.CV.UMat());
            }
            return ThermalImages;
        }
        
        static public List<Emgu.CV.Matrix<int>> ScaleIntensity(List<int[,]> temperatureDatas, out double min, out double max)
        {
            List<Emgu.CV.Matrix<int>> intMatrices = new List<Emgu.CV.Matrix<int>>();
            List<double> minTemps = new List<double>();
            List<double> maxTemps = new List<double>();
            for (int i = 0; i < temperatureDatas.Count; i++)
            {
                intMatrices.Add(new Emgu.CV.Matrix<int>(new System.Drawing.Size(160, 120)));
                minTemps.Add(new double());
                maxTemps.Add(new double());
            }
            Parallel.For(0, temperatureDatas.Count, i =>
            {
                intMatrices[i] = new Emgu.CV.Matrix<int>(temperatureDatas[i]);
                double minT;
                double maxT;
                System.Drawing.Point minTPoint;
                System.Drawing.Point maxTPoint;
                intMatrices[i].MinMax(out minT, out maxT, out minTPoint, out maxTPoint);
                minTemps[i] = minT;
                maxTemps[i] = maxT;

            });
            min = ((from l in minTemps select l).Min());
            max = ((from l in maxTemps select l).Max());
            max = max - ((max - min) * 0.75);
            return intMatrices;
        }

    }
}
