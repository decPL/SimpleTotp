using System;
using System.Linq.Expressions;

namespace SimpleTotp
{
    /// <summary>
    /// Internal class for easier param validation
    /// </summary>
    internal static class ValidationHelper
    {
        /// <summary>
        /// Checks if the provided expression results in a null or whitespace string. Throws an ArgumentException if it does.
        /// </summary>
        /// <param name="expression">Expression to check against a null/whitespace string</param>
        public static void CheckNotNullOrWhitespace(Expression<Func<String>> expression)
        {
            if (!String.IsNullOrWhiteSpace(expression.Compile().Invoke()))
                return;

            var memberName = (expression.Body as MemberExpression)?.Member.Name;
            if (String.IsNullOrWhiteSpace(memberName))
                throw new Exception($"Incorrect use of the {nameof(CheckNotNullOrWhitespace)} method - must be a member expression");

            throw new ArgumentException($"Provided {memberName} is empty", memberName);
        }
    }
}