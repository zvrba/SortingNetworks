# Sorting networks

Playground for exploring implementation techniques for sorting networks.  These can sort smaller arrays much faster
than `Array.Sort()`; see benchmarks below.

## Project structure

`SortingNetworks` project is the main code and has no dependencies.  The high-performance public types use `unsafe`
code and can only be used from `unsafe` methods.  The code depends on AVX2 instruction set.  In addition, `AESRand`
class depends on AES-NI instruction set.

`SNBenchmark` project contains the benchmarks and dependes on BenchmarkDotNet.  The main program exhaustively validates
`Periodic16` sorter.  This is not possible for larger networks as `2^N` zero/one inputs would have to be tested.  Benchmarks
call `Environment.FailFast` if the result is found to be unsorted, but this is of little assurance: by another theorem
in TAOCP, there exist "almost correct" networks that sort _every but one_ input.

## Lessons learned
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

# Benchmarks

Benchmarks were run on the following configuration:

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

## Direct vs delegate invocation

There is no substantial difference between directly invoking an instance method and invoking it through a generic delegate.
See `SNBenchmark/DelegateBenchmark.cs`.  A sample comparison:

|         Method |     Mean |    Error |   StdDev |
|--------------- |---------:|---------:|---------:|
|   DirectInvoke | 55.70 ns | 1.035 ns | 1.192 ns |
| DelegateInvoke | 55.42 ns | 0.884 ns | 0.738 ns |

This is the main result driving the design of `UnsafeSort<T>`.

# References

D. E. Knuth, The Art of Computer Programming, vol. 3, section 5.3.4 for basic expositio. The ""periodic" network as
implemented here appears in TAOCP exercise 53, but has first been described by Dowd et al.: "The Periodic Balanced Sorting
Network", JACM Vol. 36, No. 4, October 1989, pp. 738-757.

Other references appear in code comments.
