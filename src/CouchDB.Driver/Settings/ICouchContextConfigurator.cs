using System;

namespace CouchDB.Driver.Settings
{
    /// <summary>
    /// Configure how the database context behave
    /// </summary>
    public interface ICouchContextConfigurator : ICouchConfigurator
    {
        /// <summary>
        /// Set the database endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint URL.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchContextConfigurator UseEndpoint(string endpoint);

        /// <summary>
        /// Set the database endpoint.
        /// </summary>
        /// <param name="endpointUri">The endpoint URI.</param>
        /// <returns>The instance to chain settings.</returns>
        ICouchContextConfigurator UseEndpoint(Uri endpointUri);

        /// <summary>
        /// When creating a CouchContext, it ensures that all databases exist.
        /// </summary>
        /// <returns>The instance to chain settings.</returns>
        ICouchContextConfigurator EnsureDatabaseExists();
    }
}