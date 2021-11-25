# Specialized sorters

These are results of benchmarking `PeriodicInt` class.

## Summary

The table below shows adjusted relative results: from each mean raw result (next section), the mean of "NoSort" benchmark is
subtracted and new results calculatd.  This is shown in "OnlySort" column.  "Ratio" column shows the ratio by which `Array.Sort`
is slower than network sort.


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

