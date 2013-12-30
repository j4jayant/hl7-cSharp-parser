using System;

namespace j4jayant.HL7.Parser
{
    [Serializable]
    public class HL7Exception : Exception
    {
        public String ErrorCode { get; set; }

        public HL7Exception(string message)
            : base(message)
        {
        }

        public HL7Exception(string message, String Code)
            : base(message)
        {
            ErrorCode = Code;
        }

        public override string ToString()
        {
            return ErrorCode + " : " + Message;
        }

        public const String REQUIRED_FIELD_MISSING = "Validation Error - Required field missing in message";
        public const String UNSUPPORTED_MESSAGE_TYPE = "Validation Error - Message Type Not Supported by this Implementation";
        public const String BAD_MESSAGE = "Validation Error - Bad Message";
        public const String PARSING_ERROR = "Parseing Error";
        public const String SERIALIZATION_ERROR = "Serialization Error";
        
    }
}