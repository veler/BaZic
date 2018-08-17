using BaZic.Core.ComponentModel.Assemblies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BaZic.Runtime.BaZic.Code.AbstractSyntaxTree
{
    /// <summary>
    /// Represents the root of a BaZic program
    /// </summary>
    [Serializable]
    public class BaZicProgram : NodeObject
    {
        #region Properties

        /// <summary>
        /// Gets the list of global variables.
        /// </summary>
        public IReadOnlyList<VariableDeclaration> GlobalVariables { get; private set; }

        /// <summary>
        /// Gets the list of methods.
        /// </summary>
        public IReadOnlyList<MethodDeclaration> Methods { get; private set; }

        /// <summary>
        /// Gets the list of required assemblies to interpret or build the program.
        /// </summary>
        public IReadOnlyList<AssemblyDetails> Assemblies { get; private set; }

        /// <summary>
        /// Gets whether the program passed into the optimizer.
        /// </summary>
        public bool IsOptimized { get; }

        #endregion

        #region Constructors & Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicProgram"/> class.
        /// </summary>
        public BaZicProgram()
        {
            GlobalVariables = new List<VariableDeclaration>().AsReadOnly();
            Methods = new List<MethodDeclaration>().AsReadOnly();
            Assemblies = new List<AssemblyDetails>().AsReadOnly();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaZicProgram"/> class.
        /// </summary>
        /// <param name="isOptimized">Defines whether the program has been optimized.</param>
        public BaZicProgram(bool isOptimized)
            : this()
        {
            IsOptimized = isOptimized;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set program global variables.
        /// </summary>
        /// <param name="variables">The variables</param>
        /// <returns>The current program</returns>
        public BaZicProgram WithVariables(params VariableDeclaration[] variables)
        {
            GlobalVariables = new List<VariableDeclaration>(variables).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set program methods.
        /// </summary>
        /// <param name="methods">The methods</param>
        /// <returns>The current program</returns>
        public BaZicProgram WithMethods(params MethodDeclaration[] methods)
        {
            Methods = new List<MethodDeclaration>(methods).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set program required assemblies.
        /// </summary>
        /// <param name="assemblies">The fullname or path to assemblies</param>
        /// <returns>The current program</returns>
        public BaZicProgram WithAssemblies(params string[] assemblies)
        {
            var assembliesDetails = new List<AssemblyDetails>();

            foreach (var assembly in assemblies)
            {
                assembliesDetails.Add(AssemblyDetails.GetAssemblyDetailsFromName(assembly));
            }

            Assemblies = assembliesDetails.AsReadOnly();
            return this;
        }

        /// <summary>
        /// Set program required assemblies.
        /// </summary>
        /// <param name="assemblies">The assembly details</param>
        /// <returns>The current program</returns>
        public BaZicProgram WithAssemblies(params AssemblyDetails[] assemblies)
        {
            Assemblies = new List<AssemblyDetails>(assemblies).AsReadOnly();
            return this;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public override object Clone()
        {
            return new BaZicProgram(IsOptimized)
            {
                Column = Column,
                Id = Id,
                Line = Line,
                StartOffset = StartOffset,
                NodeLength = NodeLength
            }
            .WithMethods(Methods.ToArray())
            .WithVariables(GlobalVariables.ToArray())
            .WithAssemblies(Assemblies.ToArray());
        }

        #endregion
    }
}
