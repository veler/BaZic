using System;
using System.Windows;

namespace BaZic.Core.ComponentModel
{
    /// <summary>
    /// Provides a helper to browse the visual tree.
    /// </summary>
    internal static class VisualTreeHelperExtension
    {
        /// <summary>
        /// Performs the specified action for each <see cref="FrameworkElement"/> discovered in the logical tree of a given component.
        /// </summary>
        /// <param name="root">The root component where the search must start.</param>
        /// <param name="action">The action to perform for each component discovered, including the root.</param>
        internal static void ProcessLogicalTree(FrameworkElement root, Action<FrameworkElement> action)
        {
            Requires.NotNull(root, nameof(root));
            Requires.NotNull(action, nameof(action));

            action(root);

            var children = LogicalTreeHelper.GetChildren(root);
            foreach (var child in children)
            {
                var castedChild = child as FrameworkElement;
                if (castedChild != null)
                {
                    ProcessLogicalTree(castedChild, action);
                }
            }
        }
    }
}
