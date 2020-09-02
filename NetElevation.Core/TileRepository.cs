using System.IO;
using System.Linq;
using System.Text.Json;

namespace NetElevation.Core
{
    public class TileRepository : ITileRepository
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

        public FileInfo GetFile(TileInfo tileInfo) => new FileInfo(Path.Combine(_directory.FullName, tileInfo.FileName!));

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
            using var tiff = GeoTiffHelper.TiffFromZip(GetFile(tileInfo));
            return GeoTiffHelper.GetElevationMap(tiff);
        }

        private TileInfo GetTileInfo(FileInfo zipFile)
        {
            using var tiff = GeoTiffHelper.TiffFromZip(zipFile);

            TileInfo tileInfo = GeoTiffHelper.GetTileInfo(tiff);
            tileInfo.FileName = zipFile.Name;
            return tileInfo;
        }
    }
}
