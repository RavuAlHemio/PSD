using System;

namespace RavuAlHemio.PSD
{
    /// <summary>
    /// Thrown if the rules of the PSD/PSB file format have been violated.
    /// </summary>
    public class PSDFormatException : Exception
    {
        public PSDFormatException()
            : base()
        {
        }

        public PSDFormatException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
