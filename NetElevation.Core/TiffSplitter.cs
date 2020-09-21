using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace NetElevation.Core
{
    internal class TiffSplitter
    {
        private readonly TileRepository _sourceRepository;

        public TiffSplitter(DirectoryInfo sourceDirectory)
        {
            _sourceRepository = new TileRepository(sourceDirectory);
        }

        public void SplitTiles(DirectoryInfo targetDirectory)
        {
            foreach (var tileInfo in _sourceRepository.GetTiles())
            {
                SplitTile(tileInfo, 1.0, 1.0, targetDirectory);
            }
        }

        private void SplitTile(TileInfo tileInfo, double latitudeSpan, double longitudeSpan, DirectoryInfo targetDirectory)
        {
            var latitudeFactor = tileInfo.LatitudeSpan / latitudeSpan;
            if (latitudeFactor <= 1.0 || latitudeFactor % 1.0 != 0)
                throw new ArgumentException("latitudeFactor must be an integer", nameof(latitudeSpan));

            var longitudeFactor = tileInfo.LongitudeSpan / longitudeSpan;
            if (longitudeFactor <= 1.0 || latitudeFactor % 1.0 != 0)
                throw new ArgumentException("longitudeFactor must be an integer", nameof(longitudeSpan));

            var subtilesInfo = GetSubTileInfo(tileInfo, latitudeSpan, longitudeSpan, targetDirectory).ToArray();
            if (subtilesInfo.Any())
            {
                var height = (int)(tileInfo.Height / latitudeFactor);
                var width = (int)(tileInfo.Width / longitudeFactor);

                CreateSubtiles(tileInfo, targetDirectory, height, width, subtilesInfo);
            }
        }

        private static IEnumerable<SubTileInfo> GetSubTileInfo(TileInfo tileInfo, double latitudeSpan, double longitudeSpan, DirectoryInfo targetDirectory)
        {
            for (double north = tileInfo.North; north > tileInfo.South; north -= latitudeSpan)
            {
                for (double west = tileInfo.West; west < tileInfo.East; west += longitudeSpan)
                {
                    var subTileName = GetTileName(north, west, latitudeSpan, longitudeSpan);
                    if (!targetDirectory.EnumerateFiles(subTileName + ".zip").Any())
                    {
                        yield return new SubTileInfo(north, west, subTileName);
                    }
                }
            }
        }

        private void CreateSubtiles(TileInfo tileInfo, DirectoryInfo targetDirectory, int height, int width, SubTileInfo[] subtilesInfo)
        {
            using var tiff = GeoTiffHelper.TiffFromFile(_sourceRepository.GetFile(tileInfo));

            short[] elevationMap = GeoTiffHelper.GetElevationMap(tiff);

            subtilesInfo.AsParallel()
                        .WithDegreeOfParallelism(4)
                        .ForAll(CreateSubtileZip);

            void CreateSubtileZip(SubTileInfo subTileInfo)
            {
                var subTileData = GetSubTileData(tileInfo, subTileInfo, width, height, elevationMap);
                if (subTileData.Any(v => v != 0))
                {
                    var (north, west, subTileName) = (subTileInfo.North, subTileInfo.West, subTileInfo.SubtileName);
                    using var subTileTiffStream = GeoTiffHelper.CreateGeoTiff(tiff, subTileName, north, west, width, height, subTileData);
                    SaveTile(targetDirectory, subTileName, subTileTiffStream);
                }
            }
        }

        private static string GetTileName(double north, double west, double latitudeSpan, double longitudeSpan)
        {
            static string formatLatitude(double latitude) => $"{(latitude >= 0 ? "N" : "S")}{Math.Abs(latitude):00}";
            static string formatLongitude(double longitude) => $"{(longitude >= 0 ? "E" : "W")}{Math.Abs(longitude):000}";

            var south = north - latitudeSpan;
            var east = west + longitudeSpan;
            return $"{formatLatitude(north)}{formatLongitude(west)}-{formatLatitude(south)}{formatLongitude(east)}";
        }

        private static short[] GetSubTileData(TileInfo tileInfo, SubTileInfo subTileInfo, int width, int height, short[] elevationMap)
        {
            int yOffset = (int)(tileInfo.Height * (tileInfo.North - subTileInfo.North) / tileInfo.LatitudeSpan);
            int xOffset = (int)(tileInfo.Width * (subTileInfo.West - tileInfo.West) / tileInfo.LongitudeSpan);
            var result = new short[width * height];
            for (int row = 0; row < height; row++)
            {
                int sourceindex = xOffset + (yOffset + row) * tileInfo.Width;
                int destinationIndex = row * width;
                Array.Copy(elevationMap, sourceindex, result, destinationIndex, width);
            }

            return result;
        }

        private static void SaveTile(DirectoryInfo directory, string tileName, Stream tiffStream)
        {
            var zipFilePath = Path.Combine(directory.FullName, tileName + ".zip");
            using var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
            var tiffZipEntry = zipArchive.CreateEntry(tileName + ".tif", CompressionLevel.Optimal);
            using var zipEntryStream = tiffZipEntry.Open();
            tiffStream.Position = 0;
            tiffStream.CopyTo(zipEntryStream);
        }

        private class SubTileInfo
        {
            public SubTileInfo(double north, double west, string subtileName)
            {
                North = north;
                West = west;
                SubtileName = subtileName;
            }

            public double North { get; }
            public double West { get; }
            public string SubtileName { get; }
        }
    }
}
