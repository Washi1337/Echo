using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Signatures.Types;
using Echo.Memory;
using Echo.Platforms.AsmResolver.Emulation;
using Echo.Platforms.AsmResolver.Emulation.Dispatch;
using Echo.Platforms.AsmResolver.Emulation.Invocation;
using Echo.Platforms.AsmResolver.Tests.Mock;
using Xunit;

namespace Echo.Platforms.AsmResolver.Tests.Emulation.Invocation;

public class StringAllocatorTest : IClassFixture<MockModuleFixture>
{
    private readonly CilExecutionContext _context;
    private readonly TypeDefinition _stringType;
    private readonly CorLibTypeFactory _corlibFactory;

    private readonly StringAllocator _allocator = new();

    public StringAllocatorTest(MockModuleFixture fixture)
    {
        var machine = new CilVirtualMachine(fixture.MockModule, false);
        var thread = machine.CreateThread();
        _context = new CilExecutionContext(thread, CancellationToken.None);
        _context.Thread.CallStack.Push(fixture.MockModule.GetOrCreateModuleConstructor());

        _corlibFactory = fixture.MockModule.CorLibTypeFactory;
        _stringType = _corlibFactory.String.Type.Resolve()!;
    }

    [Fact]
    public void UsingCharArrayConstructorNull()
    {
        // Locate .ctor(char[])
        var ctor = _stringType.GetConstructor(_corlibFactory.Char.MakeSzArrayType())!;

        // Allocate using null pointer.
        var result = _allocator.Allocate(_context, ctor, [_context.Machine.ValueFactory.CreateNull()]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);
        Assert.Equal(0, result.Address!.AsObjectHandle(_context.Machine).Address);
    }

    [Fact]
    public void UsingCharArrayConstructorNonNull()
    {
        const string expectedString = "Hello, world!";

        // Allocate a char[].
        char[] data = expectedString.ToCharArray();
        var array = _context.Machine.Heap
            .AllocateSzArray(_corlibFactory.Char, data.Length, true)
            .AsObjectHandle(_context.Machine);

        array.WriteArrayData(MemoryMarshal.Cast<char, byte>(data));

        // Allocate string using .ctor(char[])
        var ctor = _stringType.GetConstructor(_corlibFactory.Char.MakeSzArrayType())!;
        var result = _allocator.Allocate(_context, ctor, [
            _context.Machine.ValueFactory.CreateNativeInteger(array.Address)
        ]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(expectedString, Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(data.Length, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingCharArrayConstructorSized()
    {
        const string expectedString = "Hello, world!";
        const int startIndex = 2;
        const int length = 6;

        // Allocate a char[].
        char[] data = expectedString.ToCharArray();
        var array = _context.Machine.Heap
            .AllocateSzArray(_corlibFactory.Char, data.Length, true)
            .AsObjectHandle(_context.Machine);

        array.WriteArrayData(MemoryMarshal.Cast<char, byte>(data));

        // Allocate string using .ctor(char[], int32, int32)
        var ctor = _stringType.GetConstructor(
            _corlibFactory.Char.MakeSzArrayType(),
            _corlibFactory.Int32,
            _corlibFactory.Int32
        )!;
        var result = _allocator.Allocate(_context, ctor, [
            _context.Machine.ValueFactory.CreateNativeInteger(array.Address),
            new BitVector(startIndex),
            new BitVector(length)
        ]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(expectedString[startIndex..(startIndex + length)],
            Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(length, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingCharPointerConstructor()
    {
        const string expectedString = "Hello, world!";

        // Allocate some flat memory.
        char[] data = expectedString.ToCharArray();
        long address = _context.Machine.Heap.AllocateFlat((uint)(data.Length + 1) * sizeof(char), true);
        _context.Machine.Memory.Write(address, MemoryMarshal.Cast<char, byte>(data));

        // Allocate string using .ctor(char*)
        var ctor = _stringType.GetConstructor(_corlibFactory.Char.MakePointerType())!;
        var result = _allocator.Allocate(_context, ctor, [_context.Machine.ValueFactory.CreateNativeInteger(address)]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(expectedString, Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(data.Length, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingCharPointerConstructorSized()
    {
        const string expectedString = "Hello, world!";
        const int startIndex = 2;
        const int length = 6;

        // Allocate some flat memory.
        char[] data = expectedString.ToCharArray();
        long address = _context.Machine.Heap.AllocateFlat((uint)(data.Length + 1) * sizeof(char), true);
        _context.Machine.Memory.Write(address, MemoryMarshal.Cast<char, byte>(data));

        // Allocate string using .ctor(char*, int32, int32)
        var ctor = _stringType.GetConstructor(
            _corlibFactory.Char.MakePointerType(),
            _corlibFactory.Int32,
            _corlibFactory.Int32
        )!;
        var result = _allocator.Allocate(_context, ctor, [
            _context.Machine.ValueFactory.CreateNativeInteger(address),
            new BitVector(startIndex),
            new BitVector(length)
        ]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(expectedString[startIndex..(startIndex + length)],
            Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(length, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingCharPointerConstructorWithUnknownBits()
    {
        // Allocate some flat memory with unknown bits.
        long address = _context.Machine.Heap.AllocateFlat(20, false);

        // Attempt to allocate string using .ctor(char*)
        var ctor = _stringType.GetConstructor(_corlibFactory.Char.MakePointerType())!;
        Assert.ThrowsAny<CilEmulatorException>(() =>
        {
            _allocator.Allocate(_context, ctor, [_context.Machine.ValueFactory.CreateNativeInteger(address)]);
        });
    }

    [Fact]
    public void UsingCharPointerConstructorWithUnknownBitsBounded()
    {
        // Allocate some flat memory with unknown bits.
        long address = _context.Machine.Heap.AllocateFlat(20, false);
        _context.Machine.Memory.Write(address, new BitVector(
            [0x41, 0x00, 0x41, 0x00, 0x00, 0x00],
            [0x0F, 0x0F, 0x0F, 0x0F, 0xFF, 0xFF]
        ));

        // Allocate string using .ctor(char*)
        var ctor = _stringType.GetConstructor(_corlibFactory.Char.MakePointerType())!;
        var result = _allocator.Allocate(_context, ctor, [_context.Machine.ValueFactory.CreateNativeInteger(address)]);

        // Verify
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(2, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingSBytePointerConstructor()
    {
        const string expectedString = "Hello, world!";

        // Allocate some flat memory.
        byte[] data = Encoding.ASCII.GetBytes(expectedString);
        long address = _context.Machine.Heap.AllocateFlat((uint)(data.Length + 1), true);
        _context.Machine.Memory.Write(address, data);

        // Allocate string using .ctor(sbyte*)
        var ctor = _stringType.GetConstructor(_corlibFactory.SByte.MakePointerType())!;
        var result = _allocator.Allocate(_context, ctor, [_context.Machine.ValueFactory.CreateNativeInteger(address)]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(expectedString, Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(data.Length, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingSbytePointerConstructorSized()
    {
        const string expectedString = "Hello, world!";
        const int startIndex = 2;
        const int length = 6;

        // Allocate some flat memory.
        byte[] data = Encoding.ASCII.GetBytes(expectedString);
        long address = _context.Machine.Heap.AllocateFlat((uint)(data.Length + 1), true);
        _context.Machine.Memory.Write(address, data);

        // Allocate string using .ctor(sbyte*, int32, int32)
        var ctor = _stringType.GetConstructor(
            _corlibFactory.SByte.MakePointerType(),
            _corlibFactory.Int32,
            _corlibFactory.Int32
        )!;
        var result = _allocator.Allocate(_context, ctor, [
            _context.Machine.ValueFactory.CreateNativeInteger(address),
            new BitVector(startIndex),
            new BitVector(length)
        ]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(expectedString[startIndex..(startIndex + length)],
            Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(length, resultObject.ReadStringLength().AsSpan().I32);
    }

    [Fact]
    public void UsingCharInt32Constructor()
    {
        // Allocate string using .ctor(char, int32)
        var ctor = _stringType.GetConstructor(_corlibFactory.Char, _corlibFactory.Int32)!;
        var result = _allocator.Allocate(_context, ctor, [
            new BitVector('A'),
            new BitVector(30)
        ]);

        // Verify.
        Assert.Equal(AllocationResultType.FullyConstructed, result.ResultType);

        var resultObject = result.Address!.AsObjectHandle(_context.Machine);
        Assert.Equal(new string('A', 30), Encoding.Unicode.GetString(resultObject.ReadStringData().Bits));
        Assert.Equal(30, resultObject.ReadStringLength().AsSpan().I32);
    }
}