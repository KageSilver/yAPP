import globals from "globals";
import pluginJs from "@eslint/js";
import tseslint from "typescript-eslint";
import pluginVue from "eslint-plugin-vue";

export default [
  {
    files: ["**/*.{js,mjs,cjs,ts,vue}"]
  },
  {
    languageOptions: {
      globals: globals.browser
    }
  },
  pluginJs.configs.recommended,
  ...tseslint.configs.recommended,
  ...pluginVue.configs["flat/essential"],
  {
    files: ["**/*.vue"],
    languageOptions: {
      parserOptions: {
        parser: tseslint.parser
      }
    }
  },
  {
    rules: {
      // **Braces and Formatting**
      "brace-style": ["error", "allman", { "allowSingleLine": false }], // Allman style brace positioning
      "no-multiple-empty-lines": ["error", { "max": 1 }], // No more than one empty line

      // **Naming Conventions**
      "camelcase": ["error", { "properties": "always" }], // Enforce camelCase for variable names
      "capitalized-comments": ["error", "always", { "ignorePattern": "pragma|ignored" }], // Capitalize comments

      // **Code Readability and Clarity**
      "no-console": "warn", // Warn when console.log is used
      "eqeqeq": ["error", "always"], // Enforce strict equality (===)
      "max-len": ["error", { "code": 90 }], // Limit line length to 80 characters for readability
      "eol-last": ["error", "always"], // Enforce newline at end of files
      "no-var": "error", // Enforce use of let/const instead of var
      "prefer-const": "error", // Prefer const for variables that are not reassigned
      "curly": ["error", "all"], // Enforce consistent use of braces for blocks (even single line)

      // **Type Declarations and Modifiers**
      "no-useless-constructor": "error", // Disallow unnecessary constructors
      "no-restricted-syntax": ["error", "WithStatement"], // Disallow use of 'with'
      "prefer-arrow-callback": "error", // Prefer arrow functions for callbacks

      // **Imports and Namespaces**
      "sort-imports": ["error", { "ignoreDeclarationSort": true }], // Enforce sorted imports

      // **Miscellaneous**
      "prefer-template": "error", // Prefer template literals over string concatenation
      "quote-props": ["error", "as-needed"], // Only quote object properties when necessary
    }
  }
];
