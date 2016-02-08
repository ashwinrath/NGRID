using NGRID.Exceptions;

namespace NGRID.Communication.Messages.ControllerMessages
{
    public static class ControlMessageFactory
    {
        public const int MessageTypeIdGetApplicationListMessage = 1;
        public const int MessageTypeIdGetApplicationListResponseMessage = 2;
        public const int MessageTypeIdClientApplicationRefreshEventMessage = 3;
        public const int MessageTypeIdAddNewApplicationMessage = 4;
        public const int MessageTypeIdRemoveApplicationMessage = 5;
        public const int MessageTypeIdRemoveApplicationResponseMessage = 6;
        public const int MessageTypeIdClientApplicationRemovedEventMessage = 7;
        public const int MessageTypeIdGetServerGraphMessage = 8;
        public const int MessageTypeIdGetServerGraphResponseMessage = 9;
        public const int MessageTypeIdUpdateServerGraphMessage = 10;
        public const int MessageTypeIdOperationResultMessage = 11;
        public const int MessageTypeIdGetApplicationWebServicesMessage = 12;
        public const int MessageTypeIdGetApplicationWebServicesResponseMessage = 13;
        public const int MessageTypeIdUpdateApplicationWebServicesMessage = 14;

        public static ControlMessage CreateMessageByTypeId(int messageTypeId)
        {
            switch (messageTypeId)
            {
                case MessageTypeIdGetApplicationListMessage:
                    return new GetApplicationListMessage();
                case MessageTypeIdGetApplicationListResponseMessage:
                    return new GetApplicationListResponseMessage();
                case MessageTypeIdClientApplicationRefreshEventMessage:
                    return new ClientApplicationRefreshEventMessage();
                case MessageTypeIdAddNewApplicationMessage:
                    return new AddNewApplicationMessage();
                case MessageTypeIdRemoveApplicationMessage:
                    return new RemoveApplicationMessage();
                case MessageTypeIdRemoveApplicationResponseMessage:
                    return new RemoveApplicationResponseMessage();
                case MessageTypeIdClientApplicationRemovedEventMessage:
                    return new ClientApplicationRemovedEventMessage();
                case MessageTypeIdGetServerGraphMessage:
                    return new GetServerGraphMessage();
                case MessageTypeIdGetServerGraphResponseMessage:
                    return new GetServerGraphResponseMessage();
                case MessageTypeIdUpdateServerGraphMessage:
                    return new UpdateServerGraphMessage();
                case MessageTypeIdOperationResultMessage:
                    return new OperationResultMessage();
                case MessageTypeIdGetApplicationWebServicesMessage:
                    return new GetApplicationWebServicesMessage();
                case MessageTypeIdGetApplicationWebServicesResponseMessage:
                    return new GetApplicationWebServicesResponseMessage();
                case MessageTypeIdUpdateApplicationWebServicesMessage:
                    return new UpdateApplicationWebServicesMessage();
                default:
                    throw new NGRIDException("Undefined ControlMessage MessageTypeId: " + messageTypeId);
            }
        }
    }
}
