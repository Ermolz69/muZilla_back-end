namespace muZilla.Services
{
    public class Filter<T>
    {
        private Func<T, bool> _filter;
        private Filter<T>? _from_filter;
        public Filter(Func<T, bool> filter, Filter<T>? from_filter=null)
        {
            _filter = filter;
            _from_filter = from_filter;
        }
        public List<T> GetFiltered(List<T> objects)
        {
            if (_from_filter == null)
                return objects.Where(_filter).ToList();
            else
                return _from_filter.GetFiltered(objects).Where(_filter).ToList();
        }
    }
}