#!/bin/bash
rm ARTCharacterGrammar.art
rm ARTLexerGrammar.art
rm ARTParserGrammar.art
rm ARTTokenGrammar.art
rm ARTPrettyGrammar.art
rm ARTChoose*.art

$ARTHOME/artV3.sh \!grammarWrite $@
