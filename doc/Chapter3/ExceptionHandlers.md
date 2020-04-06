Exception handlers in CFGs
==========================

Exception handlers are special constructs that some platforms provide. Typically, in such a construct, one region of code (also known as the the try block in some languages) that is protected from exceptions or crashes. If any exception is thrown inside this region of code, control is transferred to one of the handler blocks that are associated to the protected region.

How are they represented in Echo?
---------------------------------

Exception handlers are modelled using the `ExceptionHandlerRegion<TInstruction>` class, which implements `IControlFlowRegion<TInstruction>`, and can therefore be added to the `Regions` property of a control flow graph, or any other sub region inside the control flow graph.

The `ExceptionHandlerRegion<TInstruction>` class consists of two parts:
- The `ProtectedRegion`: This is also known as the try block of the exception handler. It is the sub region of the CFG that is protected by this exception handler from exceptions.
- The `HandlerRegions`: A collection of regions that represent the handler blocks. Control might be transferred to one of these regions whenever an exception occures in the protected region.

Detecting Exception Handler regions
-----------------------------------

Echo can automatically create and subdivide a given CFG into exception handler regions if it is provided a list of address ranges representing the exception handlers.

```csharp
ControlFlowGraph<TInstruction> cfg = ...

// Define exception handler ranges.
var ranges = new[]
{
    new ExceptionHandlerRange(
        protectedRange: new AddressRange(tryStartOffset, tryEndOffset),
        handlerRange: new AddressRange(handlerStartOffset, handlerEndOffset)),
    ...
};

// Subdivivde the control flow graph into exception handler regions.
cfg.DetectExceptionHandlerRegions(ranges);
```

If two `ExceptionHandlerRange`s have the same address range for their protected region, they will be merged into one `ExceptionHandlerRegion<TInstruction>`, and both handler regions will be added to the same `ExceptionHandlerRegion<TInstruction>` region.

Exception handler ranges can also be assigned some user data. This allows for associating these ranges to additional metadata like the exception type that is caught:

```csharp
object someMetadata = ...

var range = new ExceptionHandlerRange(
    protectedRange: new AddressRange(tryStartOffset, tryEndOffset),
    handlerRange: new AddressRange(handlerStartOffset, handlerEndOffset),
    tag: someMetadata),
```

This additional metadata is then added to every handler region's `Tag` property.    


Common problems with detecting exception handlers
--------------------------------------------------

**I cannot find any exception handler regions in the CFG after calling DetectExceptionHandlerRegions:**

The `DetectExceptionHandlers` method requires that all nodes are present in the graph to function properly. Make sure that this also includes all nodes from any handler block.

On some platforms that implement exception handlers, the handler blocks are considered unreachable through normal execution paths. Therefore, if the control flow graph is built using a static or symbolic control flow graph builder, these nodes are not reached (as they perform a recursive traversal). To solve this issue, you can provide these flow graph builders these handler blocks as known basic block headers:

```csharp
IFlowGraphBuilder<TInstruction> builder = ...

var cfg = builder.ConstructFlowGraph(instructions, entrypointAddress, ranges);
```
