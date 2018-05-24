using System.Collections;
using System.Data;

namespace FrameWork
{
    public class DataTableHandler
    {
        private readonly Hashtable _precache;
        private bool _hasRelations;

        public DataTableHandler(DataSet dataSet)
        {
            Cache = new SimpleCache();
            _precache = new Hashtable();
            DataSet = dataSet;
            _hasRelations = false;
        }

        public bool HasRelations
        {
            get { return _hasRelations; }
            set { _hasRelations = false; }
        }

        public ICache Cache { get; }

        public DataSet DataSet { get; }

        public bool UsesPreCaching { get; set; }

        public bool RequiresObjectId { get; set; }

        public void SetCacheObject(object key, DataObject obj)
        {
            Cache[key] = obj;
        }

        public DataObject GetCacheObject(object key)
        {
            return Cache[key] as DataObject;
        }

        public void SetPreCachedObject(object key, DataObject obj)
        {
            _precache[key] = obj;
        }

        public DataObject GetPreCachedObject(object key)
        {
            return _precache[key] as DataObject;
        }


        // Method of loading items from this table
        public EBindingMethod BindingMethod { get; set; }
    }
}