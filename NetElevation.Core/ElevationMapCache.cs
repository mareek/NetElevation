namespace NetElevation.Core
{
    public class ElevationMapCache : BaseCache<TileInfo, short[]>
    {
        private readonly ITileRepository _repository;

        public ElevationMapCache(ITileRepository repository, int maxCacheSize)
            : base(maxCacheSize)
        {
            _repository = repository;
        }

        protected override short[] LoadValue(TileInfo key) => _repository.LoadElevationMap(key);

        protected override int GetSize(short[] value) => sizeof(short) * value.Length;
    }
}
