using System;
using System.Linq.Expressions;

namespace SimpleInjector.Advanced.Core
{
    internal sealed class DependencyContextRewriter : ExpressionVisitor
    {
        public Type DependencyServiceType { get; set; }

        public Type ServiceType { get; set; }

        public object ContextBasedFactory { get; set; }

        public object RootFactory { get; set; }

        public Expression Expression { get; set; }

        public Type ImplementationType
        {
            get
            {
                var expression = this.Expression as NewExpression;

                if (expression != null)
                {
                    return expression.Constructor.DeclaringType;
                }

                return this.ServiceType;
            }
        }

        protected override Expression VisitInvocation(
            InvocationExpression node)
        {
            if (!this.IsRootedContextBasedFactory(node))
            {
                return base.VisitInvocation(node);
            }

            return Expression.Invoke(
                Expression.Constant(this.ContextBasedFactory),
                Expression.Constant(
                    new DependencyContext(
                        this.ServiceType,
                        this.ImplementationType,
                        this.DependencyServiceType)));
        }

        private bool IsRootedContextBasedFactory(
            InvocationExpression node)
        {
            var expression =
                node.Expression as ConstantExpression;

            if (expression == null)
            {
                return false;
            }

            return object.ReferenceEquals(expression.Value,
                this.RootFactory);
        }
    }
}