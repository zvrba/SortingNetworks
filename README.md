# Sorting networks

Playground for exploring implementation techniques for sorting networks.  These can sort smaller arrays much faster
than `Array.Sort()`; see [benchmarks](#benchmarks) below.

# Project structure

The projects are developed with Visual Studio 2019 and target netcore3.1.  The solution consists of two projects.

## SNBenchmark

This project dependes on BenchmarkDotNet. It contains validation code, benchmarks and demonstrates the use of sorting methods.
The main program must be run with a single argument: `V` or `B`. 

When run with `V`, it runs an exhaustive validation of networks for element counts of up to 28.  This is not possible for larger
networks as `2^N` zero/one inputs would have to be tested.

When run with "B", it passes the rest of the arguments to BenchmarkDotNet.  Without any additional arguments, it will present a menu.
All benchmarks call `Environment.FailFast` if the result is found to be unsorted so that this can be detected in the logs.

## SortingNetworks

`SortingNetworks` project is the main code and has no dependencies.  The high-performance public types use `unsafe`
code and can only be used from `unsafe` methods.  The code depends on AVX2 instruction set.  In addition, `AESRand`
class depends on AES-NI instruction set.  

### Sorting

The main interface is `UnsafeSort` struct which exposes a couple of public fields and static factory functions.  The actual
sorting code is in `PeriodicInt` class.  You are not expected to understand how it works without studying [references](#references).

Directory `Attic` contains the (failed) experiment with expression trees and an earlier iterations of the periodic network.

### Random numbers

This subsystem consists of three classes: and abstract `UnsafeRandom` class and two concrete classes: `AESRand` and `MWC1616Rand`.
These can be instantiated directly.  **NB!** The correctness of the code and the quality of random numbers has not been verified!
Benchmarks use `MWC1616Rand` with a fixed seed as `AESRand` seemed to generate some obvious patterns. ()

# Lessons learned
These were learned by inspecting the generated assembly code in Release mode.

When random generator state is a `struct`, that variable must not be declared as `readonly` in its
containing type -- defensive copies will be generated and state updates will be discarded.

Accessing static data has more overhead than accessing instance data: extraneous CALL instructions into the runtime
are generated.  My guess is that these ensure thread-safe, once-only static initialization semantics.

Passing parameters by `ref` as in `Periodics16Branchless` generates a lot of load/store instructions.
It is much more efficient to load ref parameters into locals at the beginning of the procedure and store
results at the end, as in `Periodic16`.  The generated assembly is lean and mean, no extra operations.
Even the call to `Swap` is inlined.

`Periodic16Expr` demonstrates how to build a sorter with expression trees.  The generated assembly is OK,
save for the long prologue/epilogue sequences  This makes the overhead of calling a lambda compiled at run-time
way too big for this application.

`unsafe` is not viral: Method `A` is allowed to call `unsafe` method `B` without `A` having to be marked
unsafe as well.

`System.Random` does not have consistent timing: when used in the baseline benchmark, the results almost always
contained a warning about it having a bimodal distribution.  This makes it rather unusable in baseline benchmarks.
Therefore `UnsafeRandom`, `AESRand` and `MWC1616Rand` classes were implemented.  Of these, only MWC is being used.

Generics suck for numeric code.  I couldn't figure out how to write a generic `bool IsSorted(T[])` method that'd
work for any numeric type.  Adding `where T : unmanaged` doesn't help as the compiler doesn't know that unmanaged
types are comparable with less-than and equal.  Nor does it seem possible to write `void Iota(T[] data)` that'd
fill `data` with numbers from `0 .. Length-1`.

I attempted to make concrete benchmark classes `sealed`, but that makes BenchmarkDotNet fail because it apparently
needs to derive from the benchmark class.

RyuJIT has some impressive optimizations: despite branches in "block" methods in `PeriodicInt`, it manages to generate
branchless code when constants that branches depend on are known at compile-time.

# Benchmarks

All benchmarks are in top-level directory of `SNBenchmark` project. Benchmarks were run on the following configuration:

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1348 (20H2/October2020Update)
Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.303
  [Host]     : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
  DefaultJob : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
```

# Main results

Observed anomaly: sorting network is data-oblivious and always runs the same number of operations for a vector of
the given size.  Yet, sorted and reverse-sorted inputs run significantly faster than random inputs.  The following
tables show "raw" results.

The tables below show adjusted relative results: from each mean result above, the mean of "Baseline" benchmark is
subtracted and new results calculatd.  I couldn't figure out how to coerce BenchmarkDotNet into treating the baseline
as additive overhead instead of, well, _baseline_.  (Actually, that's what `[IterationSetup]` and `[IterationCleanup]`
are for, but they come with a warning that they could spoil results of microbenchmarks.)

## Invocation: direct vs delegate vs compiled expression

This project was initially started to investigate manual code generation using expression trees, but it turns out that
these are unsuitable for high-performance scenarios as the prologue/epilogue in the generated code has way too high overhead
(see `ExpressionInvocationBenchmark`):

|           Method |      Mean |    Error |   StdDev |
|----------------- |----------:|---------:|---------:|
|     DirectInvoke |  45.51 ns | 0.934 ns | 2.147 ns |
| ExpressionInvoke | 124.08 ns | 2.512 ns | 6.747 ns |

On the other hand, there is no substantial difference between directly invoking an instance method, invoking it through an
interface or invoking it through a (generic) delegate (see `InvocationBenchmark`):

|          Method |     Mean |    Error |   StdDev |
|---------------- |---------:|---------:|---------:|
|    DirectInvoke | 57.54 ns | 0.763 ns | 0.676 ns |
| InterfaceInvoke | 58.66 ns | 1.002 ns | 1.954 ns |
|  DelegateInvoke | 59.28 ns | 1.198 ns | 1.718 ns |

The results between the two benchmarks are not directly comparable as they run different algorithms.

# References

D. E. Knuth, The Art of Computer Programming, vol. 3, section 5.3.4 for basic exposition. The ""periodic" network as
implemented here appears in TAOCP exercise 53, but has first been described by Dowd et al.: "The Periodic Balanced Sorting
Network", JACM Vol. 36, No. 4, October 1989, pp. 738-757.

Other references appear in code comments.
