using Provausio.Practices.Specification.Provausio.Practices.Patterns;

namespace Provausio.Practices.Specification
{
    public class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _specification;

        public NotSpecification(ISpecification<T> specification)
        {
            _specification = specification;
        }

        public override bool IsSatisfiedBy(T target)
        {
            return !_specification.IsSatisfiedBy(target);
        }
    }
}