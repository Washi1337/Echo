Control Flow Regions
====================

Control flow graphs can be partitioned into one or more regions. This is used to encode certain relationships between nodes, such as modelling lexical scopes and exception handlers.


Types of regions 
----------------

Echo defines three types of regions, which all implement the ``IControlFlowRegion<TInstruction>`` interface.

- ``ControlFlowGraph``: The root region. Every node that is not part of any sub region is part of this region.
- ``BasicControlFlowRegion``: A simple grouping of nodes and/or sub regions.
- ``ExceptionHandlerRegion``: A region of nodes that is protected by one or more exception handlers, which are represented by other sub regions.

Interacting with regions
------------------------

Nodes in a control flow graph are always part of a region. By default, they are part of the root region, which is the control flow graph itself.

Obtaining the region a node is situated in can be done using the ``ParentRegion`` property, or ``GetSituatedRegions`` method to get all the regions it is present in:

.. code-block:: csharp

    ControlFlowNode<TInstruction> node = ...

    var directParentRegion = node.ParentRegion;
    var allRegions = node.GetSituatedRegions();


Testing whether a node belongs to a region (including sub regions):

.. code-block:: csharp

    if (node.IsInRegion(parentRegion))
    {
        // ...
    }


Since it is often required to determine whether a node is inside of an exception handler regions, Echo defines a shortcut for finding the parent exception handler region:

.. code-block:: csharp

    ExceptionHandlerRegion<TInstruction> ehRegion = node.GetParentExceptionHandler();
    if (ehRegion is null)
        Console.WriteLine("Node is not present in any exception handler.");


Moving a node to a region can be done using the ``MoveToRegion``, or by adding it directly to the ``Nodes`` property of a ``BasicControlFlowRegion`` method.

.. code-block:: csharp

    BasicControlFlowRegion<TInstruction> otherRegion = ...;

    node.MoveToRegion(otherRegion);
    otherRegion.Nodes.Add(otherRegion);

Removing a node from any region can be done using the ``RemoveFromAnyRegion`` method (i.e. moving it back to the root region):

.. code-block:: csharp

    node.RemoveFromAnyRegion();


