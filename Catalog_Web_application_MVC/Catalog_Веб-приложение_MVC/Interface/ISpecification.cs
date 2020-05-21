using System;
using System.Linq.Expressions;

namespace Catalog_Веб_приложение_MVC.Interface
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T obj);

        Expression<Func<T, bool>> ToExpression();
    }
}
