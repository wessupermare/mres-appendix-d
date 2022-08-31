# Experiment 8
> Counting Lexicalisations, TWE Set Sizes, and Sentences

## Background
The majority of this experiment, its results, and its analysis is covered in section 2 of the paper, but this report has been written for completeness. Additionally, this report presents the commands appropriately for the Linux ART scripts in line with the other experiments where the paper uses the Windows forms for brevity.

## Method
Each referenced input file is included in this folder and should be copied to the `ART` folder (where the below commands are run) for this experiment.

1. `export ARTHOME=.`
1. `./grammarWrite.sh sml.art`
1. `./lexGLL.sh sml1.str \!tweDead \!tweSegments \!tweLexicalisations \!tweCounts`
	- Note the number of 'token-only lexicalisations'; these are the `Token Strings` values given in the tables in the paper.
	- Note the TWE set cardinality _excluding_ the number of suppressed elements (i.e. the first number on the TWE set cardinality line, not the last one)
1. `./parseMGLL.sh sml1.str \!tweFromSPPF \!tweSegments \!tweLexicalisations`
	- Note the number of 'token-only lexicalisations'; these are the 'sentences' referenced in the tables of the paper, specifically the upper bound of the number of sentences.
1. `./lexGLL.sh sml1.str \!twePriority \!tweDead \!tweSegments \!tweLexicalisations \!tweCounts`
	- Note the results as with 2, except noting that priority has been applied.
1. `./parseMGLL.sh sml1.str \!tweFromSPPF \!tweSegments \!tweLexicalisations`
	- Note the results as with 3, noting priority as above.
1. `./lexGLL.sh sml1.str \!tweLongest \!twePriority \!tweDead \!tweSegments \!tweLexicalisations \!tweCounts`
	- Note the results as with 2, noting priority and longest match have been applied.
1. `./parseMGLL.sh sml1.str \!tweFromSPPF \!tweSegments \!tweLexicalisations`
	- Note the results as with 3, noting priority and longest match.
1. Repeat steps 2 through 7 on `sml2.str` and `sml3.str`.

## Results
Can be found in section 2 of the paper; copied here for convenience.

| File |   Disambiguations  | Token Strings | TWE Set Size | Sentence Upper Bound |
| ----:|:------------------:|:-------------:|:------------:|:--------------------:|
| sml1 |        None        |   108928800   |     156      |         720          |
| sml1 |      Priority      |    89777700   |     146      |         630          |
| sml1 | Longest & Priority |         135   |      20      |           1          |
|------|--------------------|---------------|--------------|----------------------|
| sml2 |        None        |  7344832950   |     192      |         896          |
| sml2 |      Priority      |  6617524230   |     174      |         620          |
| sml2 | Longest & Priority |        1215   |      30      |           2          |
|------|--------------------|---------------|--------------|----------------------|
| sml3 |        None        |    23645844   |      96      |         288          |
| sml3 |      Priority      |    14780475   |      85      |         252          |
| sml3 | Longest & Priority |       18225   |      40      |           6          |

## Analysis
The addition of longest match provides the greatest sentence reduction on these input strings. Other inputs may be different in this regard. Not all inputs are reduced to a single sentence by longest match & priority, so further (likely at the rewriter, but potentially even semantic) disambiguation may be required. The number of token strings appears quite large compared to the TWE set size even for these small inputs, so the use of a TWE set based ambiguity reduction strategy appears highly efficient. `sml2` started with a higher sentence count than `sml1` before disambiguation, but priority disambiguation removed significantly more sentences from `sml2`. I attribute this to the greater flexibility of the syntax of the `val` construct in `sml1` than the `while`-`do` construct in `sml2`; the parser is simply able to discard more options within the more constrained bounds of the `while` loop's syntax.  