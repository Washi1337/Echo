using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AsmResolver.DotNet.Code.Cil;
using Echo.Code;

namespace Echo.Platforms.AsmResolver.Emulation.Stack
{
    /// <summary>
    /// Provides a mechanism for implementing exception handling for a single protected range within a single method body.
    /// </summary>
    [DebuggerDisplay("Range = {ProtectedRange}, Handler Count = {Handlers.Count}, Next Offset = {NextOffset}")]
    public class ExceptionHandlerFrame
    {
        private readonly List<CilExceptionHandler> _handlers;
        private int _currentHandlerIndex = -1;

        /// <summary>
        /// Creates a new exception handler frame.
        /// </summary>
        /// <param name="protectedRange">The IL offset range the exception handler is protecting.</param>
        public ExceptionHandlerFrame(AddressRange protectedRange)
        {
            ProtectedRange = protectedRange;
            _handlers = new List<CilExceptionHandler>();
            Reset();
        }
        
        /// <summary>
        /// Gets the IL offset range the exception handler is protecting.
        /// </summary>
        public AddressRange ProtectedRange
        {
            get;
        }

        /// <summary>
        /// Gets a list of handlers that are associated to the protected range.
        /// </summary>
        public IReadOnlyList<CilExceptionHandler> Handlers => _handlers;

        /// <summary>
        /// Gets a value indicating whether the exception handler has a finalizer block.
        /// </summary>
        public bool HasFinalizer => Handlers.Any(x => x.HandlerType == CilExceptionHandlerType.Finally);
        
        /// <summary>
        /// Gets the current active exception handler (if available).
        /// </summary>
        public CilExceptionHandler? CurrentHandler
        {
            get
            {
                if (_currentHandlerIndex < 0 || _currentHandlerIndex >= Handlers.Count)
                    return null;
                
                return Handlers[_currentHandlerIndex];
            }
        }

        /// <summary>
        /// Gets the offset to jump to after a finalizer has exited (if available).
        /// </summary>
        public int? NextOffset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the current exception object that was thrown within the protected range. 
        /// </summary>
        public ObjectHandle ExceptionObject
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the exception handler is currently handling the exception referenced
        /// by <see cref="ExceptionObject"/>.
        /// </summary>
        public bool IsHandlingException
        {
            get;
            private set;
        }
        
        internal void AddHandler(CilExceptionHandler handler)
        {
            if (handler.TryStart?.Offset != ProtectedRange.Start || handler.TryEnd?.Offset != ProtectedRange.End)
                throw new ArgumentException($"Exception handler does not protect the range {ProtectedRange}.");

            _handlers.Add(handler);
        }

        /// <summary>
        /// Determines whether the provided offset falls within the protected offset range or any of the handlers. 
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns><c>true</c> if the offset is within the frame, <c>false</c> otherwise.</returns>
        public bool ContainsOffset(int offset)
        {
            if (ProtectedRange.Contains(offset))
                return true;

            foreach (var handler in _handlers)
            {
                // As per ecma-335, the filter start must be right before the handler start. Thus we can assume that any
                // exception handler for which the type is a filter, the full handler spans [filter start, handler end).
                int startOffset = handler.HandlerType == CilExceptionHandlerType.Filter
                    ? handler.FilterStart!.Offset
                    : handler.HandlerStart!.Offset;

                if (offset >= startOffset && offset < handler.HandlerEnd!.Offset)
                    return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Resets the exception handler to its initial state.
        /// </summary>
        public void Reset()
        {
            _currentHandlerIndex = -1;
            NextOffset = null;
            ExceptionObject = default;
        }

        /// <summary>
        /// Registers the occurrence of an exception, and determines the next offset to jump to that will attempt
        /// to handle the exception or finalize the code block.
        /// </summary>
        /// <param name="exceptionObject">The exception object.</param>
        /// <returns></returns>
        public int? RegisterException(ObjectHandle exceptionObject)
        {
            // If we are already handling an exception, and we are throwing a new exception, exception handling failed,
            // and thus we must jump out.
            if (IsHandlingException)
            {
                IsHandlingException = false;
                return null;
            }

            // Only register the exception object if we don't have one already. We dot his because filter clauses can
            // also throw exceptions, which are silently consumed by the runtime.
            if (ExceptionObject.IsNull)
                ExceptionObject = exceptionObject;
            
            return MoveToNextCompatibleHandler();
        }

        /// <summary>
        /// Leaves either a protected range or an exception handler while marking the exception as handled successfully.
        /// </summary>
        /// <param name="leaveTargetOffset">The target offset to jump to.</param>
        /// <returns>The offset to jump to.</returns>
        public int Leave(int leaveTargetOffset)
        {
            // Save the target offset, such that any finalizer blocks can jump to it later.
            NextOffset = leaveTargetOffset;

            // Mark any exception as handled.  
            ExceptionObject = default;
            IsHandlingException = false;
            
            // TODO: support fault handlers.
            return MoveToFinalizer() ?? leaveTargetOffset;
        }

        /// <summary>
        /// Exits a finally block, and determines the next offset to jump to.
        /// </summary>
        /// <returns>The offset to jump to.</returns>
        public int? EndFinally()
        {
            if (Handlers.All(x => x.HandlerType != CilExceptionHandlerType.Finally))
                throw new CilEmulatorException("Operation requires an active finally exception handler.");
            
            return NextOffset;
        }

        /// <summary>
        /// Exits a filter clause, and determines the next offset to jump to.
        /// </summary>
        /// <param name="result">
        /// <c>true</c> if the exception should be handled by the current handler, <c>false</c> otherwise.
        /// </param>
        /// <returns>The offset to jump to.</returns>
        public int? EndFilter(bool result)
        {
            if (CurrentHandler?.HandlerType != CilExceptionHandlerType.Filter)
                throw new CilEmulatorException("Operation requires an active filter exception handler.");
            
            if (result)
                return CurrentHandler.HandlerStart!.Offset;

            return MoveToNextCompatibleHandler();
        }

        private int? MoveToNextCompatibleHandler()
        {
            _currentHandlerIndex++;

            while (CurrentHandler is not null)
            {
                switch (CurrentHandler.HandlerType)
                {
                    case CilExceptionHandlerType.Exception:
                        var supportedType = CurrentHandler.ExceptionType!.ToTypeSignature();
                        var actualType = ExceptionObject.GetObjectType().ToTypeSignature();

                        if (actualType.IsAssignableTo(supportedType))
                        {
                            IsHandlingException = true;
                            return CurrentHandler.HandlerStart!.Offset;
                        }

                        break;
                    
                    case CilExceptionHandlerType.Filter:
                        return CurrentHandler.FilterStart!.Offset;

                    case CilExceptionHandlerType.Finally:
                        return CurrentHandler.HandlerStart!.Offset;

                    case CilExceptionHandlerType.Fault:
                        // TODO: support fault handlers.
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                _currentHandlerIndex++;
            }

            return null;
        }
        
        private int? MoveToFinalizer()
        {
            _currentHandlerIndex++;

            while (CurrentHandler is not null)
            {
                if (CurrentHandler.HandlerType == CilExceptionHandlerType.Finally)
                    return CurrentHandler.HandlerStart!.Offset;

                _currentHandlerIndex++;
            }

            _currentHandlerIndex = -1;
            return null;
        }
    }
}