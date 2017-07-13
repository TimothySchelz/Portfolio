Markov Name Generator
by Timothy Schelz

This is a name generator that uses a markov process to generate random names.  It can 
take in a list of names and then produce names similar to them.  The more names put in 
the better.  To use this name generator create an instance of it passing in a list of 
names.  Then to get a name out call GenerateName().

Implementation:
Name Generator has a dictionary of Letter.  Each Letter keeps track of the probability
of going to each other letter.  This creates a Markov process to, hopefully, create
names that are similar but slightly different to the input.
