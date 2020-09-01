using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using BitMiracle.LibTiff.Classic;

namespace NetElevation.Core
{
    public class TileRepository
    {
        private readonly DirectoryInfo _directory;

        public TileRepository(DirectoryInfo directory)
        {
            _directory = directory;
        }

        private string ConfigFilePath => Path.Combine(_directory.FullName, "tiles.json");

        public TileInfo[] GetTiles()
        {
            InitRepository();
            var fileContent = File.ReadAllBytes(ConfigFilePath);
            return JsonSerializer.Deserialize<TileInfo[]>(fileContent);
        }

        private void InitRepository()
        {
            if (!File.Exists(ConfigFilePath))
            {
                var allTiles = _directory.GetFiles("*.zip").AsParallel().Select(GetTileInfo).ToArray();
                var serializedTiles = JsonSerializer.Serialize(allTiles, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(ConfigFilePath, serializedTiles);
            }
        }

        public short[] GetElevationMap(TileInfo tileInfo)
        {
            using var tiff = GetTiff(tileInfo.FileName!);
            return GeoTiffHelper.GetElevationMap(tiff);
        }

        public Tiff GetTiff(string fileName)
        {
            var memoryStream = GetZippedTiffStream(new FileInfo(Path.Combine(_directory.FullName, fileName)));
            return GeoTiffHelper.TiffFromStream(memoryStream);
        }

        private TileInfo GetTileInfo(FileInfo zipFile)
        {
            using var tiff = GetTiff(zipFile.Name);

            TileInfo tileInfo = GeoTiffHelper.GetTileInfo(tiff);
            tileInfo.FileName = zipFile.Name;
            return tileInfo;
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
    }
}
