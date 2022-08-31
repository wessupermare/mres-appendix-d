#!/bin/bash
rm  ARTGeneratedParser.*
rm  ARTGeneratedLexer.*

./artV3.sh ARTParserGrammar.art ARTChooseParseSPPF.art ARTChooseParseTWE.art !mgllGeneratorPool
./artV3CompileGenerated.sh
./artV3TestGenerated.sh $*
