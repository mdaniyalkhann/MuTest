MuTest supports a variety of mutators, which are listed below.
###### Note: Method Call Mutator is considering new mutants if there is no any other mutant at specific line of code


<!-- TOC -->
- [Arithmetic Operators](#arithmetic-operators)
- [Logical Connectors](#logical-connectors)
- [Relational Operators](#Relational-operators)
- [Boolean Literals](#boolean-literals)
- [Assignment statements](#assignment-statements)
- [Unary Operators](#unary-operators)
- [Linq Methods](#linq-methods)
- [String Literals](#string-literals)
- [Method Calls](#method-calls)
- [Statement Blocks](#statement-blocks)
<!-- /TOC -->

Default Mutators:

<!-- TOC -->
- [Arithmetic Operators](#arithmetic-operators)
- [Logical Connectors](#logical-connectors)
- [Relational Operators](#Relational-operators)
- [Unary Operators](#unary-operators)
- [Statement Blocks](#statement-blocks)
<!-- /TOC -->

## Arithmetic Operators
| Original | Mutated | 
| ------------- | ------------- | 
| `+` | `-` |
| `-` | `+` |
| `*` | `/` |
| `/` | `*` |
| `%` | `*` |

## Relational Operators
| Original | Mutated | 
| ------------- | ------------- |
| `>` | `<` |
| `>` | `>=` |
| `>=` | `<` |
| `>=` | `>` |
| `<` | `>` |
| `<` | `<=` |
| `<=` | `>` |
| `<=` | `<` |
| `==` | `!=` |
| `!=` | `==` |

## Logical Connectors
| Original | Mutated | 
| ------------- | ------------- | 
| `&&` | `\|\|` | 
| `\|\|` | `&&` |

## Boolean Literals
| Original | Mutated | 
| ------------- | ------------- | 
| `true`	| `false` |
| `false`	| `true` |
| `!person.IsAdult()`		| `person.IsAdult()` |
| `if(person.IsAdult())` | `if(!person.IsAdult())` |
| `while(person.IsAdult())` | `while(!person.IsAdult())` |

## Assignment Statements
| Original | Mutated | 
| ------------- | ------------- | 
|`+= `	| `-= ` |
|`-= `	| `+= ` |
|`*= `	| `/= ` |
|`/= `	| `*= ` |
|`%= `	| `*= ` |
|`<<=`  | `>>=` |
|`>>=`  | `<<=` |
|`&= `	| `\|= ` |
|`\|= `	| `&= ` |

## Unary Operators
|    Original   |   Mutated  | 
| ------------- | ---------- | 
| `-variable`	| `+variable`|
| `+variable` 	| `-variable`|
| `~variable` 	| `variable` |
| `variable++`	| `variable--` |
| `variable--`	| `variable++` |
| `++variable`	| `--variable` |
| `--variable`	| `++variable` |


## Linq Methods
|      Original         |       Mutated         |
| --------------------- | --------------------- |
| `SingleOrDefault()`   | `FirstOrDefault()`    |
| `FirstOrDefault()`    | `SingleOrDefault()`   |
| `First()`             | `Last()`              |
| `Last()`              | `First()`             |
| `All()`               | `Any()`               |
| `Any()`               | `All()`               |
| `Skip()`              | `Take()`              |
| `Take()`              | `Skip()`              |
| `SkipWhile()`         | `TakeWhile()`         |
| `TakeWhile()`         | `SkipWhile()`         |
| `Min()`               | `Max()`               |
| `Max()`               | `Min()`               |
| `Sum()`               | `Count()`             |
| `Count()`             | `Sum()`               |

## String Literals
| Original | Mutated |
| ------------- | ------------- | 
| `"foo"` | `""` |
|  `""` | `"MuTest"` |
| `$"foo {bar}"` | `$""` |
| `@"foo"` | `@""` |

## Method Calls
| Original | Mutated |
| ------------- | ------------- |
| `ActionOne();` | `;` |
| `OpenConnection();` | `;` |

## Statement Blocks
| Original | Mutated |
| ------------- | ------------- |
| `void Method() { .... }` | `void Method() { }` |
| `if (condition > 0 ) { .... }` | `if (condition > 0 ) { }` |
