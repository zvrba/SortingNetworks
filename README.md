# Sorting networks

Playground for exploring implementation techniques for sorting networks.  These can sort small arrays much faster
than `Array.Sort()`; depending on the size (4-32) and pattern, the speedup is 3-12X. See [benchmarks](#benchmarks) below.

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
branchless code when constants that branches depend on are known at compile-time.  Though it can produce straight-line
code (even if source code contains branches), the generated machine code is huge: 32-sorter is > 1kB in size.

# Benchmarks

All benchmarks are in top-level directory of `SNBenchmark` project. Benchmarks were run on the following configuration:

```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19042.1348 (20H2/October2020Update)
Intel Core i7-8650U CPU 1.90GHz (Kaby Lake R), 1 CPU, 8 logical and 4 physical cores
.NET SDK=5.0.303
  [Host]     : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
  DefaultJob : .NET Core 3.1.21 (CoreCLR 4.700.21.51404, CoreFX 4.700.21.51508), X64 RyuJIT
```

## Summary

The table below show adjusted relative results: from each mean raw result (next section), the mean of "NoSort" benchmark is
subtracted and new results calculatd.  This is shown in "OnlySort" column.  "Ratio" column shows the ratio by which `Array.Sort`
is slower than network sort.

I couldn't figure out how to coerce BenchmarkDotNet into treating the baseline as additive overhead instead of, well, _baseline_.
(Actually, that's what `[IterationSetup]` and `[IterationCleanup]` are for, but they come with a warning that they could spoil results
of microbenchmarks.)  

Observed anomaly: sorting network is data-oblivious and always runs the same number of operations for a vector of
given size.  Yet, sorted and reverse-sorted inputs run significantly faster than random inputs.  The following
tables shows "raw" results for three different patterns (ascending, descending, random) and sizes of 4, 8 and 16.

|Method	        |Size	|Pattern	|Mean 	    |OnlySort   |Ratio
|-------------- |------ |---------- |----------:|----------:|----------:|
|NoSort	        |4	    |Asc	    |9.197		|           |           |
|ArraySort	    |4	    |Asc	    |34.932	    |25.735	    |3.084621839|
|NetworkSort	|4	    |Asc	    |17.54	    |8.343	    |1          |
|               |       |           |           |           |           |
|NoSort	        |4	    |Desc	    |10.197	    |	        |           |
|ArraySort	    |4	    |Desc	    |40.525	    |30.328	    |3.53020603 |
|NetworkSort	|4	    |Desc	    |18.788	    |8.591	    |1          |
|               |       |           |           |           |           |
|NoSort	        |4	    |Rand	    |13.267	    |	        |           |
|ArraySort	    |4	    |Rand	    |54.718	    |41.451	    |4.557058047|
|NetworkSort	|4	    |Rand	    |22.363	    |9.096	    |1          |
|               |       |           |           |           |           |
|NoSort	        |8	    |Asc	    |17.591	    |	        |           |
|ArraySort	    |8	    |Asc	    |57.123	    |39.532	    |4.326110746|
|NetworkSort	|8	    |Asc	    |26.729	    |9.138	    |1          |
|               |       |           |           |           |           |
|NoSort	        |8	    |Desc	    |22.368	    |	        |           |
|ArraySort	    |8	    |Desc	    |82.407	    |60.039	    |10.20377294|
|NetworkSort	|8	    |Desc	    |28.252	    |5.884	    |1          |
|               |       |           |           |           |           |
|NoSort	        |8	    |Rand	    |23.726	    |	        |           |
|ArraySort	    |8	    |Rand	    |101.007    |77.281	    |6.388971561|
|NetworkSort	|8	    |Rand	    |35.822	    |12.096	    |1          |
|               |       |           |           |           |           |
|NoSort	        |16	    |Asc	    |35.836	    |	        |           |
|ArraySort	    |16	    |Asc	    |92.05	    |56.214	    |4.597906102|
|NetworkSort	|16	    |Asc	    |48.062	    |12.226	    |1          |
|               |       |           |           |           |           |
|NoSort	        |16	    |Desc	    |34.743	    |	        |           |
|ArraySort	    |16	    |Desc	    |218.171    |183.428	|12.24976626|
|NetworkSort	|16	    |Desc	    |49.717	    |14.974	    |1          |
|               |       |           |           |           |           |
|NoSort	        |16	    |Rand	    |44.974	    |	        |           |
|ArraySort	    |16	    |Rand	    |223.987    |179.013	|7.861791831|
|NetworkSort	|16	    |Rand	    |67.744	    |22.77	    |1          |
|               |       |           |           |           |           |
|NoSort	        |32	    |Asc	    |66.193	    |	        |           |
|ArraySort	    |32	    |Asc	    |160.966    |94.773	    |3.808591866|
|NetworkSort	|32	    |Asc	    |91.077	    |24.884	    |1          |
|               |       |           |           |           |           |
|NoSort	        |32	    |Desc	    |87.786	    |	        |           |
|ArraySort	    |32	    |Desc	    |205.57	    |117.784	|4.844685752|
|NetworkSort	|32	    |Desc	    |112.098    |24.312	    |1          |
|               |       |           |           |           |           |
|NoSort	        |32	    |Rand	    |88.008	    |	        |           |
|ArraySort	    |32	    |Rand	    |673.462    |585.454	|11.42706016|
|NetworkSort	|32	    |Rand	    |139.242    |51.234	    |1          |

## Raw results

|      Method | Size | Pattern |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |
|------------ |----- |-------- |-----------:|----------:|----------:|-----------:|------:|--------:|
|      NoSort |    4 |     Asc |   9.197 ns | 0.1868 ns | 0.1459 ns |   9.259 ns |  1.00 |    0.00 |
|   ArraySort |    4 |     Asc |  34.932 ns | 0.4527 ns | 0.4235 ns |  34.855 ns |  3.79 |    0.09 |
| NetworkSort |    4 |     Asc |  17.540 ns | 0.3664 ns | 0.3762 ns |  17.441 ns |  1.91 |    0.05 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |    4 |    Desc |  10.197 ns | 0.2316 ns | 0.3805 ns |  10.106 ns |  1.00 |    0.00 |
|   ArraySort |    4 |    Desc |  40.525 ns | 0.4553 ns | 0.4036 ns |  40.484 ns |  3.94 |    0.19 |
| NetworkSort |    4 |    Desc |  18.788 ns | 0.4078 ns | 0.8512 ns |  18.410 ns |  1.85 |    0.09 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |    4 |    Rand |  13.267 ns | 0.2058 ns | 0.1825 ns |  13.275 ns |  1.00 |    0.00 |
|   ArraySort |    4 |    Rand |  54.718 ns | 0.1441 ns | 0.1125 ns |  54.701 ns |  4.12 |    0.06 |
| NetworkSort |    4 |    Rand |  22.363 ns | 0.2577 ns | 0.2411 ns |  22.432 ns |  1.69 |    0.03 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |    8 |     Asc |  17.591 ns | 0.3766 ns | 0.3868 ns |  17.394 ns |  1.00 |    0.00 |
|   ArraySort |    8 |     Asc |  57.123 ns | 0.1566 ns | 0.1388 ns |  57.121 ns |  3.24 |    0.07 |
| NetworkSort |    8 |     Asc |  26.729 ns | 0.1498 ns | 0.1251 ns |  26.722 ns |  1.51 |    0.04 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |    8 |    Desc |  22.368 ns | 0.0925 ns | 0.0772 ns |  22.356 ns |  1.00 |    0.00 |
|   ArraySort |    8 |    Desc |  82.407 ns | 0.3785 ns | 0.3160 ns |  82.470 ns |  3.68 |    0.02 |
| NetworkSort |    8 |    Desc |  28.252 ns | 0.4532 ns | 0.5395 ns |  28.011 ns |  1.27 |    0.03 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |    8 |    Rand |  23.726 ns | 0.3506 ns | 0.2737 ns |  23.764 ns |  1.00 |    0.00 |
|   ArraySort |    8 |    Rand | 101.007 ns | 0.6275 ns | 0.5563 ns | 100.972 ns |  4.25 |    0.06 |
| NetworkSort |    8 |    Rand |  35.822 ns | 0.6963 ns | 0.7151 ns |  35.648 ns |  1.51 |    0.03 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |   16 |     Asc |  35.836 ns | 0.1115 ns | 0.0988 ns |  35.852 ns |  1.00 |    0.00 |
|   ArraySort |   16 |     Asc |  92.050 ns | 1.2173 ns | 1.1386 ns |  91.701 ns |  2.57 |    0.03 |
| NetworkSort |   16 |     Asc |  48.062 ns | 0.9817 ns | 0.9642 ns |  47.609 ns |  1.34 |    0.03 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |   16 |    Desc |  34.743 ns | 0.6968 ns | 0.6518 ns |  34.444 ns |  1.00 |    0.00 |
|   ArraySort |   16 |    Desc | 218.171 ns | 0.5402 ns | 0.4789 ns | 218.239 ns |  6.27 |    0.12 |
| NetworkSort |   16 |    Desc |  49.717 ns | 0.8631 ns | 1.0275 ns |  49.247 ns |  1.44 |    0.04 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |   16 |    Rand |  44.974 ns | 0.2714 ns | 0.2539 ns |  44.923 ns |  1.00 |    0.00 |
|   ArraySort |   16 |    Rand | 223.987 ns | 2.0495 ns | 1.6001 ns | 224.106 ns |  4.98 |    0.04 |
| NetworkSort |   16 |    Rand |  67.744 ns | 1.3352 ns | 1.3114 ns |  68.121 ns |  1.51 |    0.03 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |   32 |     Asc |  66.193 ns | 0.3529 ns | 0.3128 ns |  66.253 ns |  1.00 |    0.00 |
|   ArraySort |   32 |     Asc | 160.966 ns | 1.7436 ns | 1.6310 ns | 161.082 ns |  2.43 |    0.03 |
| NetworkSort |   32 |     Asc |  91.077 ns | 0.4683 ns | 0.4151 ns |  91.144 ns |  1.38 |    0.01 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |   32 |    Desc |  87.786 ns | 0.9980 ns | 0.9335 ns |  87.555 ns |  1.00 |    0.00 |
|   ArraySort |   32 |    Desc | 205.570 ns | 1.2919 ns | 1.2084 ns | 205.687 ns |  2.34 |    0.02 |
| NetworkSort |   32 |    Desc | 112.098 ns | 0.6522 ns | 0.6101 ns | 112.078 ns |  1.28 |    0.02 |
|             |      |         |            |           |           |            |       |         |
|      NoSort |   32 |    Rand |  88.008 ns | 0.6930 ns | 0.6144 ns |  88.027 ns |  1.00 |    0.00 |
|   ArraySort |   32 |    Rand | 673.462 ns | 3.5335 ns | 3.1323 ns | 674.029 ns |  7.65 |    0.06 |
| NetworkSort |   32 |    Rand | 139.242 ns | 2.8139 ns | 7.3137 ns | 141.198 ns |  1.55 |    0.06 |


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
