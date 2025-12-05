// CustomerCrudProvider.cs
using ExportOrderEntites.ImportDocument;
using ExportOrderWebServer.Areas.ImportDocument.Provider;
using ExportOrderWebServer.Areas.ImportDocument.Repositories;
using System.Linq.Expressions;

namespace ExportOrderWebServer.Areas.ImportDocument.Customer.Provider
{
    public class CustomerCrudProvider : BaseCrudProvider<CustomerBaseEntity>
    {
        public CustomerCrudProvider(
            IRepository<CustomerBaseEntity> repository,
            IHttpContextAccessor httpContextAccessor)
            : base(repository, httpContextAccessor)
        {
        }

        protected override Expression<Func<CustomerBaseEntity, bool>>? CreateSearchFilter(string searchTerm)
        {
            return e => e.FullName.Contains(searchTerm) ||
                       e.Code.Contains(searchTerm) ||
                       e.Name.Contains(searchTerm) ||
                       e.Address.Contains(searchTerm) ||
                       (e.NameRu != null && e.NameRu.Contains(searchTerm)) ||
                       (e.AddressRu != null && e.AddressRu.Contains(searchTerm));
        }

        protected override Func<IQueryable<CustomerBaseEntity>, IOrderedQueryable<CustomerBaseEntity>>?
            CreateOrderBy(string? sortBy, bool sortDesc)
        {
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                return sortBy switch
                {
                    "FullName" => sortDesc ?
                        q => q.OrderByDescending(e => e.FullName) :
                        q => q.OrderBy(e => e.FullName),
                    "Code" => sortDesc ?
                        q => q.OrderByDescending(e => e.Code) :
                        q => q.OrderBy(e => e.Code),
                    "Name" => sortDesc ?
                        q => q.OrderByDescending(e => e.Name) :
                        q => q.OrderBy(e => e.Name),
                    "NameRu" => sortDesc ?
                        q => q.OrderByDescending(e => e.NameRu) :
                        q => q.OrderBy(e => e.NameRu),
                    "DisplayName" => sortDesc ?
                        q => q.OrderByDescending(e => e.NameRu ?? e.Name) :
                        q => q.OrderBy(e => e.NameRu ?? e.Name),
                    _ => base.CreateOrderBy(sortBy, sortDesc)
                };
            }

            return base.CreateOrderBy(sortBy, sortDesc);
        }

        public override async Task<CustomerBaseEntity> CreateAsync(CustomerBaseEntity entity, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(entity.FullName))
                throw new ArgumentException("Full name is required");

            if (string.IsNullOrWhiteSpace(entity.Code))
                throw new ArgumentException("Customer code is required");

            if (string.IsNullOrWhiteSpace(entity.Name))
                throw new ArgumentException("Name is required");

            if (string.IsNullOrWhiteSpace(entity.Address))
                throw new ArgumentException("Address is required");

            return await base.CreateAsync(entity, cancellationToken);
        }
    }
}