// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    ///     Represents the restriction of the number of elements in the Argument collection to the specified Limit value.
    /// </summary>
    public sealed class DbLimitExpression : DbExpression
    {
        private readonly DbExpression _argument;
        private readonly DbExpression _limit;
        private readonly bool _withTies;

        internal DbLimitExpression(TypeUsage resultType, DbExpression argument, DbExpression limit, bool withTies)
            : base(DbExpressionKind.Limit, resultType)
        {
            DebugCheck.NotNull(argument);
            DebugCheck.NotNull(limit);
            Debug.Assert(
                ReferenceEquals(resultType, argument.ResultType), "DbLimitExpression result type must be the result type of the argument");

            _argument = argument;
            _limit = limit;
            _withTies = withTies;
        }

        /// <summary>
        ///     Gets the expression that specifies the input collection.
        /// </summary>
        public DbExpression Argument
        {
            get { return _argument; }
        }

        /// <summary>
        ///     Gets the expression that specifies the limit on the number of elements returned from the input collection.
        /// </summary>
        public DbExpression Limit
        {
            get { return _limit; }
        }

        /// <summary>
        ///     Gets whether the limit operation will include tied results, which could produce more results than specifed by the Limit value if ties are present.
        /// </summary>
        public bool WithTies
        {
            get { return _withTies; }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType">
        ///     The type of the result produced by <paramref name="visitor" />
        /// </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null
        /// </exception>
        /// <returns>
        ///     An instance of <typeparamref name="TResultType" /> .
        /// </returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
