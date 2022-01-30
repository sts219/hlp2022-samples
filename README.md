# hlp2022 TBL2 Code

## Intro

The purpose of the sample code in `labels` is to understand the difference between *good* and *bad* FP code.

The code there was taken from Issie (working) but is of very poor quality. It illustrates some of teh typical mistakes that can be made when writing FP code.

The purpose of this TBL is to get practice applying the [Issie Coding Guidelines](https://github.com/tomcl/issie/wiki/Coding-guidelines-for-ISSIE) to existing code (which you will have to do in project work).

The code here can maybe be run - but the purpose of this TBL is to study the source code without running it.

Method:

1. Fork and clone this repo. Go to labels solution, `labels.fs`.
1. Trace down from top-level function drawing a tree representing which functions are called by each function (you don't need to include library functions or subfunctions). The root of this tree is the top-level function. You do not need to consider any code other than that which appears in your call tree. Subfunvtions local to a function can get analysed with the function.
2. For each function (and its local subfunctions) apply the Issie coding standards considering:
    * Function name
    * parameter names
    * local names
    * choice of named vs anonymous subfunction
    * Is this function useful - or does it just make code less readable?
    * Is code documentation needed (and is that only because of bad code/names/etc?)

This is not an exclusive list - you may find code issues in addition to those mechanically obtained as above.

Document your results per team, and any questions you have where you can't decide. Lits issues per function + any overall issues.
Correct names (where possible) this is the easiest refactoring
Make other code improvements if you can (you probably won't have time in the TBL). Suggest nature of improvement in your documentation if you like.

E-mail me your results. I'm ok with whatever file type (markdown / word/ pdf) you prefer.