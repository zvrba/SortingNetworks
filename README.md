# Sorting networks

Playground for exploring implementation techniques for sorting networks.  These can sort small arrays much faster
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

The main interface is `UnsafeSort` class which exposes a couple of properties/methods and static factory functions.  The actual
sorting code is in `PeriodicInt` class.  You are not expected to understand how it works without studying [references](#references).
Nevertheless, methods are exposed as public as they can be used as building blocks of larger sorters and/or parallel sorters
of smaller sizes.  It also exposes "raw" sorting methods with hard-coded maximum sizes.  The class has no writable internal state,
so it is recommended to use a single (non-static) instance throughout the program (see remark about statics in below).

NB! `UnsafeSort<T>.Sort(T* data, int c)` are not yet implemented -- only sizes 4, 8, 16 and 32 are supported.

Directory `Attic` contains the (failed) experiment with expression trees and an earlier (less performant) iterations of the
periodic network.

### Random numbers

This subsystem consists of three classes: and abstract `UnsafeRandom` class and two concrete classes: `AESRand` and `MWC1616Rand`.
These can be instantiated directly.  **NB!** The correctness of the code and the quality of random numbers has not been verified!
Benchmarks use `MWC1616Rand` with a fixed seed as `AESRand` seemed to generate some obvious patterns. ()

# Lessons learned
These were learned by inspecting the generated assembly code in Release mode.

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
unsafe as well.  Also, it is allowed to assign an `unsafe` method to a non-unsafe delegate variable.

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
given size.  Yet, sorted and reverse-sorted inputs run significantly faster than random inputs.  The following
tables shows "raw" results for three different patterns (ascending, descending, random) and sizes of 4, 8 and 16.

|      Method | Pattern | Size |       Mean |     Error |    StdDev | Ratio | RatioSD |
|------------ |-------- |----- |-----------:|----------:|----------:|------:|--------:|
|      NoSort |     Asc |    4 |   9.235 ns | 0.1546 ns | 0.3521 ns |  1.00 |    0.00 |
|   ArraySort |     Asc |    4 |  36.464 ns | 0.7381 ns | 0.6163 ns |  3.80 |    0.18 |
| NetworkSort |     Asc |    4 |  17.414 ns | 0.3547 ns | 0.3145 ns |  1.82 |    0.10 |
|             |         |      |            |           |           |       |         |
|      NoSort |    Desc |    4 |   9.726 ns | 0.1218 ns | 0.0951 ns |  1.00 |    0.00 |
|   ArraySort |    Desc |    4 |  43.071 ns | 0.7246 ns | 0.6778 ns |  4.42 |    0.08 |
| NetworkSort |    Desc |    4 |  19.277 ns | 0.1599 ns | 0.1417 ns |  1.98 |    0.03 |
|             |         |      |            |           |           |       |         |
|      NoSort |    Rand |    4 |  13.326 ns | 0.1377 ns | 0.1288 ns |  1.00 |    0.00 |
|   ArraySort |    Rand |    4 |  54.264 ns | 0.2849 ns | 0.2379 ns |  4.07 |    0.05 |
| NetworkSort |    Rand |    4 |  22.556 ns | 0.4795 ns | 0.5708 ns |  1.68 |    0.05 |
|             |         |      |            |           |           |       |         |
|      NoSort |     Asc |    8 |  17.757 ns | 0.3177 ns | 0.3902 ns |  1.00 |    0.00 |
|   ArraySort |     Asc |    8 |  55.678 ns | 1.0613 ns | 1.0423 ns |  3.12 |    0.09 |
| NetworkSort |     Asc |    8 |  26.037 ns | 0.4418 ns | 0.4133 ns |  1.46 |    0.04 |
|             |         |      |            |           |           |       |         |
|      NoSort |    Desc |    8 |  22.384 ns | 0.1172 ns | 0.1039 ns |  1.00 |    0.00 |
|   ArraySort |    Desc |    8 |  82.120 ns | 0.3849 ns | 0.3005 ns |  3.67 |    0.02 |
| NetworkSort |    Desc |    8 |  28.125 ns | 0.1688 ns | 0.1579 ns |  1.26 |    0.01 |
|             |         |      |            |           |           |       |         |
|      NoSort |    Rand |    8 |  24.462 ns | 0.1588 ns | 0.1240 ns |  1.00 |    0.00 |
|   ArraySort |    Rand |    8 | 110.004 ns | 2.1193 ns | 1.9824 ns |  4.51 |    0.07 |
| NetworkSort |    Rand |    8 |  35.322 ns | 0.7283 ns | 1.0446 ns |  1.44 |    0.06 |
|             |         |      |            |           |           |       |         |
|      NoSort |     Asc |   16 |  32.679 ns | 0.5633 ns | 0.4398 ns |  1.00 |    0.00 |
|   ArraySort |     Asc |   16 |  85.866 ns | 1.2877 ns | 1.6285 ns |  2.64 |    0.08 |
| NetworkSort |     Asc |   16 |  46.122 ns | 0.7638 ns | 0.7501 ns |  1.41 |    0.04 |
|             |         |      |            |           |           |       |         |
|      NoSort |    Desc |   16 |  44.764 ns | 0.1354 ns | 0.1201 ns |  1.00 |    0.00 |
|   ArraySort |    Desc |   16 | 216.594 ns | 2.2502 ns | 1.7568 ns |  4.84 |    0.04 |
| NetworkSort |    Desc |   16 |  51.849 ns | 0.2652 ns | 0.2214 ns |  1.16 |    0.00 |
|             |         |      |            |           |           |       |         |
|      NoSort |    Rand |   16 |  46.484 ns | 0.6184 ns | 0.5784 ns |  1.00 |    0.00 |
|   ArraySort |    Rand |   16 | 228.829 ns | 2.5336 ns | 2.2460 ns |  4.92 |    0.06 |
| NetworkSort |    Rand |   16 |  68.567 ns | 1.1436 ns | 1.5267 ns |  1.48 |    0.04 |

The tables below show adjusted relative results: from each mean result above, the mean of "NoSort" benchmark is
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

On the other hand, there is no substantial difference between directly invoking an instance method, or invoking it through an
abstract base method.  Thus there is no penalty in using the more convenient `UnsafeSort` class as opposed to directly calling
methods on an instance of `PeriodicInt`:


|         Method |     Mean |    Error |   StdDev |
|--------------- |---------:|---------:|---------:|
| AbstractInvoke | 23.80 ns | 0.421 ns | 0.603 ns |
| ConcreteInvoke | 23.28 ns | 0.310 ns | 0.290 ns |

NB! The results between the two benchmarks are not directly comparable as they run different algorithms.

# References

D. E. Knuth, The Art of Computer Programming, vol. 3, section 5.3.4 for basic exposition. The ""periodic" network as
implemented here appears in TAOCP exercise 53, but has first been described by Dowd et al.: "The Periodic Balanced Sorting
Network", JACM Vol. 36, No. 4, October 1989, pp. 738-757.

Other references appear in code comments.
