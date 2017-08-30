using System;
using System.Collections.Generic;

namespace Provausio.Core
{
    public class DisposableActionCollection : IDisposable
    {
        private readonly List<Action> _disposeActions = new List<Action>();

        public void Add(Action disposeAction)
        {
            _disposeActions.Add(disposeAction);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            foreach (var action in _disposeActions)
            {
                action();
            }
        }
    }

    public class DisposableObjectCollection : IDisposable
    {
        private readonly List<IDisposable>  _disposables = new List<IDisposable>();

        public void Add(IDisposable disposable)
        {
            if(!_disposables.Contains(disposable))
                _disposables.Add(disposable);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            foreach (var disposable in _disposables)
                disposable.Dispose();
        }
    }
}
