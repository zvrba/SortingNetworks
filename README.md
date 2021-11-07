# Sorting networks

Playground for exploring implementation techniques for sorting networks.  These can sort smaller arrays much faster
than `Array.Sort()`; see benchmarks below.

## Project structure

`SortingNetworks` project is the main code and has no dependencies.  The high-performance public types use `unsafe`
code and can only be used from `unsafe` methods.  The code depends on AVX2 instruction set.  In addition, `AESRand`
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
Even the call to `Swap` is inlined.

`Periodic16Expr` demonstrates how to build a sorter with expression trees.  The generated assembly is OK,
save for the long prologue/epilogue sequences  This makes the overhead of calling a lambda compiled at run-time
way too big for this application.

`unsafe` is not viral: Method `A` is allowed to call `unsafe` method `B` without `A` having to be marked
unsafe as well.

`System.Random` does not have consistent timing: when used in the baseline benchmark, the results almost always
contained a warning about it having a bimodal distribution.  This makes it rather unusable in baseline benchmarks.
Therefore `IUnsafeRandom`, `AESRand` and `MWC1616Rand` classes were implemented.  Of these, only MWC is being used;
AES with a single round seems to generate obvious patterns.

# Benchmarks

Observed anomaly: sorting network is data-oblivious and always runs the same number of operations for a vector of
the given size.  Yet, sorted and reverse-sorted inputs run significantly faster than random inputs.

# References

D. E. Knuth, The Art of Computer Programming, vol. 3, section 5.3.4 for basic expositio. The ""periodic" network as
implemented here appears in TAOCP exercise 53, but has first been described by Dowd et al.: "The Periodic Balanced Sorting
Network", JACM Vol. 36, No. 4, October 1989, pp. 738-757.

Other references appear in code comments.


