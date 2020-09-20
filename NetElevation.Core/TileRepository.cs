using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace NetElevation.Core
{
    public class TileRepository : ITileRepository
    {
        private readonly DirectoryInfo _directory;

        public TileRepository(string directoryPath) : this(new DirectoryInfo(directoryPath)) { }

        public TileRepository(DirectoryInfo directory) => _directory = directory;

        private string ConfigFilePath => Path.Combine(_directory.FullName, "tiles.json");

        public TileInfo[] GetTiles()
        {
            InitRepository(false);
            return LoadConfigFile();
        }

        private TileInfo[] LoadConfigFile()
            => JsonSerializer.Deserialize<TileInfo[]>(File.ReadAllBytes(ConfigFilePath));

        public FileInfo GetFile(TileInfo tileInfo) => new FileInfo(Path.Combine(_directory.FullName, tileInfo.FileName!));

        public void InitRepository(bool force)
        {
            bool configFileExists = File.Exists(ConfigFilePath);
            if (configFileExists && !force)
                return;

            var existingTiles = new HashSet<string>(configFileExists ? LoadConfigFile().Select(t => t.FileName) : Enumerable.Empty<string>());
            var zipFiles = _directory.EnumerateFiles("*.zip");
            var tiffFiles = _directory.EnumerateFiles("*.tif");
            var allTiles = zipFiles.Concat(tiffFiles)
                                   .Where(f => !existingTiles.Contains(f.Name))
                                   .ToArray()
                                   .AsParallel()
                                   .Select(GetTileInfo)
                                   .ToArray();
            var serializedTiles = JsonSerializer.Serialize(allTiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, serializedTiles);
        }

        public short[] LoadElevationMap(TileInfo tileInfo)
        {
            using var tiff = GeoTiffHelper.TiffFromFile(GetFile(tileInfo));
            return GeoTiffHelper.GetElevationMap(tiff);
        }

        private TileInfo GetTileInfo(FileInfo zipFile)
        {
            using var tiff = GeoTiffHelper.TiffFromFile(zipFile);
            return GeoTiffHelper.GetTileInfo(tiff, zipFile.Name);
        }
    }
}
