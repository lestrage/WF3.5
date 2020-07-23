using Autofac;
using WF.Business.DataAccess;

namespace WF.Web.ServiceLocation
{
    public class DataServiceProvider : IDataServiceProvider
    {
        private readonly IContainer _container;

        public DataServiceProvider(IContainer container)
        {
            _container = container;
        }

        public T Get<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
