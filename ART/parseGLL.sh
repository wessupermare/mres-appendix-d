#!/bin/bash
rm ARTGeneratedParser.*
rm ARTGeneratedLexer.*

artV3 $1 !gllGeneratorPool
artV3CompileGenerated
artV3TestGenerated $2 $3 $4 $5 $6 $7 $8 $9 
