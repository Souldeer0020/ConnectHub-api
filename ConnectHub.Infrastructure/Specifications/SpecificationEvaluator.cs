using ConnectHub.Application.Specifications;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectHub.Infrastructure.Specifications
{
    public class SpecificationEvaluator<T> where T : class
    {
        public static IQueryable<T> GetQuery(IQueryable<T> InputQuery,Ispecification<T> spec)
        {
            var query = InputQuery;
            if(spec.Criteria!=null)
                query= query.Where(spec.Criteria);
            if(spec.Includes != null)
                query = spec.Includes.Aggregate(query, (current, include) => current.Include(include));

            if(spec.OrderBy != null)
                query=query.OrderBy(spec.OrderBy);
            else if(spec.OrderByDescending != null)
                query = query.OrderByDescending(spec.OrderByDescending);

            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);

            return query;
        }
    }
}
