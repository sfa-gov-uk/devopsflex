﻿namespace DevOpsFlex.Data.Naming
{
    /// <summary>
    /// Names <see cref="DevOpsComponent"/> so that Azure resource names can be generated.
    /// </summary>
    /// <typeparam name="T">The type of the component that we are naming.</typeparam>
    public interface IName<in T>
        where T : DevOpsComponent
    {
        /// <summary>
        /// Slot name for medium to long names.
        /// </summary>
        /// <param name="component">The component that we want the generate the slot name for.</param>
        /// <returns>The slot name.</returns>
        string GetSlotName(T component);
    }
}
