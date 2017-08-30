using Provausio.Practices.Specification.Provausio.Practices.Patterns;

namespace Provausio.Practices.Specification
{
    public class AndSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> _left;
        private readonly ISpecification<T> _right;

        public AndSpecification(ISpecification<T> left, ISpecification<T> right)
        {
            _left = left;
            _right = right;
        }

        public override bool IsSatisfiedBy(T target)
        {
            return _left.IsSatisfiedBy(target)
                   && _right.IsSatisfiedBy(target);
        }
    }
}