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

            var existingTiles = configFileExists ? LoadConfigFile() : new TileInfo[0];
            var existingTileNames = new HashSet<string>(existingTiles.Select(t => t.FileName));
            var zipFiles = _directory.EnumerateFiles("*.zip");
            var tiffFiles = _directory.EnumerateFiles("*.tif");
            var allTiles = zipFiles.Concat(tiffFiles)
                                   .Where(f => !existingTileNames.Contains(f.Name))
                                   .ToArray()
                                   .AsParallel()
                                   .Select(GetTileInfo)
                                   .Concat(existingTiles.AsParallel())
                                   .OrderByDescending(t => t.North)
                                   .ThenBy(t => t.West)
                                   .ToArray();
            var serializedTiles = JsonSerializer.Serialize(allTiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, serializedTiles);
        }

        public short[] LoadElevationMap(TileInfo tileInfo)
        {
            using var tiff = GeoTiffHelper.TiffFromFile(GetFile(tileInfo));
            return GeoTiffHelper.GetElevationMap(tiff);
        }

        private TileInfo GetTileInfo(FileInfo tileFile)
        {
            using var tiff = GeoTiffHelper.TiffFromFile(tileFile);
            return GeoTiffHelper.GetTileInfo(tiff, tileFile.Name);
        }
    }
}
