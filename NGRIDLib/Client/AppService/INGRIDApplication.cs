namespace NGRID.Client.AppService
{
    /// <summary>
    /// Represents a NGRID Application from NGRIDMessageProcessor perspective.
    /// This class also provides a static collection that can be used as a cache,
    /// thus, an NGRIDMessageProcessor/NGRIDClientApplicationBase can store/get application-wide objects.
    /// </summary>
    public interface INGRIDApplication
    {
        /// <summary>
        /// Name of the application.
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        /// Gets/Sets application-wide object by a string key.
        /// </summary>
        /// <param name="key">Key of the object to access it</param>
        /// <returns>Object, that is related with given key</returns>
        object this[string key] { get; set; }
    }
}
