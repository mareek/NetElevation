using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace NetElevation.Core
{
    public class ZipRepository : ITileRepository, IDisposable
    {
        private readonly ZipArchive _repositoryArchive;

        public ZipRepository(string repositoryFilePath)
        {
            _repositoryArchive = ZipFile.OpenRead(repositoryFilePath);
        }

        public TileInfo[] GetTiles()
        {
            using var memoryStream = GetFileStream("tiles.json");
            return JsonSerializer.Deserialize<TileInfo[]>(memoryStream.ToArray());
        }

        public short[] LoadElevationMap(TileInfo tileInfo)
        {
            MemoryStream memoryStream;
            if (tileInfo.FileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            {
                memoryStream = GeoTiffHelper.GetZippedTiffStream(GetFileStream(tileInfo.FileName));
            }
            else
            {
                memoryStream = GetFileStream(tileInfo.FileName);
            }
            using var tiff = GeoTiffHelper.TiffFromStream(memoryStream);
            return GeoTiffHelper.GetElevationMap(tiff);
        }

        private MemoryStream GetFileStream(string fileName)
        {
            var tileInfoEntry = _repositoryArchive.Entries.First(e => e.Name == fileName);
            using var zipStream = tileInfoEntry.Open();
            var memoryStream = new MemoryStream();
            zipStream.CopyTo(memoryStream);
            return memoryStream;
        }

        public void Dispose() => _repositoryArchive?.Dispose();
    }
}
