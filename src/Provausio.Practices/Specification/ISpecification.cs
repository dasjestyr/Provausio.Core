using System;
using System.Threading.Tasks;

namespace Provausio.Practices.Specification
{
    public interface ISpecification<T>
    {
        bool IsSatisfiedBy(T target);

        Task<bool> IsSatisfiedByAsync(T target);

        ISpecification<T> And(ISpecification<T> specification);

        ISpecification<T> Or(ISpecification<T> specification);

        ISpecification<T> Not(ISpecification<T> specification);

        ISpecification<T> AndNot(ISpecification<T> right);

        ISpecification<T> OrNot(ISpecification<T> right);

        /// <summary>
        /// Runs IsSatisfiedBy() and executes action if IsSatisfied() returns true.
        /// </summary>
        /// <param name="target">The target object that will be supplied to IsSatisfied().</param>
        /// <param name="action">The action that will be run if IsSatisfied() returns true.</param>
        /// <param name="invert">If true, action will execute when IsSatisfied() returns false.</param>
        bool EvalAndExecute(T target, Action action, bool invert = false);

        /// <summary>
        /// Runs IsSatisfiedBy() and executes action if IsSatisfied() returns true.
        /// </summary>
        /// <param name="target">The target object that will be supplied to IsSatisfied().</param>
        /// <param name="action">The action that will be run if IsSatisfied() returns true.</param>
        /// <param name="invert">If true, action will execute when IsSatisfied() returns false.</param>
        Task<bool> EvalAndExecuteAsync(T target, Func<Task> action, bool invert = false);
    }
}