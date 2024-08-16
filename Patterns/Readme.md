# Readme

To help following developers getting started with the project every project should have a README.md at the root. 
It should describe what the project is, how to get it running locally, and what tool dependencies it has. If the project has unusual requirements or quirks, put them in the README.md. Ideally, a developer should be able to get started with the project with just the information in there.

If you encounter a project that has no README.md, you should make one right away.
A good start would be filling in the template below. It also contains some examples on how to generate diagrams in markdown.

``` markdown
# Project Title

## Overview

At the top of the file there should be a short introduction and/ or overview that explains what the project is.

* Explanation of the application and its purpose. Answer the questions:
  * What is the motivation behind the creation of the project? This should explain why the project exists.
  * Why is this project relevant to the user?
  * What purpose does it serve?

## What does it do?
If relevant, provide a more detailed explanation of what the the project/app/library does.

## How do I use it?

### Requirements/Pre-requisites
List any requirements for using this project, like dependencies and version. Remove this section if it is not needed.

### How to run the project locally
Include instructions on how to run the project on a user's local machine. Be sure to reference the technologies they might have to download for the application to run.

1. Step 1

2. Step 2
  * Substep (a)
  * Substep (b)
3. Step 3

` ` ` text (remove spaces)
Your code here
` ` `

### Project Structure

Use the below for Markdown formatting syntax:

#### Sub Section

> This is a blockquote.
>
> This is the second paragraph in the blockquote.

* list item #1
* list item #2

1. ordered item #1
2. ordered item #2

*italic text*
**bold text**
[links](https://www.urlgoeshere.nl)

```

## Markdown graphs

By using text-to-graph features of built in various markdown tools, it is possible to include complex diagrams and schemes directly in markdown and in the repo. Various tools exist:

### Mermaid

See https://mermaid.js.org/

``` mermaid
graph TD
    A[Have Complex Stuff] --> B(Use)
    B --> C{decide}
    C --> D[Graphs]
    C --> E[Graphs]
    D --> B
    E --> B
```

``` mermaid
sequenceDiagram
    Alice->>John: Hello John, how are you?
    John-->>Alice: Great!
    Alice-)John: See you later!
```

### Ascii flow

Use https://asciiflow.com

``` goat
      ┌──────────────────────┐
      │                      │
      │                      │   ┌──────┐
      │                      │   │      │
┌─────┼────┐          ┌──────┼───┼─┐    │
│     │    │          │      └───┘ │    │
│          ├─────────►│            │    │
│  Idea    │          │  Graph     │    │
│          │          │         ◄──┼────┘
│          │          │            │
└────┬─────┘          └─▲──────────┘
     │                  │
     │                  │
     │                  │
     └──────────────────┘
```
