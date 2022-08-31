# Experiment 5
> Inspection of the SML '97 B Appendix

## Background
The B Appendix of the SML '97 specification contains a partial grammar for Full SML. The beginning of the appendix notes that it contains the grammar of the Core of SML only and that the rest of the grammar is found in section 8 (programs) and as given in section 3 figures 5–8 and augmented by a portion of the A Appendix. The prescribed modifications have been performed and the results are collocated in the A Appendix of my paper.  
While a number of overlaps are readily apparent in the lexical classes of SML and, per experiments 1–3, the rewrites have portions which are ambiguous, incomplete, prone to cyclicity, or otherwise difficult to write algorithms over, no experimentation has yet been done in this research to explore the potential for similar issues in the syntax of Full SML.  
This experiment was designed and intended to promote familiarity with the formal grammars of the SML '97 specification, especially those of Full SML given in the B Appendix (much as experiment 1 was for the A Appendix). This experiment was conducted by much less formal methods as its primary objective was to promote familiarity with the grammar instead of obtaining data on any specific aspect of the specification. To encourage a deep understanding and to keep in line with the themes of the paper, this inspection is performed with a mind to potential ambiguities.  

This experiment relies on the overlapping patterns of many paraterminals as found in section 2.4 of the SML '97 specification:  
- For any _x_, _longx_'s pattern is a superset of that of _x_.  
- _vid_, _tycon_, _lab_, and _strid_ all overlap as identifiers not starting with a prime.  
- `=` is both a token and an identifier; all other reserved words are excluded from the pattern of all identifiers so will not be considered as overlap here.  

The authors give a method for determining which identifiers belong to which paraterminal from context, but these rules are _not_ context-free so will have no bearing on our inspection of the grammar.

## Method
1. Starting from the productions of `program`, compare alternates for ambiguities by inspecting for alternates which differ only by items which are not singleton tokens (tokens with only a single admissible lexeme).
1. Note any pairs of alternates which meet this criterion.
	- If any of these pairs differ only by items with overlapping patterns and/or non-terminals already determined to have overlapping languages (including the empty string) then mark the entry as ambiguous.
1. Repeat recursively for each non-terminal in each alternate until all alternates accessible from the start symbol `program` have been considered.  

## Results
This experiment showed a _large_ degree of ambiguity when considering overlap on the empty string (_ε_), and thus it was interrupted, put aside, and replaced by  experiment 6 which automates this process in a more tangible context.  

## Analysis
From _strdec_ (the very first alternate of _topdec_ from the first alternate _program_), a large number of ambiguities are immediately obvious in the last pair of alternates: the empty string and `strdec <;> strdec`. This allows a sequence of an arbitrary number of empty strings to be a valid _strdec_. As _topdec_ → _strdec_ and all alternates of _topdec_ end with an optional _topdec_, this means any program can have any arbitrary sequence of empty strings parsed between each top-level declaration which automatically means every program's possible derivations are an infinite ambiguity class.  
Experiment 6 provides a greater insight to ambiguities which present a greater issue in practical parsing applications.~~~~