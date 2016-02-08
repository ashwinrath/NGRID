using NGRID.Exceptions;

namespace NGRID.Communication.Messages
{
    public static class NGRIDMessageFactory
    {
        public const int MessageTypeIdNGRIDDataTransferMessage = 1;
        public const int MessageTypeIdNGRIDOperationResultMessage = 2;
        public const int MessageTypeIdNGRIDPingMessage = 3;
        public const int MessageTypeIdNGRIDRegisterMessage = 4;
        public const int MessageTypeIdNGRIDChangeCommunicationWayMessage = 5;
        public const int MessageTypeIdNGRIDControllerMessage = 6;
        public const int MessageTypeIdNGRIDDataTransferResponseMessage = 7;

        public static NGRIDMessage CreateMessageByTypeId(int messageTypeId)
        {
            switch (messageTypeId)
            {
                case MessageTypeIdNGRIDDataTransferMessage:
                    return new NGRIDDataTransferMessage();
                case MessageTypeIdNGRIDOperationResultMessage:
                    return new NGRIDOperationResultMessage();
                case MessageTypeIdNGRIDPingMessage:
                    return new NGRIDPingMessage();
                case MessageTypeIdNGRIDRegisterMessage:
                    return new NGRIDRegisterMessage();
                case MessageTypeIdNGRIDChangeCommunicationWayMessage:
                    return new NGRIDChangeCommunicationWayMessage();
                case MessageTypeIdNGRIDControllerMessage:
                    return new NGRIDControllerMessage();
                case MessageTypeIdNGRIDDataTransferResponseMessage:
                    return new NGRIDDataTransferResponseMessage();
                default:
                    throw new NGRIDException("Unknown MessageTypeId: " + messageTypeId);
            }
        }
    }
}
