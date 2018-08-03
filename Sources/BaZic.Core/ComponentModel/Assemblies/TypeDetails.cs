using System;

namespace BaZic.Core.ComponentModel.Assemblies
{
    /// <summary>
    /// Provides informations about a type.
    /// </summary>
    [Serializable]
    public class TypeDetails
    {
        #region Properties

        /// <summary>
        /// Gets or sets the information about the.
        /// </summary>
        public AssemblyDetails Assembly { get; set; }

        /// <summary>
        /// Gets or sets the name of the type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the namespace of the type.
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Gets or sets whether the type is an interface.
        /// </summary>
        public bool IsInterface { get; set; }

        /// <summary>
        /// Gets or sets whether the type is a class.
        /// </summary>
        public bool IsClass { get; set; }

        /// <summary>
        /// Gets or sets whether the type is a structure.
        /// </summary>
        public bool IsValueType { get; set; }

        /// <summary>
        /// Gets or sets whether the type is public.
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets whether the type is a generic type.
        /// </summary>
        public bool IsGenericType { get; set; }

        /// <summary>
        /// Gets or sets whether the type is a generic type definition.
        /// </summary>
        public bool IsGenericTypeDefinition { get; set; }

        /// <summary>
        /// Gets or sets whether the type is static.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets whether the type is abstract.
        /// </summary>
        public bool IsAbstract { get; set; }

        #endregion
    }
}
