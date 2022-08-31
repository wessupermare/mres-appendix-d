#!/bin/bash
rm  ARTGeneratedParser.*
rm  ARTGeneratedLexer.*

$ARTHOME/artV3.sh ARTLexerGrammar.art ARTChooseLexTWE.art \!gllTWEGeneratorPool
$ARTHOME/artV3CompileGenerated.sh
$ARTHOME/artV3TestGenerated.sh \!tweDump $1 $2 $3 $4 $5 $6 $7 $8 $9 
