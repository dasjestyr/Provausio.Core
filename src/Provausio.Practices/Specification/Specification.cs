using System;
using System.Threading;
using System.Threading.Tasks;

namespace Provausio.Practices.Specification
{
    namespace Provausio.Practices.Patterns
    {
        public abstract class Specification<T> : ISpecification<T>
        {
            public abstract bool IsSatisfiedBy(T target);

            public virtual Task<bool> IsSatisfiedByAsync(T target)
            {
                return IsSatisfiedByAsync(target, CancellationToken.None);
            }

            public virtual async Task<bool> IsSatisfiedByAsync(T target, CancellationToken cancellationToken)
            {
                return await Task.Run(() => IsSatisfiedBy(target), cancellationToken).ConfigureAwait(false);
            }

            public ISpecification<T> And(ISpecification<T> specification)
            {
                return new AndSpecification<T>(this, specification);
            }

            public ISpecification<T> Or(ISpecification<T> specification)
            {
                return new OrSpecification<T>(this, specification);
            }

            public ISpecification<T> Not(ISpecification<T> specification)
            {
                return new NotSpecification<T>(this);
            }

            public ISpecification<T> AndNot(ISpecification<T> specification)
            {
                return new AndNotSpecification<T>(this, specification);
            }

            public ISpecification<T> OrNot(ISpecification<T> specification)
            {
                return new OrNotSpecification<T>(this, specification);
            }
            
            public virtual bool EvalAndExecute(T target, Action action, bool invert = false)
            {
                var isSatisfied = IsSatisfiedBy(target);
                if (invert)
                {
                    if (!isSatisfied) action();
                }
                else
                {
                    if (isSatisfied) action();
                }

                return isSatisfied;
            }
            
            public virtual async Task<bool> EvalAndExecuteAsync(T target, Func<Task> action, bool invert = false)
            {
                var isSatisfied = await IsSatisfiedByAsync(target).ConfigureAwait(false);
                if (invert)
                {
                    if (!isSatisfied) await action().ConfigureAwait(false);
                }
                else
                {
                    if (isSatisfied) await action().ConfigureAwait(false);
                }

                return isSatisfied;
            }
        }
    }
}
