# Experiment 7
> Handling of Potentially Ambiguous Strings in HaMLet

## Background
[HaMLet](https://people.mpi-sws.org/~rossberg/hamlet/) is an ML interpreter/compiler written by Andreas Rossberg which is intended as "a faithful and complete implementation of the Standard ML programming language (SML '97)". Looking through the published [source code](https://github.com/rossberg/hamlet), one can observe a number of references to the standard. As such, we opted to use HaMLet as our reference implementation of an SML interpreter which uses classical, non-generalised parsing techniques.  

This experiment serves to demonstrate some of the limitations of the classical parsing techniques employed by using the `-p` (parse only) option of HaMLet. If a term is generated, then HaMLet has constructed a derivation tree for that input. If no term is generated, HaMLet has failed to find a derivation. Note that HaMLet will return at most _one_ derivation as the underlying parsing technique is not generalised. Each input provided has been tested against in ART as well as validated by hand to ensure that at least one grammatically-compliant derivation _does_ exist. This can be verified by the experimenter by passing each string through the `lexGLL` and `parseMGLL` scripts of ART (as detailed in other experiments) and confirming a result of `Accept`.

## Inputs
1: The sample input given by [Rossberg](https://people.mpi-sws.org/~rossberg/hamlet/defects.pdf) (pg 10, second paragraph) as having "no SML implementation being able to parse the … fragment" despite the fact that "according to the grammar this ought to be legal." ART is able to parse this input (use ART directive `!sppfShow` to see the (E)SPPF output as a GraphViz DOT file).
```
fun f x = case e1 of z => e2
  | f y = e3;
```  

2: This shows infixed patterns in value bindings. ART correctly parses the pattern as having an infixed VID.
```
val (a f b) = c;
```  

3: As above, but with different groupings to remove the crutch of the clearly delimited _pat_. ART again recognises the infixed VID.
```
val (a) b (c) = c;
```  

4: As above, this time completely ungrouped. ART recognises the infixed VID.
```
val a b c = c;
```  

4: This time the =/vid ambiguity discussed in the previous experiment is being used. ART returns all derivations, including the infixed pattern, the infixed expression, and the prefixed expression (i.e. the curried form of `b(=, c)`) amongst myriad others encoding the combinations of optional items in the grammar.
```
val a = b = c;
```  

## Method
1. `hamlet -p`
	- This loads HaMLet in 'parse mode' which simply returns a term corresponding to the selected derivation tree from parsing the input.
1. Type each input in term and note if a term or an error is produced.
	- If a term is produced: Compare with the ART output to determine what choices the parser has made.
	- If no term is produced: A parsing limitation has been found.

## Results
1. `syntax error: deleting EQUALS ALPHA SEMICOLON`
2. `syntax error: deleting EQUALS ALPHA RPAR`
3. `misplaced atomic pattern`
4. `misplaced atomic pattern`
5. Semantically equivalent to `val a = (b = c);`
```
(Program:1.0-1.14
  (STRDECTopDec:1.0-1.13
    (DECStrDec:1.0-1.13
      (VALDec:1.0-1.13
        (Seq:1.13-1.13
        )
        (PLAINValBind:1.4-1.13
          (ATPat:1.4-1.5
            (IDAtPat:1.4-1.5
              (LongVId a)
            )
          )
          (ATExp:1.10-1.13
            (PARAtExp:1.10-1.13
              (APPExp:1.10-1.13
                (ATExp:1.10-1.11
                  (IDAtExp:1.10-1.11
                    (LongVId =)
                  )
                )
                (RECORDAtExp:1.8-1.13
                  (ExpRow:1.8-1.13
                    (Lab 1)
                    (ATExp:1.8-1.9
                      (IDAtExp:1.8-1.9
                        (LongVId b)
                      )
                    )
                    (ExpRow:1.12-1.13
                      (Lab 2)
                      (ATExp:1.12-1.13
                        (IDAtExp:1.12-1.13
                          (LongVId c)
                        )
                      )
                    )
                  )
                )
              )
            )
          )
        )
      )
    )
  )
)
```

## Analysis
### File 1
This result is inline with the limitations Rossberg mentions and is a good demonstration of the practical advantages of generalised parsing algorithms over those employing a fixed lookahead.

### Files 2, 3, and 4
All three of these results are interesting because they represent use-cases which are common in SML: Infixed function definitions. This shows that there is likely some parser-hacking involving the semantics of the `infix`, `infixr`, and `nonfix` declarations. Files 2 & 3 are especially interesting in that they do not report as a 'syntax error'. I suspect that is what was intended to be reported, so this could be a bug in the software.

### File 5
While this produced a derivation, it made the implicit assumption that the first `=` was a keyword and that the second was an infixed function identifier. ART produced all valid derivations of this input and highlights that there are many which are semantically very different from HaMLet's chosen interpretation.

## Conclusions
Generally speaking, the limitations seen in HaMLet's parsing are limitations of the classical fixed lookahead-based algorithms it employs. The first issue is even explicitly mentioned by Rossberg in his analysis as a lookahead issue. The middle three are places where the accepted language differs from the specification, likely as the specification is somewhat vague on the handling of infixed identifiers in bindings. The final file is a case where multiple options are possible, but the parser is forced to choose a single result as this is a classical parsing system.  

ART fixes these issues. It uses a generalised algorithm, so lookahead is not a restriction. It can follow the grammar in the specification with very few modifications; it is quite close to an 'executable specification'-type system. Finally, it can return _all_ derivations instead of choosing a single one to hand to the semantic evaluator.