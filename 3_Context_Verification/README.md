# Experiment 3
> Automated Schema Context Testing

## Background
The A Appendix of the SML '97 specification contains a number of rewrite schemata to convert from the language of the external grammar (_Full SML_) to the language over which the semantics are defined (_Bare SML_) as discussed in detail in the body of the paper. During the course of our experimentation with ART, an attempt was made at performing the rewriting both by iteratively modifying the source string of tokens in-place and by constructing an ART term representing the program's Full SML parse tree and rewriting that term. A _term_ in ART is simply a textual representation of a tree as defined in the paper.  
While issues with the in-place token-string rewriting approach became quickly apparent (see the paper and experiment 2), there was a more fundemental issue with the term-rewriting approach: How does one construct a term from the inherantly string-like format of the schemata presented in the A Appendix? To this end, a simple modification to the grammer can be made (see below) to permit parsing of the strings given in the Appendix which freely intermix tokens, lexemes, and nonterminals. These strings, however, represent program fragments instead of whole programs, so we require knowledge of what the root of the terms we are constructing must be. The authors provides these, and they have been included as the _A_ values in the A Appendix of our paper. A simple validation check is performed to ensure that the given _A_ values are actually capable of deriving the strings given in the original A Appendix by using ART on our modified grammar to recognise the left and right hand sides of each schema as presented. In addition, recognition of each string is attempted against all other _A_ values to check for potential future ambiguities such as if a string can be derived in two separate contexts but is rewritten differently in either case.

## Method
As the rewrite schemata's variables are constrained by the non-terminal they must match, a modification to the `sml.art` presented in the paper is used to prevent the potential of an inadvertent selection of a concretisation of a given variable which may either not meet the constraint or matches other non-terminals. Each (syntax) non-terminal in our specification which has a directly equivalent non-terminal in the SML '97 specification's B Appendix has added as an alternate a new token whose only lexeme is the name of that non-terminal. In addition, all paraterminals are replaced by the name of the paraterminal to prevent mis-identification of nonterminal names as identifiers, for instance. The modified grammar is accessible as `sml_nt.art` in this folder. All commands are given to be run inside the ART directory of this repository, so the grammar and any inputs may need to be copied in as appropriate.  

For results tracking, we create a spreadsheet with two rows for each schema (one for the LHS and one for the RHS) and a column for every _A_. As this exercise is lengthy and highly repetitive, automation of the procedure is highly encouraged.  

For each context non-terminal _A_:
1. Modify the start symbol of `sml_nt.art` to _A_.
1. `export ARTHOME=.`
1. `./grammarWrite.sh sml_nt.art`
1. Add `.`_A_ to the filename of each generated file `ART*.art`
	- Example: For _A_ = _exp_, `ARTParserGrammar.art` would become `ARTParserGrammar.art.exp`, etc.
1. For each schema in the SML '97 specification's A Appendix:
	1. Copy the provided string of the left-hand side into a file `test.str`. When optional items are present, omit them.
	1. For each _A_:
		1. Copy the files `ART*.art.`_A_ to `ART*.art`
			- Example: `ARTParserGrammar.art.exp` would be copied to `ARTParserGrammar.art`
		1. `./lexGLL.sh test.str \!tweLongest \!twePriority \!tweDead`
		1. The first line of the result will begin either `Accept,test.str,…` or `Reject,test.str,…`. Note an `A` for `Accept` or an `R` for `Reject` in the corresponding cell of the spreadsheet.
	1. Repeat both steps for the right-hand side of the schema.

## Results
Included as both a plain CSV (results.csv) and an XLSX with conditional formatting (results.xlsx). The provided results are over both the full & bare language; the method for the full language is above, so the analysis will be over this portion of the results.

## Analysis
The right-hand sides represented by rows 5, 11, 13, 15, 23, 28, 30, 33, 36, 42, and 66 are not in the language of any of the surveyed _A_ values (rows marked in red). All left-hand sides were in the language of their assigned context non-terminal, and rows 2, 3, 22, 25, 26, and 39 were in the language of an _A_ value besides their assigned context non-terminal.  

An analysis of each of the unmatched right-hand sides:  

Rows 5, 28, and 36: `{1=exp, …, n=exp}`, `{1=pat, …, n=pat}`, and `{1:ty, …, n:ty}` cannot be recognised here as the authors have confounded the numeral lexemes with the token _LAB_. Replacing the numbers with _LAB_ in the examples allows these to be derivable in their respectives contexts.  

Row 11: `case exp of true => exp | false => exp` cannot be recognised as an _exp_ because `true` and `false` are identifier lexemes in SML instead of the more common handling of them as keywords.  

Rows 13 and 15: `if exp then true else exp` and `if exp then exp else false` have the same issue as row 11 in that `true` and `false` aren't keywords in SML.  

Rows 23 and 30: `exp :: … :: exp :: nil` and `pat :: … :: pat :: nil` are not recognised as `::` and `nil` are both identifiers and not tokens. Replacing these with the corresponding tokens from our modified A Appendix resolves the issue.  

Row 42: `val tyvarseq rec fvalbind` is noted in the SML '97 specification as not being in the language. This is an expected result.  

Row 66: `val it = exp ;` is not a _program_ as `it` here is another instance of a lexeme being used as if it were a token. Replacing `it` with `pat` (from the first alternate of _dec_ through _valbind_) allows the string to be recognised.  

## Conclusions
We have made numerous modifications to our A Appendix as a result of the findings of this experiment. This also demonstrated a clear need for the respecification of the rewrites to be over terminal-lexeme pairs instead of pure terminal strings or character strings as specific lexemes are required to be introduced in the rewriting process for later semantic evaluation. Use of these pairs allows us to avoid many of the potential pitfalls that the SML '97 authors needed to set as a limitation of their notation. Take the example of _A_ = _patrow_'s single schema: `vid`→ `vid = vid` (optional terms removed for brevity); the first `vid` instance on the right-hand side actually isn't a _vid_ at all! If the rule was to be followed explicitly, the resulting token string would not parse in many situations. This rewrite schema is described much more precisely in terminal-lexeme pair notation as `<vid, v>` → `<lab, v> = <vid, v>`.  
Additionally, this data shows that overlap between context non-terminals is rare in the specification, only occurring in `()` → `{ }`. The only place where the root node of an equivalent term-based rewriting system would _not_ be able to be the context non-terminal _A_ would be in the single schema where _A_ = _fvalbind_ as the right-hand side here is a _valbind_ instead of the left-hand side's _valbind_. This experiment shows that all other terms could be constructed to have the corresponding _A_ as their root.  