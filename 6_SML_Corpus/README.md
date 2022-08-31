# Experiment 6
> Automated Analysis of the Full SML Grammar

## Background
After the first ambiguities in experiment 5 were discovered, it was readily apparent that an exhaustive manual search was inefficient. The decision was made to automate the checking of ambiguity classes using ART; additionaly, the data was taken from real-world SML libraries and compiler validation sets. We hoped this would give a better overview of which ambiguities occur in real-world SML usage and their frequencies.

## Method
### Obtaining the Corpus
For our experiment, we opted to use the verification set available with the HaMLet SML interpreter which served as our reference point throughout the research. Further details can be found in the paper.  
In addition to this data, we also included the SML/NJ Basis Library and the source of ML-Lex & ML-Yacc.

Sources:  
- https://github.com/rossberg/hamlet/tree/master/test
- https://github.com/rossberg/hamlet/tree/master/basis
- https://www.smlnj.org/dist/working/2021.1/index.html
	- ml-lex.tgz
	- ml-yacc.tgz
	1. Recursively extract all files with paths ending in `.sml`.
	1. Discard the rest of the archive.
	1. Flatten the results into a single folder per archive.

### Preprocessing the Corpus
> This portion of experiment was performed by automation.

Before ART can handle the input, some preprocessing is required. Each file must have all comments removed (our grammar doesn't support them) and all leading whitespace trimmed (a limitation of ART at time of writing).  
A simple C# script for performing these tasks is included in this folder as `clean.csx`. The rest of the document will assume that the corpus has been preprocesses/_cleaned_ before continuing.

### Detecting Ambiguity Classes
> This portion of experiment was performed by automation.

Each file must be lexed and parsed by ART, then the resulting TWE set can be read out. Any left-extents with more than one TWE set element are counted as an _ambiguous left extent_. A simple script for performing this on the windows version of ART is included, but it is recommended that the version of ART in use be checked for compatability first. This script is current to the versions of ART published through the RHUL website in March/April 2022 (the 'SLELabs' builds). A method compatable with the Linux-version of current (September 2022, included in this repo) ART builds is included below:  

1. Navigate to the directory containing ART and the grammar `sml.art`.
1. `export ARTHOME=.`
1. `./grammarWrite.sh sml.art`
1. For each SML file `f` in the corpus:
	1. `./lexGLL.sh f \!tweLongest \!twePriority \!tweDead`
	1. `./parseMGLL.sh f \!tweFromSPPF \!twePrint`
	1. Document each left extent with more than one TWE set element in the resulting output.
		- Note the left extent.
		- Note the tokens of the element at that extent.

## Results
Included in this directory as ouput.csv.

## Analysis
Three ambiguity classes were discovered, each is explored in depth in the body of the paper.

## Conclusions
By far the most common ambiguity resulted from infexp's two alternates:
```
infexp	::= appexp
		  | infexp vid infexp

appexp	::= atexp
		  | appexp atexp

atexp	::= <op> longvid
			…
```

Considering the overlap between vid & longvid's patterns, any sequence of lexemes which results in three or more juxtaposed vids/longvids will be ambiguous between some variant of the following two terms:
```
infexp(infexp(appexp(atexp(longvid))), vid, infexp(appexp(atexp(longvid))))
infexp(appexp(appexp(appexp(atexp(longvid)), atexp(longvid)), atexp(longvid)))
```

Additionally, the non-exclusion of `=` from the pattern of the identifiers yields a number of ambiguities in light of the infixed operator declaration form (e.g. `val a = b = c`) as the first alternate of _valbind_ includes `valbind ::= pat = exp` and the language of _pat_ includes sentences where `=` is used as a lexeme of a _vid_.  