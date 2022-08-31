# Experiment 1
> Static Analysis of the SML '97 A Appendix

## Background
This experiment was conducted towards the beginning of the research period and served primarily as a way to acquaint myself with the A Appendix rewrites to a depth beyond what I already had as a programmer familiar with ML family languages.  

The A Appendix of the SML '97 specification contains thirty-three rewriting schemata in the form of pairs of SML fragments with constrained placeholder 'variables' which have been sorted into twelve tables by their contextual nonterminal (represented as the 'A' in many places throughout the data). The contextual nonterminal represents the root of the subtree being rewritten by the schema. Many of the rewrite schemata's right-hand-sides contained grammatical structures which necessarily match the left-hand-side of the same or a different schema; thus many rewrites are explicitly dependent on other rewrites to fully reduce a term to Bare SML.  

As a simple example, `exp orelse exp` rewrites to `if exp then true else exp`. As an `if` expression is not supported by Bare SML, _at least_ another two rewrites are needed (first to a `case` expression, then to an anonymous function) before the rewriting process is complete. If any of the `exp`s require further rewriting themselves, that number will be higher.  

This knowledge may not seem immediately relevant, but it proves useful when considering the objective of an automated rewriting algorithm. Having a recogniser check at each rewriting step costs time, so knowing beforehand that a given rewrite will _never_ yield a string in Bare SML allows the recogniser check to be skipped and the algorithm can immediately search for the next reducible expression.  

This experiment is performed largely by inspection as its primary objective was to improve familiarity with the rewrite schemata.  

## Method
Each rewrite is numbered and inspected in turn.  
For each rewrite:
- Observe the structure of the left- & right-hand sides.
- Determine if the right-hand side or any portion thereof is of the same form as the left-hand side.
	+ If so, note the rewrite as potentially cyclical and continue.
- Compare the right-hand side and all portions thereof against the left-hand sides of each other schema.
	+ Note as dependencies the numbers of the schemata which necessarily match.

Once the data has been collected, connect the rewrites into a graph based on dependency relation established. For example, if the rewrites in figure 15 are numbered 1, 2, …, 11 from top-to-bottom then the path beginning with #7 (LHS `exp andalso exp`) and ending in #4 (RHS `(fn match)(exp)`) would be `7 → 5 → 4`. Some rewrites are dependent on multiple other rewrites, so the graph will not be linear.

## Results
The results for each (ordered 1--33) along with the minimum number of rewrites for a term matching the left-hand side of the schema to be reduced to Bare SML is included below. Rewrites which are cyclical in form (that is, their right-hand side is of the form or contains a substring of the form of their left-hand side) are marked as `RECURSIVE` instead of being given a minimum count. For clarity of reading, a `---` demarcates the boundaries between figures in the SML '97 specification's A Appendix and variable instancing indices have been removed unless referenced in the minimum rewrite count.  

```
1: 1
	() -> { }

2: 1
	(exp , --- , exp) -> {1=exp , --- , n=exp}
3: 1
	# lab -> fn {lab=vid, ...} => vid
	
4: 1
	case exp of match -> (fn match)(exp)

5: 2
	if exp then exp else exp -> case exp of true => exp | false => exp -> rule 4 (1)

6: 3
	exp orelse exp -> if exp then true else exp
	if exp then true else exp -> rule 5 (2)

7: 3
	exp andalso exp -> if exp then exp else false
	if exp then exp else false -> rule 5 (2)

8: n
	(exp1; --- ; expn-1; expn) -> case exp1 of (_) => --- case expn of (_) => exp
	case exp1 of (_) => --- case expn-1 of (_) => expn -> rule 4 on each 1–n-1 (n - 1)

9: n + 1
	let dec in exp1; --- ; expn end -> let dec in (exp1; --- ; expn) end
	let dec in (exp1; --- ; expn) end -> rule 8 (n)

10: 9
	while exp do exp -> let val rec vid = fn () =>
							if exp then (exp; vid()) else ()
						in vid() end
	
	let val rec vid = fn () =>
		if exp then (exp; vid()) else ()
	in vid() end						 -> rule 5 (2; if then else),
											rule 8 (2; (exp; vid())),
											rule 1 * 4 (4)

11: 1
	[exp , --- , exp] -> exp :: --- :: exp :: nil

---

12: 1
	() -> { }

13: 1
	(pat , --- , pat) -> {1=pat , --- , n=pat}

14: 1
	[pat , --- , pat] -> pat :: --- :: pat :: nil

---

15: RECURSIVE
	vid <: ty> <as pat> <, patrow> -> vid = vid <: ty> <as pat> <, patrow>
	vid = vid <: ty> <as pat> <, patrow> -> vid = vid = vid <: ty> <as pat> <, patrow>
	---

---

16: 1
	ty * --- * ty -> {1:ty, ---, n:ty}

---

17: n + 2
	<op> vid atpat --- atpat <: ty> = exp1
	| <op> vid atpat --- atpat <: ty> = exp2
	--- ---
	| <op> vid atpat --- atpat <: ty> = expn <and fvalbind>
	->
	<op> vid = fn vid => --- fn vid =>
		case (vid, ---, vid) of
			(atpat, ---, atpat) => exp1 <: ty>
			| (atpat, ---, atpat) => exp2 <: ty>
			---
			| (atpat, ---, atpat) => expn <: ty> <and fvalbind>
													-> rule 2 (n; tuples),
													   rule 4 (1; case --- of)

---

18: 1
	fun tyvarseq fvalbind -> val tyvarseq rec fvalbind

19: 1 (2 via rule 9 if in let-in-end)
	datatype datbind withtype typbind -> datatype datbind ; type typbind

20: 1
	abstype datbind withtype typbind with dec end -> abstype datbind with type typbind; dec end

---

21: 1
	strid : sigexp = strexp <and strbind> -> strid = strexp : sigexp <and strbind>

22: 1
	strid :> sigexp = strexp <and strbind> -> strid = strexp :> sigexp <and strbind>

---

23: 1
	funid ( strdec ) -> funid ( struct strdec end )

---

24: 1
	funid (strid : sigexp): sigexp = strexp <and funbind> -> funid (strid : sigexp) = strexp : sigexp <and funbind>

25: 1
	funid (strid : sigexp) :> sigexp = strexp <and funbind> -> funid (strid : sigexp) = strexp :> sigexp <and funbind>

26: 1
	funid (spec) <: sigexp> = strexp <and funbind> -> funid (strid : sig spec end) = let open strid in strexp <: sigexp> end <and funbind>

27: 1
	funid (spec) <:> sigexp> = strexp <and funbind> -> funid (strid : sig spec end) = let open strid in strexp <:> sigexp> end <and funbind>

---

28: RECURSIVE
	exp ; <program> -> val it = exp ; <program>
	val it = exp ; <program> -> val it = val it = exp ; <program>

---

29: RECURSIVE
	type tyvarseq tycon = ty -> include sig type tyvarseq tycon end where type tyvarseq tycon = ty
	include sig type tyvarseq tycon end where type tyvarseq tycon = ty -> include sig type tyvarseq tycon end where include sig type tyvarseq tycon end where type tyvarseq tycon = ty

30: RECURSIVE
	type tyvarseq tycon = ty and --- --- and tyvarseq tycon = ty -> type tyvarseq tycon = ty type --- --- type tyvarseq tycon = ty
	type tyvarseq tycon = ty type --- --- type tyvarseq tycon = ty -> rule 29 * n (RECURSIVE)

31: 1
	include sigid --- sigid -> include sigid ; --- ; include sigid

32: 1
	spec sharing longstrid = --- = longstrid -> spec sharing type longtycon = longtycon --- sharing type longtycon = longtycon

---

33: RECURSIVE
	sigexp where type tyvarseq longtycon = ty and type --- --- and type tyvarseq longtycon = ty -> sigexp where type tyvarseq longtycon = ty where type --- --- where type tyvarseq longtycon = ty
	sigexp where type tyvarseq longtycon = ty where type --- --- where type tyvarseq longtycon = ty -> rule 29 * n (RECURSIVE)
```  

## Analysis
Of the rewrites, twenty-one (64%) have right-hand sides which are of a form admitted by Bare SML (labelled with a minimum count of `1`) and five (15%) are either immediately cyclical or dependent on a schema which is.  

## Conclusions
As over half of the available rewrites cannot immediately guarantee the lack of need for a recogniser check, further data would be required to determine if significant time savings would be attainable by use of this data. Specifically, one could analyse the frequency with which each schema's left-hand side occurs in the rewriting of a large sample set of SML source code; if the constructs which require multiple rewrites occur significantly more frequently than those which don't, that lends to a stronger argument for bypassing the recogniser check after these rewrites. Additionally, the amount of time saved by skipping the recogniser check merits further analysis to determine how often these checks would need to be skipped to net a notable time saving; these numbers can be compared with the percentages mentioned above for a more complete picture.  

The cyclical rewrites demonstrate the value of human intuition in the application of the rewriting process and show a major hurdle for an automated system to overcome. Looking at the schema `exp ; <program>` → `val it = exp ; <program>`, for instance, shows that many of these cycles can be trivial for a human to 'intuit' away (simply don't rewrite if the `exp` is being bound to something), but not so for a computer. One solution may be to use the context nonterminal to see if the rewritten right-hand side parses in Bare SML (without necessarily checking the whole input) and break the cycle at that point, but this would require further exploration by a potential implementer.  

The ultimate goal of this experiment was to improve my understanding of the rewrites and at this it greatly succeeded. Seeing how the various mechanisms were converted down to base forms added a new level of understanding to my SML programming and greatly assisted in the development and execution of further experiments during the research period. The improved intuition I gained was especially helpful in the execution of Experiment 2.  