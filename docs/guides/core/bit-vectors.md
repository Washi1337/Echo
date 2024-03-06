# Bit Vectors

Bit Vectors are a sequence of bits that can be interpreted and manipulated in various ways, and form the fundamental building blocks for anything related to emulation of bytecode and the memory models involved in the process.
It is therefore crucial to understand the fundamentals of this data type to be able to use a large portion of Echo's API.


## Trileans

Unlike normal bit vectors that comprise booleans or bits, Echo's bit vectors are based on ternary logic and comprise trileans instead.
A trilean is similar to a boolean or bit, but instead of two fundamental states it can be in one of three fundamental states:

<div class="table-responsive">
    <table class="table table-bordered table-condensed" style="width:auto;">
        <thead>
            <tr>
                <th>Value</th>
                <th>Representation</th>
            </tr>
        </thead>
        <tbody>
            <tr><td style="text-align:center"><code>0</code></td><td>False</td>
            <tr><td style="text-align:center"><code>1</code></td><td>True</td>
            <tr><td style="text-align:center"><code>?</code></td><td>Unknown</td>
        </tbody>
    </table>
</div>

Echo represents this data type using the `Trilean` structure:

```csharp
Trilean a = Trilean.True;
```

`Trilean` is meant as a drop-in replacement for anything that can accept `bool`.
A trilean can be seamlessly converted from a `bool` or `bool?` instances, thanks to implicit conversion operators:

```csharp
Trilean a = true;   // Trilean.True
Trilean b = false;  // Trilean.False
Trilean c = null;   // Trilean.Unknown
```

Testing whether a `Trilean` is `true` can be done in any place a boolean is expected as well, and does not require explicit comparison with `Trilean.True` or `true`.

```csharp
Trilean a = ...;
if (a)
{
    // `a` is true.
}
else 
{
    // `a` is false or unknown.
}
```

Note that if `a` is not `true` then it is either `false` or `unknown`.
Negating `false` works in a similar manner:

```csharp
Trilean a = ...;
if (!a)
{
    // `a` is false.
}
else 
{
    // `a` is true or unknown.
}
```

`Trilean` implements the same binary operations as booleans, and Echo defines syntax operators that allows you to use them 

```csharp
Trilean a = ...;
Trilean b = ...;

Trilean c = !a;
Trilean d = a & b;
Trilean e = a | b;
Trilean f = a ^ b;
```

The operations follow the following truth tables:

<style>
    .truth-table > tbody > tr > td, .truth-table > thead > tr > th {
        text-align:center;
    }
</style>

<div class="row">
    <div class="col-md-3">
        <b>NOT:</b>
        <div class="table-responsive">
            <table class="table table-bordered table-condensed truth-table">
                <thead>
                    <tr>
                        <th></th>
                        <th><b>!</b></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><b>0</b></td>
                        <td><code>1</code></td>
                    </tr>
                    <tr>
                        <td><b>1</b></td>
                        <td><code>0</code></td>
                    </tr>
                    <tr>
                        <td><b>?</b></td>
                        <td><code>?</code></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
    <div class="col-md-3">
        <b>AND:</b>
        <div class="table-responsive">
            <table class="table table-bordered table-condensed truth-table">
                <thead>
                    <tr>
                        <th><b>&</b></th>
                        <th><b>0</b></th>
                        <th><b>1</b></th>
                        <th><b>?</b></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><b>0</b></td>
                        <td><code>0</code></td>
                        <td><code>0</code></td>
                        <td><code>0</code></td>
                    </tr>
                    <tr>
                        <td><b>1</b></td>
                        <td><code>0</code></td>
                        <td><code>1</code></td>
                        <td><code>?</code></td>
                    </tr>
                    <tr>
                        <td><b>?</b></td>
                        <td><code>0</code></td>
                        <td><code>?</code></td>
                        <td><code>?</code></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
    <div class="col-md-3">
        <b>OR:</b>
        <div class="table-responsive">
            <table class="table table-bordered table-condensed truth-table">
                <thead>
                    <tr>
                        <th><b>|</b></th>
                        <th><b>0</b></th>
                        <th><b>1</b></th>
                        <th><b>?</b></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><b>0</b></td>
                        <td><code>0</code></td>
                        <td><code>1</code></td>
                        <td><code>?</code></td>
                    </tr>
                    <tr>
                        <td><b>1</b></td>
                        <td><code>1</code></td>
                        <td><code>1</code></td>
                        <td><code>?</code></td>
                    </tr>
                    <tr>
                        <td><b>?</b></td>
                        <td><code>?</code></td>
                        <td><code>?</code></td>
                        <td><code>?</code></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
    <div class="col-md-3">
        <b>XOR:</b>
        <div class="table-responsive">
            <table class="table table-bordered table-condensed truth-table">
                <thead>
                    <tr>
                        <th><b>^</b></th>
                        <th><b>0</b></th>
                        <th><b>1</b></th>
                        <th><b>?</b></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><b>0</b></td>
                        <td><code>0</code></td>
                        <td><code>1</code></td>
                        <td><code>?</code></td>
                    </tr>
                    <tr>
                        <td><b>1</b></td>
                        <td><code>1</code></td>
                        <td><code>0</code></td>
                        <td><code>?</code></td>
                    </tr>
                    <tr>
                        <td><b>?</b></td>
                        <td><code>?</code></td>
                        <td><code>?</code></td>
                        <td><code>?</code></td>
                    </tr>
                </tbody>
            </table>
        </div>
    </div>
</div>


## Initializing Bit Vectors

To initialize a new vector with a specific bit-length, use one of the constructors:

```csharp
var x = new BitVector(count: 32, initialize: true);  // Vector of 32 zeroes.
var y = new BitVector(count: 32, initialize: false); // Vector of 32 unknowns.
```

> [!NOTE]
> Currently, the bit-length of a bit vectors can only be a multiple of 8.

Bit vectors can also be created using one of the implicit conversion operators:

```csharp
BitVector y = 1337;   // Initializes a 32-bit vector with the bits of the integer 1337.
BitVector z = 1337L;  // Initializes a 64-bit vector with the bits of the long 1337.
BitVector w = 1337.0; // Initializes a 64-bit vector with the bits of the double 1337.0.
```

Bit vectors are not limited to just the sizes of primitives such as integers and longs. 
Any byte array of any size can be assigned to a bit vector:

```csharp
BitVector x = new byte[] {1, 2, 3, 4, 5}; // Initializes a 48-bit vector.
BitVector y = new byte[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13}; // Initializes a 136-bit vector.
```

The bits in a bit vector are accompanied with an equally-sized bit mask called the **known mask**.
Bit vectors can be initialized with such a bit mask to indicate which bits are actually known in the provided value.
Every `1` in the mask indicates the bit is known, and every `0` indicates an unknown bit instead:

```csharp
var x = new BitVector(bits: 0b00001111, knownMask: 0b11111111); // Constructs 00001111
var y = new BitVector(bits: 0b00001111, knownMask: 0b11001100); // Constructs 00??11??
var z = new BitVecotr(bits: 0b00000000, knownMask: 0b00000000); // Constructs ????????
```

The same can also be achieved by parsing a string containing a sequence of trileans:

```csharp
var x = BitVector.ParseBinary("00001111") // Constructs 00001111
var y = BitVector.ParseBinary("00??11??") // Constructs 00??11??
var z = BitVector.ParseBinary("????????") // Constructs ????????
```

This way, it is possible to represent fully known values, fully unknown values, as well as anything in between where some bits are known and some bits are not.

To test whether a bit vector is fully known, query the `IsFullyKnown` property:

```csharp
BitVector x = ...;
if (x.IsFullyKnown)
    Console.WriteLine("x contains no unknown bits.");
```


## Interpreting Bit Vectors

Bit vectors can be interpreted and converted to other values by creating a`BitVectorSpan`.
Similar to a `Span<byte>`, a `BitVectorSpan` can span an entire bit vector or a subrange within the bit vector:

```csharp
BitVector x = ...;
var full = x.AsSpan();
var slice1 = x.AsSpan(bitIndex: 8);
var slice2 = x.AsSpan(bitIndex: 8, length: 32);
```

Creating spans on bit vectors is cheap; it does not copy the vector nor does it allocate on the heap.
Rather, it provides a stack-allocated view on the underlying bit vector.

Once a span is obtained, individual bits (trileans) can be read:

```csharp
BitVectorSpan span = ...;

Trilean bit = span[6]; // Gets bit 6 from the span.
```

It is also possible to interpret entire spans as integers or floats:

```csharp
BitVectorSpan span = ...;

int x = span.I32;   // Interprets the bits stored in the span as a 32-bit integer.
float y = span.F64; // Interprets the bits stored in the span as a 32-bit float.
```

> [!WARNING]
> If the span contains unknown bits, these are treated as zeroes in the final result.
> Therefore, ensure that all bits are fully known before a higher-level interpretation of the bit vector.


## Mutating Bit Vectors

Operating on bit vectors is also done via `BitVectorSpan`s.
Once a span is obtained, individual trileans within a vector can be modified:

```csharp
BitVectorSpan span = ...;

span[6] = true;             // Sets bit 6 to `1`
span[7] = false;            // Sets bit 7 to `0`.
span[8] = Trilean.Unknown;  // Sets bit 8 to `?`.
```

Bit vector spans can also be cleared to zeroes, or marked fully known or unknown in their entirety:

```csharp
BitVectorSpan x = ...;

x.Clear();            // Resets all values to fully known zeroes.
x.MarkFullyKnown();   // Marks all bits as known.
x.MarkFullyUnknown(); // Marks all bits as unknown.
```

Assigning primitives can be done by assigning values to the convenience properties such as `I32` and `F32`:

```csharp
BitVectorSpan span = ...;

span.I32 = 1337; // Replaces the first 32-bits of the span with the bits of the signed integer 1337.
```

> [!WARNING]
> Assigning a value to convenience properties like `I32` does **not** change the known bit mask.
> If the original value contained unknown bits, these bits will remain unknown until marked known explicitly.


It is possible to perform bitwise operations on a bit vector span.
For example, an AND operation can be done:

```csharp
var x = BitVector.ParseBinary("11111100").AsSpan();
var y = BitVector.ParseBinary("11001100").AsSpan();

x.And(y);
// `x` now contains the bit vector `11001100`.
```

These operations adhere to the truth tables of trilean logic, and thus will take into account which bits are known and which are not.

```csharp
var x = BitVector.ParseBinary("????????").AsSpan();
var y = BitVector.ParseBinary("11001100").AsSpan();

x.And(y);
// `x` now contains the bit vector `??00??00`.
```

Shift operations are supported as well:

```csharp
var x = BitVector.ParseBinary("000011??").AsSpan();

x.ShiftLeft(8);
// `x` now contains the bit vector `0011??00`.
```

Various basic arithmetic operations are also supported for integers and floating-point typed values:

```csharp
var x = BitVector.ParseBinary("00010001").AsSpan();
var y = BitVector.ParseBinary("00000001").AsSpan();

x.IntegerAdd(y);
// `x` now contains the bit vector `00010010`.
```

This also supports partially known values, and Echo will try its best to infer which bits are certain and which are not:

```csharp
var x = BitVector.ParseBinary("00010001").AsSpan();
var y = BitVector.ParseBinary("0000000?").AsSpan();

x.IntegerAdd(y);
// `x` now contains the bit vector `000100??`.
```

When Echo cannot infer the final value of the bit vector with 100% certainty, the result will be a bit vector containing only `?`:

```csharp

var x = BitVector.ParseBinary("10000000").AsSpan();
var y = BitVector.ParseBinary("0000000?").AsSpan();

x.IntegerSubtract(y);
// `x` now contains the bit vector `????????`
```

> [!NOTE]
> Currently, floating-point arithmetic is limited if some bits are unknown in either of the involved floating-point bit vectors.

