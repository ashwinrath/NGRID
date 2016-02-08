using System;

namespace NGRID.Client.NGRIDServices
{
    /// <summary>
    /// This attribute is used to add information to a parameter or return value of a NGRIDServiceMethod.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class NGRIDServiceMethodParameterAttribute : Attribute
    {
        /// <summary>
        /// A brief description of parameter.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates a new NGRIDServiceMethodParameterAttribute.
        /// </summary>
        /// <param name="description"></param>
        public NGRIDServiceMethodParameterAttribute(string description)
        {
            Description = description;
        }
    }
}
