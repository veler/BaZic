using BaZic.Runtime.BaZic.Code.AbstractSyntaxTree;
using System;
using System.Collections.Generic;

namespace BaZic.Runtime.BaZic.Code.Optimizer
{
    /// <summary>
    /// Represents the informations about a method required to perform an optimization on the syntax tree.
    /// </summary>
    internal sealed class MethodInformation
    {
        #region Properties

        /// <summary>
        /// Gets or sets the original method declaration.
        /// </summary>
        internal MethodDeclaration MethodDeclaration { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="VariableDeclaration"/> that represents the arguments or variables in a inline context. The guid is the ID of the previous <see cref="ParameterDeclaration"/> that must be changed in all the <see cref="VariableReferenceExpression"/>.
        /// </summary>
        internal Dictionary<Guid, VariableDeclaration> SubstituteVariableDeclarations { get; set; }

        /// <summary>
        /// Gets or sets the label that defines the beginning of the method.
        /// </summary>
        internal LabelDeclaration StartLabel { get; set; }

        /// <summary>
        /// Gets or sets the label that defines the end of the method.
        /// </summary>
        internal LabelDeclaration EndLabel { get; set; }

        /// <summary>
        /// Gets or sets the variable that must receive the returned value.
        /// </summary>
        internal VariableDeclaration ReturnValueRecepter { get; set; }

        #endregion
    }
}
