using System;

namespace NGRID.Client.NGRIDServices
{
    /// <summary>
    /// Any NGRIDService class must use this attribute on it's remote methods.
    /// If a method has not NGRIDServiceMethod attribute, it can not be invoked by remote applications.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class NGRIDServiceMethodAttribute : Attribute
    {
        /// <summary>
        /// A brief description (and may be usage) of method.
        /// </summary>
        public string Description { get; set; }
    }
}
