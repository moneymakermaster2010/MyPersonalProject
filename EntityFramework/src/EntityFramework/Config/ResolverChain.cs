// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Config
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>
    ///     Chain-of-Responsibility implementation for <see cref="IDbDependencyResolver" /> instances.
    /// </summary>
    internal class ResolverChain : IDbDependencyResolver
    {
        // DbConfiguration depends on this class being thread safe
        private readonly ConcurrentStack<IDbDependencyResolver> _resolvers = new ConcurrentStack<IDbDependencyResolver>();
        private volatile IDbDependencyResolver[] _resolversSnapshot = new IDbDependencyResolver[0];

        public virtual void Add(IDbDependencyResolver resolver)
        {
            DebugCheck.NotNull(resolver);

            // The idea here is that Add and GetService must all be thread-safe, but
            // Add is only called infrequently. Therefore each time Add is called a snapshot is taken
            // of the stack that can then be enumerated without needing to make a snapshot
            // every time the enumeration is asked for, which is the normal behavior for the concurrent
            // collections.
            _resolvers.Push(resolver);
            _resolversSnapshot = _resolvers.ToArray();
        }

        public virtual IEnumerable<IDbDependencyResolver> Resolvers
        {
            get { return _resolversSnapshot.Reverse(); }
        }

        public virtual object GetService(Type type, object key)
        {
            return _resolversSnapshot
                .Select(r => r.GetService(type, key))
                .FirstOrDefault(s => s != null);
        }
    }
}
