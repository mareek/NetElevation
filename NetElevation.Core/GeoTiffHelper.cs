using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using BitMiracle.LibTiff.Classic;

namespace NetElevation.Core
{
    internal static class GeoTiffHelper
    {
        static GeoTiffHelper()
        {
            Tiff.SetErrorHandler(new DisableErrorHandler());
            Tiff.SetTagExtender(TagExtender);
        }

        private static void TagExtender(Tiff tif)
        {
            TiffFieldInfo[] tiffFieldInfo =
            {
                new TiffFieldInfo(TiffTag.GEOTIFF_MODELPIXELSCALETAG, TiffFieldInfo.Variable, TiffFieldInfo.Variable,
                                  TiffType.DOUBLE, FieldBit.Custom, true, true, "scale"),
                new TiffFieldInfo(TiffTag.GEOTIFF_MODELTIEPOINTTAG, TiffFieldInfo.Variable, TiffFieldInfo.Variable,
                                  TiffType.DOUBLE, FieldBit.Custom, true, true, "tie"),
            };

            tif.MergeFieldInfo(tiffFieldInfo, tiffFieldInfo.Length);
        }

        public static Tiff TiffFromFile(FileInfo tiffFile)
        {
            MemoryStream memoryStream;
            if (string.Equals(tiffFile.Extension, ".zip", StringComparison.OrdinalIgnoreCase))
            {
                memoryStream = GetZippedTiffStream(tiffFile);
            }
            else if (string.Equals(tiffFile.Extension, ".tif", StringComparison.OrdinalIgnoreCase))
            {
                memoryStream = new MemoryStream();
                using var fileStream = tiffFile.OpenRead();
                fileStream.CopyTo(memoryStream);
            }
            else
            {
                throw new ArgumentException($"unkown file format {tiffFile.Extension}", nameof(tiffFile));
            }

            return TiffFromStream(memoryStream);
        }

        public static Tiff TiffFromStream(MemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            return Tiff.ClientOpen("Tiff from zipstream", "r", memoryStream, new TiffStream());
        }

        public static TileInfo GetTileInfo(Tiff tiff)
        {
            int imageWidth = tiff.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            int imageHeight = tiff.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            double[] modelTransformation = tiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG)[1].ToDoubleArray();
            double west = modelTransformation[3];
            double north = modelTransformation[4];

            FieldValue[] modelPixelScaleTag = tiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            double[] modelPixelScale = modelPixelScaleTag[1].ToDoubleArray();
            var DW = modelPixelScale[0];
            var DH = modelPixelScale[1];

            return new TileInfo(north, west, DH * imageHeight, DW * imageWidth, imageWidth, imageHeight);
        }

        public static short[] GetElevationMap(Tiff tiff)
        {
            int stripCount = tiff.NumberOfStrips();
            int stripSize = tiff.StripSize();
            var elevationMap = new short[stripCount * stripSize];
            byte[] buffer = new byte[stripSize];
            for (int stripIndex = 0; stripIndex < stripCount; stripIndex++)
            {
                tiff.ReadEncodedStrip(stripIndex, buffer, 0, -1);
                Buffer.BlockCopy(buffer, 0, elevationMap, stripIndex * stripSize, stripSize);
            }

            return elevationMap;
        }

        public static MemoryStream CreateGeoTiff(Tiff sourceTiff, string name, double north, double west, int width, int height, short[] data)
        {
            var tempFile = CreateGeoTiffFile(sourceTiff, name, north, west, width, height, data);

            var tiffStream = new MemoryStream();
            using (var fileStream = tempFile.OpenRead())
            {
                fileStream.CopyTo(tiffStream);
            }

            tempFile.Delete();
            return tiffStream;
        }

        private static FileInfo CreateGeoTiffFile(Tiff sourceTiff, string name, double north, double west, int width, int height, short[] data)
        {
            var tempTiffFilePath = Path.GetTempFileName();

            var geoTiff = Tiff.Open(tempTiffFilePath, "w");

            SetTags(sourceTiff, name, north, west, width, height, geoTiff);
            WriteGeoTiffData(width, height, data, geoTiff);

            geoTiff.Close();

            return new FileInfo(tempTiffFilePath);
        }

        private static void SetTags(Tiff sourceTiff, string name, double north, double west, int width, int height, Tiff geoTiff)
        {
            //LibTiff.net doesn't seem to be thread safe
            lock (sourceTiff)
            {
                CopyTags(sourceTiff, geoTiff);

                geoTiff.SetField(TiffTag.DOCUMENTNAME, name + ".tif");
                geoTiff.SetField(TiffTag.IMAGEWIDTH, width);
                geoTiff.SetField(TiffTag.IMAGELENGTH, height);

                SetGeoTiffTags(sourceTiff, north, west, geoTiff);
            }

            geoTiff.CheckpointDirectory();
        }

        private static void WriteGeoTiffData(int width, int height, short[] data, Tiff geoTiff)
        {
            int stripSize = width * sizeof(short);
            var rowData = new byte[stripSize];
            for (int row = 0; row < height; row++)
            {
                Buffer.BlockCopy(data, row * stripSize, rowData, 0, stripSize);
                geoTiff.WriteEncodedStrip(row, rowData, stripSize);
            }
        }

        private static void CopyTags(Tiff sourceTiff, Tiff targetTiff)
        {
            TiffTag[] tagsToIgnore = { TiffTag.STRIPBYTECOUNTS, TiffTag.STRIPOFFSETS, TiffTag.TILEBYTECOUNTS, TiffTag.TILEOFFSETS, TiffTag.DOCUMENTNAME, TiffTag.IMAGEWIDTH, TiffTag.IMAGELENGTH, TiffTag.GEOTIFF_MODELTIEPOINTTAG, TiffTag.GEOTIFF_MODELPIXELSCALETAG };
            var tagsToCopy = Enum.GetValues(typeof(TiffTag))
                                 .OfType<TiffTag>()
                                 .Where(tag => sourceTiff.GetField(tag) != null)
                                 .Except(tagsToIgnore)
                                 .ToArray();
            foreach (var tag in tagsToCopy)
            {
                targetTiff.SetField(tag, sourceTiff.GetField(tag).Select(t => t.Value).ToArray());
            }
        }

        private static void SetGeoTiffTags(Tiff sourceTiff, double north, double west, Tiff geoTiff)
        {
            var modelTiePointSource = sourceTiff.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG);
            var coordinatesParam = modelTiePointSource[1].ToDoubleArray();
            coordinatesParam[3] = west;
            coordinatesParam[4] = north;
            var modelTiePointTarget = modelTiePointSource.Select(v => v.Value).ToArray();
            modelTiePointTarget[1] = coordinatesParam;
            geoTiff.SetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG, modelTiePointTarget);

            var modelPixelScaleSource = sourceTiff.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
            var modelPixelScaleTarget = modelPixelScaleSource.Select(v => v.Value).ToArray();
            geoTiff.SetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG, modelPixelScaleTarget);
        }

        private static MemoryStream GetZippedTiffStream(FileInfo zipFile)
        {
            using var zipArchive = ZipFile.OpenRead(zipFile.FullName);
            var zippedTiff = zipArchive.Entries.Single(e => e.Name.EndsWith(".tif", StringComparison.OrdinalIgnoreCase));
            using var zippedTiffStream = zippedTiff.Open();
            var memoryStream = new MemoryStream();
            zippedTiffStream.CopyTo(memoryStream);
            return memoryStream;
        }

        private class DisableErrorHandler : TiffErrorHandler
        {
            public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
            {
                // do nothing, ie, do not write warnings to console
            }
            public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
            {
                // do nothing ie, do not write warnings to console
            }
        }
    }
}
