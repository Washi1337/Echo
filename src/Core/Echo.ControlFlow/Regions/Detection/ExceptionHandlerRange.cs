using Echo.Core.Code;

namespace Echo.ControlFlow.Regions.Detection
{
    public readonly struct ExceptionHandlerRange
    {
        public ExceptionHandlerRange(AddressRange protectedRange, AddressRange handlerRange)
        {
            ProtectedRange = protectedRange;
            HandlerRange = handlerRange;
        }
        
        public AddressRange ProtectedRange
        {
            get;
        }

        public AddressRange HandlerRange
        {
            get;
        }
    }
}