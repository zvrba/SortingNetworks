# Sorting networks

Playground for exploring implementation techniques for sorting networks.  These can sort smaller arrays much faster
than `Array.Sort()`; see benchmarks below.

## Project structure

`SortingNetworks` project is the main code and has no dependencies.  The high-performance public types use `unsafe`
code and can only be used from `unsafe` blocks.  The code depends on AVX2 instruction set.  In addition, `AESRand`
class depends on AES-NI instruction set.

`SNBenchmark` project contains the benchmarks and dependes on BenchmarkDotNet.

## Lessons learned
These were learned by inspecting the generated assembly code in Release mode.

When random generator state is a `struct`, that variable must not be declared as `readonly` in its
containing type -- defensive copies will be generated and state updates will be discarded.

Accessing static data has more overhead than accessing instance data: extraneous CALL instructions into the runtime
are generated.  My guess is that these ensure thread-safe, once-only static initialization semantics.

Passing parameters by `ref` as in `Periodics16Branchless` generates a lot of load/store instructions.
It is much more efficient to load ref parameters into locals at the beginning of the procedure and store
results at the end, as in `Periodic16`.  The generated assembly is lean and mean, no extra operations.

`Periodic16Expr` demonstrates how to build a sorter with expression trees.  The generated assembly is OK,
but the overhead of calling a compiled lambda is way too big.

`unsafe` is not viral: Method `A` is allowed to call `unsafe` method `B` without `A` having to be marked
unsafe as well.

# Benchmarks

# References
