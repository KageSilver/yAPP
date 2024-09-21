# C\# Coding Conventions

===============

This document is inspired by [.NET's coding style guidelines](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md).

## Braces and Formatting

- **Brackets**: 
	- We use [Allman style](http://en.wikipedia.org/wiki/Indent_style#Allman_style) braces, where each brace begins on a new line. This rule applies for all cases, including single-line if-statements, for the sake of consistency.
   
- **Indentation**:  
	- We use tabs for indentation (i.e., four spaces).

- **Line Spacing**:  
	- Avoid more than one empty line at any time.

- **Trailing Spaces**:  
	 - Avoid spurious/trailing white spaces.

## Naming Conventions

- **Field Naming**:  
	- Use `_camelCase` for internal and private fields and `readonly` where possible.  
	- Prefixes
		- Internal and private instance fields: `_`
		- Static fields: `s_`
		- Thread-static fields: `t_`
	- Public fields use PascalCasing with no prefix (used sparingly).
	- Use names that sounds close to regular English rather than using `modifier-subject` or `subject-modifier` standards. 
		- e.g., `TotalArea` instead of `AreaTotal`, but `WallCount` instead of `CountWall`
	- If you start needing to differentiate fields by a prefix, consider splitting that prefix off into its own entity somehow.
		- e.g., `AreaInnerSurface` and `AreaOuterSurface` turns into `Area` as an object, where we can use `Area[INNER]`.

- **Constant Naming**:  
	- Use PascalCasing to name all constant local variables and fields, with an exception for interop code (i.e., code written in different programming languages) where the name should match exactly.

- **Method Naming**:  
	- Use PascalCasing for all method names, including local functions.
	- Method names should start with a present-tense verb.
		- e.g., `getUsers()`, `saveDraft()`

- **Type Keywords**:  
	- Use language keywords instead of BCL types for type references and method calls.
		- e.g., `int, string, float` instead of `Int32, String, Single`, etc.

## Code Readability and Clarity

-  **Visibility**:  
	- Visibility should *always* be explicitly stated and listed as the first modifier.

- **`this.` Keyword**:  
	- Avoid using `this.` unless absolutely necessary.

- **Field Declaration Placement**:  
	- Fields should be declared at the top within type declarations.

- **Labels and `goto` Statements**:  
	- When using labels for `goto`, indent the label one level less than the current indentation.

- **Ordering**:
	- Static Variables -> Static Methods // neither object nor behaviour
	- Public Variables -> Protected Variables -> Constructors // define object
	- Public Methods -> Protected Methods -> Private Methods // define behaviour

## Type Declarations and Modifiers

- **Use of `var`**:  
	- Use `var` only when the type is explicitly named on the right-hand side 
		- e.g., `var stream = new FileStream(...)`
	- Target-typed `new()` can only be used when the type is explicitly named on the left-hand side 
		- e.g., `FileStream stream = new(...);`

- **Use of `readonly`**:  
	- `readonly` should follow `static` when used on static fields   
    	- e.g., `static readonly`, not `readonly static`

- **Static/Sealed Modifiers**:  
	- Make internal and private types static or sealed unless derivation is required.

## Imports and Namespaces

- **Import Placement**:  
	- Namespace imports should be specified at the top of the file, outside `namespace` declarations.
	- Imports should be sorted alphabetically, except `System.*` namespaces, which should appear at the top.

## Miscellaneous

- **`nameof` Usage**:  
	- Use `nameof(…)` instead of string literals `"..."` whenever relevant.

- **Non-ASCII Characters**:  
	- Use Unicode escape sequences (`\uXXXX`) instead of literal non-ASCII characters in the source code to avoid potential issues with the editor.

---

For other languages, we aim to adhere to the same code guidelines above as best as possible. 